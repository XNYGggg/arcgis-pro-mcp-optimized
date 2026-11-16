using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ArcGISProMCP.Bridge
{
    /// <summary>Which serial execution resource a command needs.</summary>
    internal enum ExecutionLane
    {
        /// <summary>Answered on the request thread; never queued.</summary>
        Inline,

        /// <summary>ArcGIS Pro's main CIM thread: the project and map object model.</summary>
        Project,

        /// <summary>The geoprocessing engine, which runs one tool at a time.</summary>
        Geoprocessing,

        /// <summary>The Python bridge on 6511, which answers one command at a time.</summary>
        Python,
    }

    /// <summary>
    /// Bounded queues in front of the three serial parts of ArcGIS Pro.
    ///
    /// Every MCP client used to race straight at the MCT, the GP engine or the
    /// Python bridge. Those are serial resources, so concurrent calls piled up
    /// out of order, one slow tool looked exactly like a dead bridge, and there
    /// was no way to see who was waiting. Each resource now has its own lane:
    ///
    ///   * lanes run independently -- a long geoprocessing run does not stall
    ///     quick map reads or Python calls;
    ///   * each lane is strictly serial and admits a bounded number of live
    ///     queued jobs, so congestion surfaces as a clear "lane busy" error
    ///     instead of an endless backlog;
    ///   * cheap read commands jump ahead of heavy writes inside the project
    ///     lane, so status queries stay responsive while edits are queued;
    ///   * a supervised worker restarts if it ever faults -- a dead lane would
    ///     otherwise look exactly like endless congestion.
    ///
    /// get_queue_status exposes the state of every lane.
    /// </summary>
    internal static class JobScheduler
    {
        public sealed class LaneBusyException : Exception
        {
            public LaneBusyException(string message) : base(message) { }
        }

        public sealed class QueueTimeoutException : Exception
        {
            public QueueTimeoutException(string message) : base(message) { }
        }

        private sealed class Job
        {
            public string Command;
            public Func<Task<object>> Work;
            public readonly TaskCompletionSource<object> Completion =
                new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            public readonly Stopwatch Waited = Stopwatch.StartNew();
            public CancellationTokenSource Expiry;
            public double WaitMs;

            // 0 = queued, 1 = running, 2 = expired. Interlocked CAS on this
            // field is what keeps the expiry timer and the lane worker from
            // both acting on the same job.
            public int State;
        }

        private sealed class Lane
        {
            public readonly string Name;
            public readonly int MaxQueued;
            public readonly TimeSpan QueueWait;

            private readonly Channel<Job> _normal;
            private readonly Channel<Job> _high;
            private int _consecutiveHigh;

            // Live queued jobs, across both channels. Admission is counted
            // here rather than by channel capacity: an expired job cannot be
            // removed from a channel, but it must not keep occupying a slot.
            private int _liveQueued;

            // At most one pending wait per channel. Abandoning a fresh
            // WaitToReadAsync every loop would pile up uncompleted waiters.
            private Task<bool> _highWait;
            private Task<bool> _normalWait;

            private sealed class RunningInfo
            {
                public string Command;
                public long StartedTimestamp;
                public double WaitMs;
            }

            // Published as one immutable snapshot so Describe() never pairs a
            // command with another job's start time.
            private volatile RunningInfo _running;
            private long _completed;
            private long _failed;
            private long _expired;
            private long _rejected;
            private long _workerRestarts;

            public Lane(string name, int maxQueued, TimeSpan queueWait)
            {
                Name = name;
                MaxQueued = maxQueued;
                QueueWait = queueWait;
                var options = new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false,
                };
                _normal = Channel.CreateUnbounded<Job>(options);
                _high = Channel.CreateUnbounded<Job>(options);
                _ = Task.Run(SuperviseWorkerAsync);
            }

            public Task<object> Enqueue(Job job, bool highPriority)
            {
                if (Interlocked.Increment(ref _liveQueued) > MaxQueued)
                {
                    Interlocked.Decrement(ref _liveQueued);
                    Interlocked.Increment(ref _rejected);
                    throw new LaneBusyException(
                        $"The {Name} lane is busy: all {MaxQueued} queue slots are taken. "
                        + "Retry shortly, or check get_queue_status for what is running.");
                }

                // The expiry timer exists before the job enters a queue, so the
                // worker always has a CTS to cancel and the two paths can never
                // race on a missing field.
                job.Expiry = new CancellationTokenSource();
                var token = job.Expiry.Token;
                _ = Task.Delay(QueueWait, token).ContinueWith(_ =>
                {
                    if (token.IsCancellationRequested) return;
                    // Only a job still waiting may time out; one that started
                    // running is left alone no matter how long it then takes.
                    if (Interlocked.CompareExchange(ref job.State, 2, 0) != 0) return;
                    Interlocked.Decrement(ref _liveQueued);
                    if (job.Completion.TrySetException(new QueueTimeoutException(
                            $"The {Name} lane stayed busy for {QueueWait.TotalSeconds:0}s while "
                            + $"'{job.Command}' waited in the queue, so it was not run. "
                            + "Check get_queue_status and retry.")))
                    {
                        Interlocked.Increment(ref _expired);
                    }
                }, TaskScheduler.Default);

                var writer = highPriority ? _high.Writer : _normal.Writer;
                if (!writer.TryWrite(job))
                {
                    // Unbounded channel writes only fail once the channel is
                    // completed; release the slot rather than leaking it.
                    if (Interlocked.CompareExchange(ref job.State, 2, 0) == 0)
                        Interlocked.Decrement(ref _liveQueued);
                    try { job.Expiry.Cancel(); } catch { /* already fired */ }
                    Interlocked.Increment(ref _rejected);
                    throw new LaneBusyException(
                        $"The {Name} lane is closing and refused the command.");
                }

                return job.Completion.Task;
            }

            private async Task<Job> NextJobAsync()
            {
                while (true)
                {
                    Job job;
                    // Serve bursts of quick reads, but never let them starve writes.
                    if (_consecutiveHigh < 4 && _high.Reader.TryRead(out job))
                    {
                        _consecutiveHigh++;
                        return job;
                    }
                    if (_normal.Reader.TryRead(out job))
                    {
                        _consecutiveHigh = 0;
                        return job;
                    }
                    if (_high.Reader.TryRead(out job))
                    {
                        _consecutiveHigh++;
                        return job;
                    }

                    _highWait ??= _high.Reader.WaitToReadAsync().AsTask();
                    _normalWait ??= _normal.Reader.WaitToReadAsync().AsTask();
                    var done = await Task.WhenAny(_highWait, _normalWait)
                                         .ConfigureAwait(false);
                    if (done == _highWait) _highWait = null;
                    else _normalWait = null;
                }
            }

            private async Task SuperviseWorkerAsync()
            {
                while (true)
                {
                    try
                    {
                        await WorkerAsync().ConfigureAwait(false);
                        return;   // workers run until the process ends
                    }
                    catch
                    {
                        // A dead worker looks identical to endless congestion:
                        // every later command sits in the queue and expires.
                        // Restart it instead of losing the lane.
                        Interlocked.Increment(ref _workerRestarts);
                        await Task.Delay(500).ConfigureAwait(false);
                    }
                }
            }

            private async Task WorkerAsync()
            {
                while (true)
                {
                    Job job;
                    try
                    {
                        job = await NextJobAsync().ConfigureAwait(false);
                    }
                    catch
                    {
                        // Never let a transient failure kill the lane.
                        await Task.Delay(250).ConfigureAwait(false);
                        continue;
                    }

                    // Claim the job: an expired one has already released its
                    // slot, and one that starts is immune to its queue timer
                    // from this point on.
                    if (Interlocked.CompareExchange(ref job.State, 1, 0) != 0)
                        continue;
                    Interlocked.Decrement(ref _liveQueued);

                    try { job.Expiry.Cancel(); } catch { /* timer already fired */ }
                    job.WaitMs = job.Waited.Elapsed.TotalMilliseconds;
                    _running = new RunningInfo
                    {
                        Command = job.Command,
                        StartedTimestamp = Stopwatch.GetTimestamp(),
                        WaitMs = job.WaitMs,
                    };

                    var runWatch = Stopwatch.StartNew();
                    try
                    {
                        var result = await job.Work().ConfigureAwait(false);
                        job.Completion.TrySetResult(result);
                        Interlocked.Increment(ref _completed);
                        RecordHistory(Name, job, runWatch.Elapsed, true, null);
                    }
                    catch (Exception exception)
                    {
                        job.Completion.TrySetException(exception);
                        Interlocked.Increment(ref _failed);
                        RecordHistory(Name, job, runWatch.Elapsed, false,
                            $"{exception.GetType().Name}: {exception.Message}");
                    }
                    finally
                    {
                        _running = null;
                    }
                }
            }

            public Dictionary<string, object> Describe()
            {
                var data = new Dictionary<string, object>
                {
                    ["queued"] = Math.Max(0, Volatile.Read(ref _liveQueued)),
                    ["max_queued"] = MaxQueued,
                    ["queue_wait_timeout_seconds"] = (int)QueueWait.TotalSeconds,
                    ["completed"] = Interlocked.Read(ref _completed),
                    ["failed"] = Interlocked.Read(ref _failed),
                    ["expired_while_queued"] = Interlocked.Read(ref _expired),
                    ["rejected_while_full"] = Interlocked.Read(ref _rejected),
                    ["worker_restarts"] = Interlocked.Read(ref _workerRestarts),
                };

                var running = _running;
                if (running != null)
                {
                    data["running"] = new Dictionary<string, object>
                    {
                        ["command"] = running.Command,
                        ["running_for_seconds"] = Math.Round(
                            (Stopwatch.GetTimestamp() - running.StartedTimestamp)
                            / (double)Stopwatch.Frequency, 2),
                        ["waited_in_queue_seconds"] = Math.Round(running.WaitMs / 1000.0, 2),
                    };
                }
                return data;
            }
        }

        private static readonly Lane Project = new Lane("project", 32, TimeSpan.FromSeconds(180));
        private static readonly Lane Geoprocessing = new Lane("geoprocessing", 16, TimeSpan.FromSeconds(900));
        private static readonly Lane Python = new Lane("python", 32, TimeSpan.FromSeconds(300));

        private static readonly ConcurrentQueue<Dictionary<string, object>> History =
            new ConcurrentQueue<Dictionary<string, object>>();
        private const int HistoryLimit = 100;

        /// <summary>
        /// Commands that only read a little of the object model. They are
        /// answered before queued writes so status calls stay responsive
        /// while edits wait behind a slow operation.
        /// </summary>
        private static readonly HashSet<string> QuickReads =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "get_layers", "get_layer_info", "get_project_info", "get_arcgis_info",
            "list_maps", "list_layouts", "list_fields", "list_bookmarks",
            "get_map_view", "get_map_extent", "get_selection", "get_environment",
            "get_messages", "get_layer_symbology", "get_layout_info", "diagnose",
        };

        public static bool IsQuickRead(string command) => QuickReads.Contains(command);

        /// <summary>
        /// Queue work on a lane and return its result. Throws LaneBusyException
        /// when the lane's queue is full and QueueTimeoutException when the
        /// wait in the queue outlives the lane's allowance.
        /// </summary>
        public static Task<object> RunAsync(ExecutionLane lane, string command,
                                            bool highPriority, Func<Task<object>> work)
        {
            var state = lane switch
            {
                ExecutionLane.Project => Project,
                ExecutionLane.Geoprocessing => Geoprocessing,
                ExecutionLane.Python => Python,
                _ => throw new ArgumentException(
                    "Inline commands run on the request thread and are not queued.", nameof(lane)),
            };
            return state.Enqueue(new Job { Command = command, Work = work }, highPriority);
        }

        public static object Describe()
        {
            return new Dictionary<string, object>
            {
                ["lanes"] = new Dictionary<string, object>
                {
                    ["project"] = Project.Describe(),
                    ["geoprocessing"] = Geoprocessing.Describe(),
                    ["python"] = Python.Describe(),
                },
                ["recent"] = History.ToArray().Reverse().Take(20).ToList(),
                ["quick_read_priority"] =
                    QuickReads.OrderBy(n => n, StringComparer.Ordinal).ToList(),
            };
        }

        private static void RecordHistory(string lane, Job job, TimeSpan ran,
                                          bool ok, string error)
        {
            History.Enqueue(new Dictionary<string, object>
            {
                ["command"] = job.Command,
                ["lane"] = lane,
                ["waited_ms"] = Math.Round(job.WaitMs, 0),
                ["ran_ms"] = Math.Round(ran.TotalMilliseconds, 0),
                ["status"] = ok ? "ok" : "failed",
                ["error"] = error,
                ["at"] = DateTime.Now.ToString("HH:mm:ss"),
            });
            while (History.Count > HistoryLimit && History.TryDequeue(out _)) { }
        }
    }
}
