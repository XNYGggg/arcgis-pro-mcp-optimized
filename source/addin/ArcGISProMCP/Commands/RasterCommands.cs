using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Raster;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGISProMCP.Bridge;

namespace ArcGISProMCP.Commands
{
    /// <summary>Reading, symbolising and calculating with rasters.</summary>
    internal static class RasterCommands
    {
        private const string Group = "raster";

        public static void Register()
        {
            CommandRouter.RegisterAsync("get_raster_info", Group,
                "Raster detail: bands, size, cell size, pixel type, statistics and CRS.",
                GetRasterInfoAsync);

            CommandRouter.Register("set_raster_symbology", Group,
                "Set a raster layer's colorizer and colour ramp.", SetSymbology);

            CommandRouter.RegisterAsync("sample_raster_values", Group,
                "Sample raster values at a point layer or explicit x/y points. "
                + "Values come back inline; no table is written unless "
                + "write_table=true.",
                SampleRasterValuesAsync);

            CommandRouter.RegisterAsync("raster_calculator", Group,
                "Map algebra over named rasters. Needs Spatial Analyst.",
                RasterCalculatorAsync, lane: ExecutionLane.Geoprocessing);
        }

        // --- info -------------------------------------------------------------

        /// <summary>
        /// Structure comes from the .NET raster API. Statistics do not: it
        /// exposes no equivalent of arcpy's raster.minimum, so those come from
        /// GetRasterProperties, which is also why this command is async.
        /// </summary>
        private static async Task<object> GetRasterInfoAsync(Params parameters)
        {
            var name = parameters.Require("layer_name");

            var info = await QueuedTask.Run(() =>
            {
                var (raster, source, disposable) = OpenRaster(parameters, name);
                try
                {
                    var bandCount = raster.GetBandCount();
                    var result = new Dictionary<string, object>
                    {
                        ["layer"] = name,
                        ["source"] = source,
                        ["band_count"] = bandCount,
                        ["width"] = raster.GetWidth(),
                        ["height"] = raster.GetHeight(),
                        ["pixel_type"] = raster.GetPixelType().ToString(),
                        ["nodata_value"] = raster.GetNoDataValue(),
                    };

                    var cell = raster.GetMeanCellSize();
                    result["cell_size_x"] = cell.Item1;
                    result["cell_size_y"] = cell.Item2;

                    var extent = raster.GetExtent();
                    if (extent != null) result["extent"] = MapHelpers.Describe(extent);

                    var reference = raster.GetSpatialReference();
                    if (reference != null)
                        result["spatial_reference"] = MapHelpers.Describe(reference);

                    var bands = new List<string>();
                    for (var i = 0; i < bandCount; i++)
                    {
                        using var band = raster.GetBand(i);
                        bands.Add(band.GetName());
                    }
                    result["band_names"] = bands;
                    return result;
                }
                finally
                {
                    disposable?.Dispose();
                }
            }).ConfigureAwait(false);

            var statistics = await ReadStatisticsAsync(
                (string)info["source"] ?? name).ConfigureAwait(false);
            foreach (var pair in statistics) info[pair.Key] = pair.Value;
            return info;
        }

        /// <summary>
        /// Band 1's statistics, or a note saying why there are none. A raster
        /// whose statistics have never been built reports the failure rather
        /// than a plausible zero.
        /// </summary>
        private static async Task<Dictionary<string, object>> ReadStatisticsAsync(string source)
        {
            var wanted = new Dictionary<string, string>
            {
                ["minimum"] = "MINIMUM",
                ["maximum"] = "MAXIMUM",
                ["mean"] = "MEAN",
                ["standard_deviation"] = "STD",
            };

            var statistics = new Dictionary<string, object>();
            foreach (var pair in wanted)
            {
                try
                {
                    var result = await GeoprocessingCommands.RunNamedAsync(
                        "management.GetRasterProperties",
                        new Dictionary<string, object>
                        {
                            ["in_raster"] = source,
                            ["property_type"] = pair.Value,
                            ["band_index"] = "Band_1",
                        },
                        addToMap: false).ConfigureAwait(false);

                    if (result.IsFailed) continue;
                    if (double.TryParse(result.ReturnValue, out var value))
                        statistics[pair.Key] = value;
                }
                catch (Exception)
                {
                    // One missing property should not cost the caller the rest
                    // of the report.
                }
            }

            if (statistics.Count == 0)
                statistics["statistics"] =
                    "not available -- run the Calculate Statistics tool on this raster";
            return statistics;
        }

