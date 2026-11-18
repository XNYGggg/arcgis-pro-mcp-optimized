using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGISProMCP.Bridge;

namespace ArcGISProMCP.Commands
{
    /// <summary>
    /// Running geoprocessing tools, so the whole toolbox stays reachable
    /// without arcpy.
    ///
    /// ArcGIS Pro's API takes parameter values by position and offers no way
    /// to ask a tool what its parameters are called. arcpy does know, so the
    /// ordering is dumped once by scripts/dump_gp_parameters.py and embedded
    /// here -- that is what lets these commands accept named parameters.
    /// </summary>
    internal static class GeoprocessingCommands
    {
        private const string Group = "geoprocessing";
        private const string ParameterResource = "ArcGISProMCP.Resources.gp-parameters.json";
        private const string DetailResource = "ArcGISProMCP.Resources.gp-parameter-details.json";

        private static readonly Lazy<Dictionary<string, string[]>> ParameterOrder =
            new Lazy<Dictionary<string, string[]>>(LoadParameterOrder);

        private static readonly Lazy<JsonDocument> ParameterDetails =
            new Lazy<JsonDocument>(LoadParameterDetails);

        public static void Register()
        {
            CommandRouter.RegisterAsync("run_geoprocessing_tool", Group,
                "Run any geoprocessing tool, by named or positional parameters.",
                RunToolAsync, lane: ExecutionLane.Geoprocessing);
            CommandRouter.Register("list_geoprocessing_tools", Group,
                "Search the available geoprocessing tools by name.", ListTools,
                runOnMct: false);
            CommandRouter.Register("describe_geoprocessing_tool", Group,
                "A tool's parameter names, in the order it takes them.",
                DescribeTool, runOnMct: false);
            CommandRouter.Register("list_toolboxes", Group,
                "List the toolbox aliases tools are grouped under.", ListToolboxes,
                runOnMct: false);
            CommandRouter.RegisterAsync("run_python_toolbox_tool", Group,
                "Run a tool from a .pyt, .atbx or .tbx toolbox on disk.",
                RunToolboxToolAsync, lane: ExecutionLane.Geoprocessing);
            CommandRouter.Register("check_extension", Group,
                "Check an ArcGIS extension licence.", CheckExtension, runOnMct: false);
            CommandRouter.Register("get_messages", Group,
                "Messages from the most recent geoprocessing run.", GetMessages,
                runOnMct: false);
        }

        // --- the parameter table ---------------------------------------------

        private static Dictionary<string, string[]> LoadParameterOrder()
        {
            var table = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var assembly = typeof(GeoprocessingCommands).Assembly;
                using (var stream = assembly.GetManifestResourceStream(ParameterResource))
                {
                    if (stream == null) return table;
                    using (var reader = new StreamReader(stream))
                    using (var document = JsonDocument.Parse(reader.ReadToEnd()))
                    {
                        foreach (var entry in document.RootElement.EnumerateObject())
                        {
                            table[entry.Name] = entry.Value.EnumerateArray()
                                .Select(v => v.GetString()).ToArray();
                        }
                    }
                }
            }
            catch
            {
                // Without the table only positional calls work, which is still
                // better than failing to load.
            }
            return table;
        }

        private static string[] OrderFor(string toolName)
        {
            var table = ParameterOrder.Value;
            if (table.TryGetValue(toolName, out var order)) return order;

            // Accept either "analysis.Buffer" or "Buffer_analysis".
            if (toolName.Contains("."))
            {
                var parts = toolName.Split('.');
                var flipped = $"{parts[parts.Length - 1]}_{parts[parts.Length - 2]}";
                if (table.TryGetValue(flipped, out order)) return order;
            }
            else if (toolName.Contains("_"))
            {
                var index = toolName.LastIndexOf('_');
                var flipped = $"{toolName.Substring(index + 1)}.{toolName.Substring(0, index)}";
                if (table.TryGetValue(flipped, out order)) return order;
            }
            return null;
        }

        // --- parameter details (types, enums, defaults) ----------------------