        /// <summary>
        /// Rasters arrive either as a layer in the map or as a path, the same
        /// as everywhere else. The third value is what to dispose, which is
        /// nothing when the raster came from a layer.
        /// </summary>
        private static (Raster, string, IDisposable) OpenRaster(Params parameters, string name)
        {
            Map map = null;
            try { map = MapHelpers.ResolveMap(parameters); } catch (Exception) { }

            var layer = map?.GetLayersAsFlattenedList().OfType<BasicRasterLayer>()
                .FirstOrDefault(l => string.Equals(l.Name, name,
                                                   StringComparison.OrdinalIgnoreCase));
            if (layer != null)
            {
                var raster = layer.GetRaster();
                return (raster, layer.GetPath()?.LocalPath ?? name, raster);
            }

            var normalised = name.Replace('/', '\\');
            var folder = Path.GetDirectoryName(normalised);
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                throw new ArgumentException(
                    $"No raster layer called '{name}', and it is not a raster path either.");

            var datastore = folder.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase)
                ? (Datastore)new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(folder)))
                : new FileSystemDatastore(new FileSystemConnectionPath(
                    new Uri(folder), FileSystemDatastoreType.Raster));

            var dataset = Path.GetFileName(normalised);
            var rasterDataset = datastore is Geodatabase geodatabase
                ? geodatabase.OpenDataset<RasterDataset>(dataset)
                : ((FileSystemDatastore)datastore).OpenDataset<RasterDataset>(dataset);

            return (rasterDataset.CreateFullRaster(), normalised, datastore);
        }

        // --- symbology --------------------------------------------------------

        // --- sampling (D3, 2026-11-18) ---------------------------------------
        //
        // Sampling reads pixels straight from the raster: values are inline,
        // explicit points need no point layer, and nothing is written to the
        // default geodatabase. The old sa.Sample path is still reachable with
        // write_table=true, for callers that want a table.

        private static async Task<object> SampleRasterValuesAsync(Params parameters)
        {
            var name = parameters.Require("layer_name");
            var band = Math.Max(0, parameters.GetInt("band", 1) - 1);
            var explicitPoints = parameters.GetObjectList("points").ToList();

            if (explicitPoints.Count == 0 && !parameters.Has("point_layer"))
                throw new ArgumentException(
                    "point_layer or points is required: pass a point layer name/path, "
                    + "or points as [{x,y}, ...] / [[x,y], ...].");

            if (parameters.GetBool("write_table"))
                return await SampleViaTableAsync(parameters, name).ConfigureAwait(false);

            return await QueuedTask.Run(() =>
            {
                var (raster, source, disposable) = OpenRaster(parameters, name);
                try
                {
                    var reference = raster.GetSpatialReference();
                    var nodata = raster.GetNoDataValue();
                    var width = raster.GetWidth();
                    var height = raster.GetHeight();
                    var samples = new List<Dictionary<string, object>>();

                    if (explicitPoints.Count > 0)
                    {
                        foreach (var point in explicitPoints)
                        {
                            var (x, y) = Coordinates(point);
                            samples.Add(SampleOne(raster, band, x, y, nodata, width, height));
                        }
                    }
                    else
                    {
                        var limit = Math.Max(1, parameters.GetInt("limit", 1000));
                        using var data = DataSource.Resolve(parameters, "point_layer");
                        using var cursor = data.Search(new QueryFilter());
                        while (cursor.MoveNext() && samples.Count < limit)
                        {
                            using var row = cursor.Current;
                            if (!(row is Feature feature)) continue;
                            if (!(feature.GetShape() is MapPoint point)) continue;

                            var projected = point;
                            if (reference != null && point.SpatialReference != null
                                && point.SpatialReference.Wkid != reference.Wkid)
                            {
                                projected = GeometryEngine.Instance.Project(
                                    point, reference) as MapPoint ?? point;
                            }
                            samples.Add(SampleOne(raster, band, projected.X, projected.Y,
                                                  nodata, width, height));
                        }
                    }

                    return (object)new Dictionary<string, object>
                    {
                        ["raster"] = name,
                        ["source"] = source,
                        ["band"] = band + 1,
                        ["nodata_value"] = nodata,
                        ["count"] = samples.Count,
                        ["values"] = samples,
                        ["written_table"] = false,
                    };
                }
                finally
                {
                    disposable?.Dispose();
                }
            }).ConfigureAwait(false);
        }

        private static (double x, double y) Coordinates(Params point)
        {
            if (point.Root.ValueKind == JsonValueKind.Array)
            {
                var numbers = point.Root.EnumerateArray()
                    .Where(v => v.ValueKind == JsonValueKind.Number)
                    .Select(v => v.GetDouble()).ToArray();
                if (numbers.Length >= 2) return (numbers[0], numbers[1]);
            }
            if (point.Has("x") && point.Has("y"))
                return (point.GetDouble("x"), point.GetDouble("y"));
            throw new ArgumentException(
                "Each point needs x and y, as {\"x\": .., \"y\": ..} or [x, y].");
        }

        private static Dictionary<string, object> SampleOne(
            Raster raster, int band, double x, double y, object nodata,
            int width, int height)
        {
            var (column, row) = raster.MapToPixel(x, y);
            var entry = new Dictionary<string, object>
            {
                ["x"] = x, ["y"] = y, ["column"] = column, ["row"] = row,
            };
            if (column < 0 || row < 0 || column >= width || row >= height)
            {
                entry["value"] = null;
                entry["reason"] = "outside raster";
                return entry;
            }

            var value = raster.GetPixelValue(band, column, row);
            if (value == null)
            {
                entry["value"] = null;
                return entry;
            }
            try
            {
                if (nodata != null
                    && Convert.ToDouble(value) == Convert.ToDouble(nodata))
                {
                    entry["value"] = null;
                    entry["reason"] = "nodata";
                    return entry;
                }
            }
            catch { }
            entry["value"] = value;
            return entry;
        }

        private static async Task<object> SampleViaTableAsync(Params parameters, string name)
        {
            var pointLayer = parameters.Require("point_layer");
            var outTable = parameters.GetString("output_table");
            if (string.IsNullOrWhiteSpace(outTable))
            {
                var gdb = Project.Current?.DefaultGeodatabasePath ?? "";
                outTable = Path.Combine(gdb,
                    "sample_" + Guid.NewGuid().ToString("N").Substring(0, 8));
            }

            var result = await GeoprocessingCommands.RunNamedAsync(
                "sa.Sample",
                new Dictionary<string, object>
                {
                    ["in_rasters"] = name,
                    ["in_location_data"] = pointLayer,
                    ["out_table"] = outTable,
                },
                addToMap: false).ConfigureAwait(false);

            if (result.IsFailed)
            {
                var messages = result.ErrorMessages != null
                               && result.ErrorMessages.Any()
                    ? string.Join("; ", result.ErrorMessages.Select(m => m.Text))
                    : "the tool reported no error text";
                throw new InvalidOperationException($"sample_raster_values failed: {messages}");
            }

            return new Dictionary<string, object>
            {
                ["raster"] = name,
                ["table"] = outTable,
                ["written_table"] = true,
                ["values"] = null,
            };
        }

        private static object SetSymbology(Params parameters)
        {
            var map = MapHelpers.ResolveMap(parameters);
            var name = parameters.Require("layer_name");

            var layer = MapHelpers.FindLayer(map, name) as BasicRasterLayer
                ?? throw new ArgumentException(
                    $"No raster layer called '{name}' (a dataset path works when "
                    + "exactly one loaded layer points at it).");

            var kind = (parameters.GetString("colorizer", "stretch") ?? "stretch")
                .ToLowerInvariant().Replace("_", "");
            var ramp = SymbologyCommands.FindColorRamp(parameters.GetString("color_ramp"));
            var applied = new Dictionary<string, object> { ["layer"] = layer.Name };

            if (parameters.Has("colorizer") || parameters.Has("color_ramp")
                || parameters.Has("stretch_type") || parameters.Has("break_count"))
            {
                RasterColorizerDefinition definition;
                switch (kind)
                {
                    case "classify":
                        definition = new ClassifyColorizerDefinition(
                            null,
                            parameters.GetInt("break_count", 5),
                            SymbologyCommands.ParseClassification(
                                parameters.GetString("classification_method")),
                            ramp);
                        break;
                    case "uniquevalues":
                        definition = new UniqueValueColorizerDefinition(null, ramp);
                        break;
                    case "rgb":
                        definition = new RGBColorizerDefinition(
                            0, 1, 2, ParseStretch(parameters.GetString("stretch_type")),
                            2.0, 2.0, 2.0);
                        break;
                    default:
                        definition = new StretchColorizerDefinition(
                            0, ParseStretch(parameters.GetString("stretch_type")), 1.0, ramp);
                        break;
                }

                if (!layer.CanCreateColorizer(definition))
                    throw new InvalidOperationException(
                        $"A {kind} colorizer does not suit '{layer.Name}'. An RGB "
                        + "colorizer needs three bands; unique values needs an "
                        + "integer raster with an attribute table.");

                var colorizer = layer.CreateColorizer(definition);
                foreach (var pair in ApplyValueSymbology(colorizer, parameters))
                    applied[pair.Key] = pair.Value;
                layer.SetColorizer(colorizer);
                applied["colorizer"] = kind;
                if (ramp != null) applied["color_ramp"] = parameters.GetString("color_ramp");
            }

            if (parameters.Has("transparency"))
            {
                layer.SetTransparency(parameters.GetDouble("transparency"));
                applied["transparency"] = layer.Transparency;
            }

            if (applied.Count == 1)
                throw new ArgumentException(
                    "Provide colorizer, color_ramp, stretch_type, break_count "
                    + "and/or transparency.");
            return applied;
        }

        /// <summary>
        /// Explicit per-value colours and labels for a unique-value raster
        /// colorizer. Keys are the raster's own values as strings ("1", "2", …);
        /// colours are [r,g,b] or [r,g,b,a] with 0-100 alpha, matching the
        /// other symbology commands. Values the raster does not have come back
        /// under unmatched_values instead of vanishing.
        /// </summary>
        private static Dictionary<string, object> ApplyValueSymbology(
            CIMRasterColorizer colorizer, Params parameters)
        {
            var result = new Dictionary<string, object>();
            if (colorizer is not CIMRasterUniqueValueColorizer unique) return result;

            var colors = parameters.GetObject("value_colors");
            var labels = parameters.GetObject("value_labels");
            if ((colors == null || colors.Count == 0)
                && (labels == null || labels.Count == 0))
                return result;

            var matched = new List<string>();
            foreach (var group in unique.Groups
                     ?? Array.Empty<CIMRasterUniqueValueGroup>())
            {
                foreach (var item in group.Classes
                         ?? Array.Empty<CIMRasterUniqueValueClass>())
                {
                    foreach (var raw in item.Values ?? Array.Empty<string>())
                    {
                        if (colors != null && colors.TryGetValue(raw, out var color))
                        {
                            item.Color = SymbologyCommands.ToColor(ReadColor(color));
                            matched.Add(raw);
                        }
                        if (labels != null && labels.TryGetValue(raw, out var label)
                            && label.ValueKind == JsonValueKind.String)
                            item.Label = label.GetString();
                    }
                }
            }

            if (colors != null && colors.Count > 0)
            {
                result["value_colors_applied"] = matched.Distinct().ToList();
                var unmatched = colors.Keys
                    .Where(key => !matched.Contains(key, StringComparer.OrdinalIgnoreCase))
                    .ToList();
                if (unmatched.Count > 0) result["unmatched_values"] = unmatched;
            }
            return result;
        }

        private static int[] ReadColor(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Array)
                throw new ArgumentException(
                    "value_colors entries are [r,g,b] or [r,g,b,a] arrays.");
            var parts = element.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.Number)
                .Select(item => (int)Math.Round(item.GetDouble()))
                .ToArray();
            if (parts.Length < 3)
                throw new ArgumentException("A colour needs at least r, g, b.");
            return parts.Length >= 4
                ? new[] { parts[0], parts[1], parts[2], parts[3] }
                : new[] { parts[0], parts[1], parts[2], 100 };
        }

        private static RasterStretchType ParseStretch(string name)
        {
            switch ((name ?? "MinimumMaximum").Replace(" ", "").ToUpperInvariant())
            {
                case "STANDARDDEVIATION":
                case "STANDARDDEVIATIONS": return RasterStretchType.StandardDeviations;
                case "PERCENTCLIP":
                case "PERCENTMINIMUMMAXIMUM": return RasterStretchType.PercentMinimumMaximum;
                case "HISTOGRAMEQUALIZE": return RasterStretchType.HistogramEqualize;
                case "NONE": return RasterStretchType.None;
                case "ESRI": return RasterStretchType.ESRI;
                default: return RasterStretchType.MinimumMaximum;
            }
        }

        // --- map algebra ------------------------------------------------------

        /// <summary>
        /// The tool takes one expression string, so the aliases are substituted
        /// into it first. Longest alias wins, so "dem" inside "dem_fill" is not
        /// replaced out from under the caller.
        /// </summary>
        private static async Task<object> RasterCalculatorAsync(Params parameters)
        {
            var rasters = parameters.GetObject("rasters")
                ?? throw new ArgumentException(
                    "rasters must map each alias to a layer name or raster path.");
            var expression = parameters.Require("expression");
            var output = parameters.Require("output_path");

            var resolved = expression;
            foreach (var alias in rasters.Keys.OrderByDescending(k => k.Length))
            {
                var source = rasters[alias].ToString();
                resolved = Regex.Replace(resolved, $@"\b{Regex.Escape(alias)}\b",
                                         "\"" + source + "\"");
            }

            var started = DateTime.Now;
            var result = await GeoprocessingCommands.RunNamedAsync(
                "sa.RasterCalculator",
                new Dictionary<string, object>
                {
                    ["expression"] = resolved,
                    ["output_raster"] = output,
                },
                parameters.GetBool("add_to_map", true)).ConfigureAwait(false);

            if (result.IsFailed)
            {
                var messages = result.ErrorMessages != null && result.ErrorMessages.Any()
                    ? string.Join("; ", result.ErrorMessages.Select(m => m.Text))
                    : "the tool reported no error text";
                throw new InvalidOperationException(
                    $"raster_calculator failed: {messages}. The expression sent was: "
                    + resolved);
            }

            return new Dictionary<string, object>
            {
                ["output"] = output,
                ["expression"] = resolved,
                ["elapsed_seconds"] = Math.Round((DateTime.Now - started).TotalSeconds, 2),
                ["messages"] = result.Messages?.Select(m => m.Text).Take(20).ToList(),
            };
        }
    }
}