        /// <summary>
        /// Types, value-list enumerations and defaults captured from arcpy at
        /// build time. Optional: without the resource (or with a damaged one)
        /// describe_geoprocessing_tool still answers with names and order.
        /// </summary>
        private static JsonDocument LoadParameterDetails()
        {
            try
            {
                var assembly = typeof(GeoprocessingCommands).Assembly;
                using var stream = assembly.GetManifestResourceStream(DetailResource);
                if (stream == null) return null;
                using var reader = new StreamReader(stream);
                return JsonDocument.Parse(reader.ReadToEnd());
            }
            catch
            {
                return null;
            }
        }

        private static JsonElement? DetailFor(string toolName)
        {
            var document = ParameterDetails.Value;
            if (document == null) return null;
            if (!document.RootElement.TryGetProperty("tools", out var tools)
                || tools.ValueKind != JsonValueKind.Object)
                return null;

            if (tools.TryGetProperty(toolName, out var detail)) return detail;
            if (toolName.Contains("."))
            {
                var parts = toolName.Split('.');
                var flipped = $"{parts[parts.Length - 1]}_{parts[parts.Length - 2]}";
                if (tools.TryGetProperty(flipped, out detail)) return detail;
            }
            else if (toolName.Contains("_"))
            {
                var index = toolName.LastIndexOf('_');
                var flipped = $"{toolName.Substring(index + 1)}.{toolName.Substring(0, index)}";
                if (tools.TryGetProperty(flipped, out detail)) return detail;
            }
            return null;
        }

        // --- environments ----------------------------------------------------

        /// <summary>
        /// The environment list handed to every tool run. Keys are mapped to
        /// their GP names through MapCommands.GpEnvironmentNames, so a key the
        /// bridge does not know is never silently forwarded as something else.
        /// Per-call values override the stored ones.
        /// </summary>
        private static IReadOnlyList<KeyValuePair<string, string>> BuildEnvironment(
            IDictionary<string, object> extra)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in MapCommands.Environment)
            {
                if (!MapCommands.GpEnvironmentNames.TryGetValue(pair.Key, out var gpName)) continue;
                var text = EnvironmentText(pair.Value);
                if (text != null) values[gpName] = text;
            }
            if (extra != null)
            {
                foreach (var pair in extra)
                {
                    if (!MapCommands.GpEnvironmentNames.TryGetValue(pair.Key, out var gpName))
                        continue;
                    var text = EnvironmentText(pair.Value);
                    if (text != null) values[gpName] = text;
                }
            }
            return values.Count == 0
                ? null
                : values.Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value))
                        .ToList();
        }

        private static string EnvironmentText(object value)
        {
            switch (value)
            {
                case null: return null;
                case bool flag: return flag ? "true" : "false";
                case double number:
                    return number.ToString("R", CultureInfo.InvariantCulture);
                case float number:
                    return number.ToString("R", CultureInfo.InvariantCulture);
                case int number:
                    return number.ToString(CultureInfo.InvariantCulture);
                default:
                    return Convert.ToString(value, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>The per-call "environment" object, or null when absent.</summary>
        private static Dictionary<string, object> ReadPerCallEnvironment(Params parameters)
        {
            var raw = parameters.GetObject("environment");
            if (raw == null || raw.Count == 0) return null;

            var values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in raw)
            {
                values[pair.Key] = pair.Value.ValueKind switch
                {
                    JsonValueKind.Object => MapCommands.EnvironmentExtentText(pair.Value)
                                            ?? pair.Value.GetRawText(),
                    _ => ValueOf(pair.Value),
                };
            }
            return values;
        }

        // --- running ---------------------------------------------------------

        private static object ValueOf(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String: return element.GetString();
                case JsonValueKind.Number: return element.GetDouble();
                case JsonValueKind.True: return true;
                case JsonValueKind.False: return false;
                case JsonValueKind.Null: return null;
                case JsonValueKind.Array:
                    return string.Join(";", element.EnumerateArray().Select(ValueOf));
                default: return element.GetRawText();
            }
        }

        private static async Task<object> RunToolAsync(Params parameters)
        {
            var toolName = parameters.Require("tool_name");
            var values = new List<object>();
            var usedNames = new List<string>();

            var positional = parameters.Raw("args");
            if (positional.ValueKind == JsonValueKind.Array)
            {
                values.AddRange(positional.EnumerateArray().Select(ValueOf));
            }
            else
            {
                var named = parameters.GetObject("parameters");
                if (named == null)
                    throw new ArgumentException(
                        "Provide parameters (named) or args (positional).");

                var order = OrderFor(toolName);
                if (order == null)
                    throw new ArgumentException(
                        $"The parameter order for '{toolName}' is not known, so named "
                        + "parameters cannot be placed. Pass args as a positional list, "
                        + "or check the name with describe_geoprocessing_tool.");

                var unknown = named.Keys.Where(
                    key => !order.Contains(key, StringComparer.OrdinalIgnoreCase)).ToList();
                if (unknown.Count > 0)
                    throw new ArgumentException(
                        $"{toolName} has no parameter(s) named {string.Join(", ", unknown)}. "
                        + $"It takes: {string.Join(", ", order)}");

                // Positional values, trimmed after the last one actually given.
                var slots = new object[order.Length];
                var lastUsed = -1;
                for (var i = 0; i < order.Length; i++)
                {
                    var match = named.Keys.FirstOrDefault(
                        key => string.Equals(key, order[i], StringComparison.OrdinalIgnoreCase));
                    if (match == null) continue;
                    slots[i] = ValueOf(named[match]);
                    usedNames.Add(order[i]);
                    lastUsed = i;
                }
                values.AddRange(slots.Take(lastUsed + 1));
            }

            var flags = FlagsFor(parameters.GetBool("add_to_map"));

            var extraEnvironment = ReadPerCallEnvironment(parameters);
            if (parameters.Has("overwrite"))
            {
                extraEnvironment ??= new Dictionary<string, object>(
                    StringComparer.OrdinalIgnoreCase);
                extraEnvironment["overwriteOutput"] = parameters.GetBool("overwrite");
            }
            var environments = BuildEnvironment(extraEnvironment);

            var started = DateTime.Now;
            var result = await Geoprocessing.ExecuteToolAsync(
                toolName, Geoprocessing.MakeValueArray(values.ToArray()),
                environments, null, null, flags).ConfigureAwait(false);
            LastResult = result;

            if (result.IsFailed)
            {
                var messages = result.ErrorMessages != null && result.ErrorMessages.Any()
                    ? string.Join("; ", result.ErrorMessages.Select(m => m.Text))
                    : "the tool reported no error text";
                throw new InvalidOperationException($"{toolName} failed: {messages}");
            }

            var data = new Dictionary<string, object>
            {
                ["tool"] = toolName,
                ["outputs"] = result.Values?.ToList(),
                ["return_value"] = result.ReturnValue,
                ["elapsed_seconds"] = Math.Round((DateTime.Now - started).TotalSeconds, 2),
                ["messages"] = result.Messages?.Select(m => m.Text).Take(50).ToList(),
            };
            if (usedNames.Count > 0) data["parameters_used"] = usedNames;
            return data;
        }

        /// <summary>
        /// A tool in a toolbox on disk is addressed by the toolbox path plus
        /// the tool name. No parameter-order table exists for one, so the
        /// values have to come in positionally.
        /// </summary>
        private static async Task<object> RunToolboxToolAsync(Params parameters)
        {
            var toolboxPath = parameters.Require("toolbox_path");
            var toolName = parameters.Require("tool_name");

            if (!File.Exists(toolboxPath) && !Directory.Exists(toolboxPath))
            {
                // Keep the original sentence (clients and logs match on it)
                // and append where the path probably meant to point. A typo
                // in one directory segment is the usual cause.
                var message = $"No toolbox at {toolboxPath}";
                var suggestion = SuggestSimilarPath(toolboxPath);
                if (!string.IsNullOrEmpty(suggestion))
                    message += $". Did you mean {suggestion}?";
                message += " Pass the full path to a .pyt, .atbx or .tbx toolbox.";
                throw new ArgumentException(message);
            }

            var positional = parameters.Raw("args");
            var values = new List<object>();
            if (positional.ValueKind == JsonValueKind.Array)
            {
                values.AddRange(positional.EnumerateArray().Select(ValueOf));
            }
            else
            {
                var named = parameters.GetObject("parameters");
                if (named != null && named.Count > 0)
                    // A custom toolbox publishes no parameter order this add-in
                    // can read, so named values would have to be guessed into
                    // slots. Saying so beats putting them in the wrong ones.
                    throw new ArgumentException(
                        "Tools from a toolbox on disk take positional values: pass "
                        + "args as a list in the tool's own parameter order. Named "
                        + "parameters work only for the built-in tools, whose order "
                        + "is known. Example: toolbox_path=\"" + toolboxPath + "\", "
                        + "tool_name=\"" + toolName + "\", "
                        + "args=[\"<first parameter>\", \"<second parameter>\"]. "
                        + "The script file goes first in args for run-code style "
                        + "toolboxes.");
            }

            var started = DateTime.Now;
            var result = await Geoprocessing.ExecuteToolAsync(
                Path.Combine(toolboxPath, toolName),
                Geoprocessing.MakeValueArray(values.ToArray()),
                BuildEnvironment(ReadPerCallEnvironment(parameters)), null, null,
                FlagsFor(parameters.GetBool("add_to_map"))).ConfigureAwait(false);
            LastResult = result;

            if (result.IsFailed)
            {
                var messages = result.ErrorMessages != null && result.ErrorMessages.Any()
                    ? string.Join("; ", result.ErrorMessages.Select(m => m.Text))
                    : "the tool reported no error text";
                throw new InvalidOperationException($"{toolName} failed: {messages}");
            }

            return new Dictionary<string, object>
            {
                ["toolbox"] = toolboxPath,
                ["tool"] = toolName,
                ["outputs"] = result.Values?.ToList(),
                ["return_value"] = result.ReturnValue,
                ["elapsed_seconds"] = Math.Round((DateTime.Now - started).TotalSeconds, 2),
                ["messages"] = result.Messages?.Select(m => m.Text).Take(50).ToList(),
            };
        }

        /// <summary>
        /// Which flags a run needs. GPExecuteToolFlags.Default already includes
        /// AddOutputsToMap -- "adds outputs to map and refreshes project items"
        /// -- so ORing AddOutputsToMap onto it does nothing, and add_to_map:false
        /// silently added the output anyway. Every intermediate result a
        /// workflow produced ended up in the table of contents because of it.
        /// </summary>
        private static GPExecuteToolFlags FlagsFor(bool addToMap)
        {
            return addToMap
                ? GPExecuteToolFlags.Default
                : GPExecuteToolFlags.RefreshProjectItems | GPExecuteToolFlags.AddToHistory;
        }

        private static IGPResult LastResult;

        /// <summary>
        /// Run a tool with named parameters, placed using the embedded order.
        /// Shared with the commands that are a single tool run under a
        /// friendlier name, so none of them hand-builds a positional array.
        /// </summary>
        public static async Task<IGPResult> RunNamedAsync(
            string toolName, IDictionary<string, object> named, bool addToMap)
        {
            var order = OrderFor(toolName)
                ?? throw new ArgumentException(
                    $"The parameter order for '{toolName}' is not known.");

            var unknown = named.Keys.Where(
                key => !order.Contains(key, StringComparer.OrdinalIgnoreCase)).ToList();
            if (unknown.Count > 0)
                throw new ArgumentException(
                    $"{toolName} has no parameter(s) named {string.Join(", ", unknown)}. "
                    + $"It takes: {string.Join(", ", order)}");

            var slots = new object[order.Length];
            var lastUsed = -1;
            for (var i = 0; i < order.Length; i++)
            {
                var match = named.Keys.FirstOrDefault(
                    key => string.Equals(key, order[i], StringComparison.OrdinalIgnoreCase));
                if (match == null || named[match] == null) continue;
                slots[i] = named[match];
                lastUsed = i;
            }

            var flags = FlagsFor(addToMap);

            var result = await Geoprocessing.ExecuteToolAsync(
                toolName,
                Geoprocessing.MakeValueArray(slots.Take(lastUsed + 1).ToArray()),
                BuildEnvironment(null), null, null, flags).ConfigureAwait(false);
            LastResult = result;
            return result;
        }

        // --- introspection ---------------------------------------------------

        private static object ListTools(Params parameters)
        {
            var wildcard = (parameters.GetString("wildcard", "*") ?? "*").Trim('*');
            var toolbox = parameters.GetString("toolbox");
            var limit = parameters.GetInt("limit", 500);

            var names = ParameterOrder.Value.Keys
                .Where(name => name.Contains("."))          // the alias.Tool form only
                .Where(name => string.IsNullOrEmpty(wildcard)
                               || name.IndexOf(wildcard, StringComparison.OrdinalIgnoreCase) >= 0)
                .Where(name => string.IsNullOrWhiteSpace(toolbox)
                               || name.StartsWith(toolbox + ".", StringComparison.OrdinalIgnoreCase))
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return new Dictionary<string, object>
            {
                ["pattern"] = parameters.GetString("wildcard", "*"),
                ["count"] = names.Count,
                ["tools"] = names.Take(limit).ToList(),
                ["truncated"] = names.Count > limit,
            };
        }

        private static object DescribeTool(Params parameters)
        {
            var toolName = parameters.Require("tool_name");
            var order = OrderFor(toolName);
            if (order == null)
                throw new ArgumentException(
                    $"Tool not found: {toolName}. Search with list_geoprocessing_tools.");

            // Detail rows, when the build carried them: datatype, ValueList
            // enumerations and short defaults captured from arcpy.
            Dictionary<string, JsonElement> detailByName = null;
            var detail = DetailFor(toolName);
            if (detail.HasValue
                && detail.Value.TryGetProperty("parameters", out var detailParameters)
                && detailParameters.ValueKind == JsonValueKind.Array)
            {
                detailByName = new Dictionary<string, JsonElement>(
                    StringComparer.OrdinalIgnoreCase);
                foreach (var row in detailParameters.EnumerateArray())
                {
                    if (row.TryGetProperty("name", out var rowName)
                        && rowName.ValueKind == JsonValueKind.String)
                        detailByName[rowName.GetString()] = row;
                }
            }

            var rows = new List<Dictionary<string, object>>();
            for (var index = 0; index < order.Length; index++)
            {
                var name = order[index];
                var row = new Dictionary<string, object>
                {
                    ["name"] = name,
                    ["position"] = index,
                };
                if (detailByName != null && detailByName.TryGetValue(name, out var extra))
                {
                    if (extra.TryGetProperty("datatype", out var datatype)
                        && datatype.ValueKind == JsonValueKind.String)
                        row["datatype"] = datatype.GetString();
                    if (extra.TryGetProperty("default", out var def))
                        row["default"] = def.ValueKind == JsonValueKind.String
                            ? def.GetString()
                            : def.ToString();
                    if (extra.TryGetProperty("filter", out var filter)
                        && filter.ValueKind == JsonValueKind.Object
                        && filter.TryGetProperty("list", out var list)
                        && list.ValueKind == JsonValueKind.Array)
                    {
                        row["filter_list"] = list.EnumerateArray()
                            .Select(v => v.GetString()).ToList();
                    }
                }
                rows.Add(row);
            }

            var result = new Dictionary<string, object>
            {
                ["tool"] = toolName,
                ["parameters"] = rows,
                ["usage"] = $"{toolName}({string.Join(", ", order)})",
                ["details_available"] = detailByName != null,
                ["note"] = "Names and order come from arcpy, captured when the add-in was "
                         + "built. Pass them as named parameters. filter_list entries are "
                         + "the tool's accepted values (localised); the tool decides what "
                         + "a value means.",
            };
            var document = ParameterDetails.Value;
            if (document != null
                && document.RootElement.TryGetProperty("generated_at", out var generated)
                && generated.ValueKind == JsonValueKind.String)
                result["details_generated_at"] = generated.GetString();
            return result;
        }

        private static object ListToolboxes(Params parameters)
        {
            var aliases = ParameterOrder.Value.Keys
                .Where(name => name.Contains("."))
                .Select(name => name.Substring(0, name.IndexOf('.')))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(alias => alias, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return new Dictionary<string, object>
            {
                ["toolbox_aliases"] = aliases,
                ["tool_count"] = ParameterOrder.Value.Keys.Count(n => n.Contains(".")),
            };
        }

        private static object CheckExtension(Params parameters)
        {
            var name = parameters.Require("extension");
            // Extension licensing is not exposed to add-ins the way arcpy exposes
            // it; the tools themselves fail with a clear message when unlicensed.
            return new Dictionary<string, object>
            {
                ["extension"] = name,
                ["status"] = "unknown",
                ["note"] = "ArcGIS Pro does not expose extension checkout to add-ins. "
                         + "Run the tool: it reports clearly if the licence is missing.",
            };
        }

        private static object GetMessages(Params parameters)
        {
            if (LastResult == null)
                return new Dictionary<string, object> { ["messages"] = new List<string>() };

            var severity = parameters.GetInt("severity");
            var messages = LastResult.Messages ?? new List<IGPMessage>();
            var filtered = severity >= 2
                ? LastResult.ErrorMessages?.Select(m => m.Text).ToList()
                : messages.Select(m => m.Text).ToList();

            return new Dictionary<string, object>
            {
                ["messages"] = filtered,
                ["failed"] = LastResult.IsFailed,
            };
        }

        /// <summary>
        /// Walks a path from its first existing ancestor and, at the first
        /// segment that does not exist, offers the closest sibling name --
        /// enough to catch a typo such as "claude code app" for
        /// "claude code_app" (spaces and underscores are ignored when
        /// comparing, which is exactly how that one happened).
        /// </summary>
        private static string SuggestSimilarPath(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path)) return null;
                string full;
                try { full = Path.GetFullPath(path.Trim().Trim('"')); }
                catch { return null; }
                var root = Path.GetPathRoot(full);
                if (string.IsNullOrEmpty(root)) return null;
                var parts = full.Substring(root.Length)
                    .Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                           StringSplitOptions.RemoveEmptyEntries);
                var current = root;
                for (var i = 0; i < parts.Length; i++)
                {
                    var candidate = Path.Combine(current, parts[i]);
                    if (File.Exists(candidate) || Directory.Exists(candidate))
                    {
                        current = candidate;
                        continue;
                    }
                    var best = BestSibling(current, parts[i]);
                    if (best == null) return null;
                    var rebuilt = Path.Combine(current, best);
                    for (var j = i + 1; j < parts.Length; j++)
                        rebuilt = Path.Combine(rebuilt, parts[j]);
                    return rebuilt;
                }
                return null;   // everything exists -- the failure was elsewhere
            }
            catch
            {
                return null;
            }
        }

        private static string BestSibling(string directory, string name)
        {
            try
            {
                if (!Directory.Exists(directory)) return null;
                string best = null;
                var bestScore = 0.0;
                var target = NormalizeName(name);
                foreach (var entry in Directory.EnumerateFileSystemEntries(directory))
                {
                    var candidate = Path.GetFileName(entry);
                    var score = Similarity(target, NormalizeName(candidate));
                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = candidate;
                    }
                }
                return bestScore >= 0.6 ? best : null;
            }
            catch
            {
                return null;
            }
        }

        private static string NormalizeName(string value)
        {
            return new string((value ?? string.Empty).ToLowerInvariant()
                .Where(c => c != ' ' && c != '_' && c != '-').ToArray());
        }

        private static double Similarity(string a, string b)
        {
            if (a.Length == 0 && b.Length == 0) return 1;
            if (a.Length == 0 || b.Length == 0) return 0;
            var distance = Levenshtein(a, b);
            return 1.0 - (double)distance / Math.Max(a.Length, b.Length);
        }

        private static int Levenshtein(string a, string b)
        {
            var previous = new int[b.Length + 1];
            var current = new int[b.Length + 1];
            for (var j = 0; j <= b.Length; j++) previous[j] = j;
            for (var i = 1; i <= a.Length; i++)
            {
                current[0] = i;
                for (var j = 1; j <= b.Length; j++)
                {
                    var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    current[j] = Math.Min(
                        Math.Min(current[j - 1] + 1, previous[j] + 1),
                        previous[j - 1] + cost);
                }
                var swap = previous;
                previous = current;
                current = swap;
            }
            return previous[b.Length];
        }
    }
}
