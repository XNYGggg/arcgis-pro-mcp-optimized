using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGISProMCP.Bridge;

namespace ArcGISProMCP.Commands
{
    /// <summary>
    /// Commands that are one geoprocessing tool under a friendlier name.
    ///
    /// Each one says which tool parameter each value belongs to; the ordering
    /// comes from the embedded parameter table. Hand-built positional arrays
    /// were tried first and quietly put select_by_location's invert flag in
    /// the output-layer slot, which is exactly the mistake this avoids.
    /// </summary>
    internal static class GpWrapperCommands
    {
        public static void Register()
        {
            Wrap("add_field", "schema", "Add a field to a layer or table.",
                "management.AddField", p => new Dictionary<string, object>
                {
                    ["in_table"] = Target(p),
                    ["field_name"] = p.Require("field_name"),
                    ["field_type"] = p.GetString("field_type", "TEXT"),
                    ["field_precision"] = p.GetString("field_precision"),
                    ["field_scale"] = p.GetString("field_scale"),
                    ["field_length"] = p.GetString("field_length"),
                    ["field_alias"] = p.GetString("field_alias"),
                    ["field_is_nullable"] = p.GetString("nullable", "NULLABLE"),
                    ["field_domain"] = p.GetString("field_domain"),
                });

            Wrap("delete_field", "schema", "Delete one or more fields.",
                "management.DeleteField", p => new Dictionary<string, object>
                {
                    ["in_table"] = Target(p),
                    ["drop_field"] = string.Join(";", p.GetStringList("field_names")
                        ?? new List<string> { p.Require("field_name") }),
                });

            Wrap("alter_field", "schema", "Rename a field or change its alias or length.",
                "management.AlterField", p => new Dictionary<string, object>
                {
                    ["in_table"] = Target(p),
                    ["field"] = p.Require("field_name"),
                    ["new_field_name"] = p.GetString("new_name"),
                    ["new_field_alias"] = p.GetString("new_alias"),
                    ["field_length"] = p.GetString("field_length"),
                });

            Wrap("calculate_field", "data", "Calculate field values across a layer.",
                "management.CalculateField", p => new Dictionary<string, object>
                {
                    ["in_table"] = Target(p),
                    ["field"] = p.Require("field_name"),
                    ["expression"] = p.Require("expression"),
                    ["expression_type"] = p.GetString("expression_type", "PYTHON3"),
                    ["code_block"] = p.GetString("code_block"),
                });

            Wrap("create_feature_class", "schema",
                "Create an empty feature class in a geodatabase or folder.",
                "management.CreateFeatureclass", p =>
                {
                    // Validate fields before anything is created: an invalid
                    // request must not leave a dataset behind.
                    ParseFieldRequests(p);
                    return new Dictionary<string, object>
                    {
                        ["out_path"] = p.GetString("out_path")
                                       ?? Project.Current?.DefaultGeodatabasePath,
                        ["out_name"] = p.Require("name"),
                        ["geometry_type"] = p.GetString("geometry_type", "POLYGON"),
                        ["template"] = p.GetString("template"),
                        ["has_m"] = p.GetString("has_m", "DISABLED"),
                        ["has_z"] = p.GetString("has_z", "DISABLED"),
                        ["spatial_reference"] = p.Has("epsg") ? p.GetInt("epsg").ToString() : null,
                    };
                }, addToMapByDefault: true,
                afterSuccess: (parameters, outputs) =>
                    ApplyAndVerifyFieldsAsync(parameters, outputs.FirstOrDefault()));

            Wrap("create_table", "schema", "Create an empty standalone table.",
                "management.CreateTable", p =>
                {
                    ParseFieldRequests(p);
                    return new Dictionary<string, object>
                    {
                        ["out_path"] = p.GetString("out_path")
                                       ?? Project.Current?.DefaultGeodatabasePath,
                        ["out_name"] = p.Require("name"),
                        ["template"] = p.GetString("template"),
                    };
                }, addToMapByDefault: true,
                afterSuccess: (parameters, outputs) =>
                    ApplyAndVerifyFieldsAsync(parameters, outputs.FirstOrDefault()));

            Wrap("create_file_geodatabase", "schema", "Create a file geodatabase.",
                "management.CreateFileGDB", p => new Dictionary<string, object>
                {
                    ["out_folder_path"] = p.GetString("folder")
                                          ?? Project.Current?.HomeFolderPath,
                    ["out_name"] = p.Require("name"),
                });

            Wrap("delete_dataset", "schema",
                "Delete a dataset from disk or a geodatabase. This cannot be undone.",
                "management.Delete", p => new Dictionary<string, object>
                {
                    ["in_data"] = p.Require("path"),
                });

            Wrap("truncate_table", "schema", "Delete every row, keeping the schema.",
                "management.TruncateTable", p => new Dictionary<string, object>
                {
                    ["in_table"] = Target(p),
                });

            Wrap("export_features", "schema",
                "Export a layer, honouring its selection and definition query.",
                "conversion.ExportFeatures", p => new Dictionary<string, object>
                {
                    ["in_features"] = Target(p),
                    ["out_features"] = p.Require("out_path"),
                    ["where_clause"] = p.GetString("where"),
                }, addToMapByDefault: true);

            Wrap("select_by_location", "selection",
                "Select features by spatial relationship to another layer.",
                "management.SelectLayerByLocation", p => new Dictionary<string, object>
                {
                    ["in_layer"] = Target(p),
                    ["overlap_type"] = p.GetString("relationship", "INTERSECT"),
                    ["select_features"] = p.Require("select_features"),
                    ["search_distance"] = p.GetString("search_distance"),
                    ["selection_type"] = p.GetString("method", "NEW_SELECTION"),
                    ["invert_spatial_relationship"] = p.GetString("invert", "NOT_INVERT"),
                });

            // sample_raster_values is native (RasterCommands): it samples the
            // raster directly, so no fixed raster_samples table is written and
            // points can be passed without a point layer (D3, 2026-11-18).

            Wrap("remove_join", "layers", "Remove a join from a layer.",
                "management.RemoveJoin", p => new Dictionary<string, object>
                {
                    ["in_layer_or_view"] = Target(p),
                    ["join_name"] = p.GetString("join_name"),
                }, afterSuccess: (p, _) =>
                {
                    JoinRegistry.Remove(p.GetString("layer_name"));
                    return Task.CompletedTask;
                });

            Wrap("zonal_statistics", "raster",
                "Summarise raster values inside zone polygons.",
                "sa.ZonalStatisticsAsTable", p => new Dictionary<string, object>
                {
                    ["in_zone_data"] = p.Require("zone_layer"),
                    ["zone_field"] = p.Require("zone_field"),
                    ["in_value_raster"] = p.Require("raster_layer"),
                    ["out_table"] = p.GetString("output_table")
                        ?? Path.Combine(Project.Current?.DefaultGeodatabasePath ?? "",
                                        "zonal_stats"),
                    ["ignore_nodata"] = p.GetString("ignore_nodata", "DATA"),
                    ["statistics_type"] = p.GetString("statistics_type", "ALL"),
                });

            // sample_raster_values is native (RasterCommands): it samples the
            // raster directly, so no fixed raster_samples table is written and
            // points can be passed without a point layer (D3, 2026-11-18).

            Wrap("apply_symbology_from_layer", "symbology",
                "Copy symbology from a .lyrx file or another layer.",
                "management.ApplySymbologyFromLayer", p => new Dictionary<string, object>
                {
                    ["in_layer"] = Target(p),
                    ["in_symbology_layer"] = p.Require("symbology_source"),
                    ["update_symbology"] = p.GetString("update_symbology", "DEFAULT"),
                });

            Wrap("save_layer_file", "symbology",
                "Save a layer with its symbology to a .lyrx file.",
                "management.SaveToLayerFile", p => new Dictionary<string, object>
                {
                    ["in_layer"] = Target(p),
                    ["out_layer"] = p.Require("output_path"),
                    ["is_relative_path"] = p.GetString("relative_paths", "ABSOLUTE"),
                });

            RegisterAddFields();
            RegisterAddJoin();

            // add_join is registered by RegisterAddJoin: it must capture the
            // layer's source path before GP turns it into a joined view.
        }

        /// <summary>
        /// AddFields takes one value table, where every field is a row of
        /// space-separated values -- and a column out of place there would put
        /// a length where an alias belongs, silently. So this calls AddField
        /// once per field, by name. Slower for a long list, and worth it.
        /// </summary>
        private static void RegisterAddFields()
        {
            CommandRouter.RegisterAsync("add_fields", "schema",
                "Add several fields in one call.", async parameters =>
                {
                    var target = Target(parameters);
                    var added = new List<string>();

                    foreach (var field in parameters.GetObjectList("fields"))
                    {
                        var name = field.Require("name");
                        var result = await GeoprocessingCommands.RunNamedAsync(
                            "management.AddField",
                            new Dictionary<string, object>
                            {
                                ["in_table"] = target,
                                ["field_name"] = name,
                                ["field_type"] = field.GetString("type", "TEXT"),
                                ["field_precision"] = field.GetString("precision"),
                                ["field_scale"] = field.GetString("scale"),
                                ["field_length"] = field.GetString("length"),
                                ["field_alias"] = field.GetString("alias"),
                                ["field_is_nullable"] = field.GetString("nullable", "NULLABLE"),
                                ["field_domain"] = field.GetString("domain"),
                            },
                            addToMap: false).ConfigureAwait(false);

                        if (result.IsFailed)
                        {
                            var messages = result.ErrorMessages != null
                                           && result.ErrorMessages.Any()
                                ? string.Join("; ", result.ErrorMessages.Select(m => m.Text))
                                : "the tool reported no error text";
                            // Half the fields may already exist by now, so say
                            // which -- a blind retry would fail on those.
                            throw new InvalidOperationException(
                                $"add_fields stopped at '{name}': {messages}. Fields "
                                + "added before it: "
                                + (added.Count == 0 ? "(none)" : string.Join(", ", added)));
                        }
                        added.Add(name);
                    }

                    if (added.Count == 0)
                        throw new ArgumentException(
                            "fields must be a list, e.g. "
                            + "[{\"name\": \"AREA_HA\", \"type\": \"DOUBLE\"}]");

                    return new Dictionary<string, object>
                    {
                        ["table"] = target,
                        ["added"] = added,
                        ["count"] = added.Count,
                    };
                }, lane: ExecutionLane.Geoprocessing);
        }

        /// <summary>
        /// add_join needs the layer's original source path: management.AddJoin
        /// turns the layer into a joined view whose GetPath() no longer points
        /// at the base dataset. The path is captured before the GP call and
        /// stored with the join so get_features can read client-side.
        /// </summary>
        private static void RegisterAddJoin()
        {
            CommandRouter.RegisterAsync("add_join", "layers",
                "Join a table to a layer on a common field.", async parameters =>
                {
                    var layerName = Target(parameters);
                    string basePath = null;
                    try
                    {
                        basePath = await QueuedTask.Run(() =>
                        {
                            var map = MapHelpers.ResolveMap(parameters);
                            var layer = map?.GetLayersAsFlattenedList()
                                .OfType<BasicFeatureLayer>()
                                .FirstOrDefault(l => string.Equals(l.Name, layerName,
                                                                   StringComparison.OrdinalIgnoreCase));
                            return layer?.GetPath()?.LocalPath;
                        }).ConfigureAwait(false);
                    }
                    catch { }

                    // Field names of the join table, read on the MCT (a map's
                    // tables cannot be touched from the caller's thread).
                    var joinTableName = parameters.GetString("join_table");
                    List<string> joinFields = null;
                    try
                    {
                        joinFields = await QueuedTask.Run(() =>
                        {
                            var json = System.Text.Json.JsonDocument.Parse(
                                "{\"layer_name\":"
                                + System.Text.Json.JsonSerializer.Serialize(joinTableName)
                                + "}");
                            using var source = DataSource.Resolve(
                                new Params(json.RootElement));
                            return source.Definition.GetFields()
                                .Select(f => f.Name).ToList();
                        }).ConfigureAwait(false);
                    }
                    catch { }

                    const string toolName = "management.AddJoin";
                    var started = DateTime.Now;
                    var result = await GeoprocessingCommands.RunNamedAsync(
                        toolName,
                        new Dictionary<string, object>
                        {
                            ["in_layer_or_view"] = layerName,
                            ["in_field"] = parameters.Require("layer_field"),
                            ["join_table"] = parameters.Require("join_table"),
                            ["join_field"] = parameters.Require("join_field"),
                            ["join_type"] = parameters.GetString("keep_all", "KEEP_ALL"),
                        },
                        addToMap: false).ConfigureAwait(false);

                    if (result.IsFailed)
                    {
                        var messages = result.ErrorMessages != null
                                       && result.ErrorMessages.Any()
                            ? string.Join("; ", result.ErrorMessages.Select(m => m.Text))
                            : "the tool reported no error text";
                        throw new InvalidOperationException($"add_join failed: {messages}");
                    }

                    JoinRegistry.Add(layerName, parameters.GetString("join_table"),
                                     parameters.GetString("layer_field"),
                                     parameters.GetString("join_field"), basePath,
                                     joinFields);

                    return new Dictionary<string, object>
                    {
                        ["command"] = "add_join",
                        ["tool"] = toolName,
                        ["outputs"] = result.Values?.ToList(),
                        ["elapsed_seconds"] = Math.Round((DateTime.Now - started).TotalSeconds, 2),
                        ["messages"] = result.Messages?.Select(m => m.Text).Take(20).ToList(),
                        ["base_path"] = basePath,
                    };
                }, lane: ExecutionLane.Geoprocessing);
        }

        /// <summary>
        /// Pro attaches a tool's output to the map a moment after the tool
        /// returns. Callers that act on the new table right away (insert into
        /// it, count it) used to race that attachment; wait until the member
        /// is visible, at most two seconds.
        /// </summary>
        private static async Task<bool> WaitForMapMemberAsync(Params parameters,
                                                             List<string> outputs)
        {
            var wanted = outputs
                .Where(o => !string.IsNullOrWhiteSpace(o) && o.Contains("\\"))
                .Select(o => { try { return Path.GetFullPath(o); } catch { return o; } })
                .ToList();
            if (wanted.Count == 0) return true;

            for (var attempt = 0; attempt < 20; attempt++)
            {
                var found = await QueuedTask.Run(() =>
                {
                    Map map;
                    try { map = MapHelpers.ResolveMap(parameters); }
                    catch { return false; }
                    if (map == null) return false;

                    bool Matches(string path)
                    {
                        if (string.IsNullOrWhiteSpace(path)) return false;
                        try { path = Path.GetFullPath(path); } catch { }
                        return wanted.Any(w => string.Equals(w, path,
                                                             StringComparison.OrdinalIgnoreCase));
                    }

                    if (map.GetLayersAsFlattenedList().OfType<BasicFeatureLayer>()
                           .Any(l => Matches(l.GetPath()?.LocalPath)))
                        return true;
                    return map.GetStandaloneTablesAsFlattenedList()
                              .Any(t => Matches(t.GetPath()?.LocalPath));
                }).ConfigureAwait(false);

                if (found) return true;
                await Task.Delay(100).ConfigureAwait(false);
            }
            return false;
        }

        /// <summary>The layer name or dataset path a command acts on.</summary>
        private static string Target(Params parameters) => parameters.Require("layer_name");

        private static void Wrap(string command, string group, string summary,
                                 string toolName,
                                 Func<Params, Dictionary<string, object>> buildValues,
                                 bool addToMapByDefault = false,
                                 Func<Params, List<string>, Task> afterSuccess = null)
        {
            CommandRouter.RegisterAsync(command, group, summary, async parameters =>
            {
                var named = buildValues(parameters);
                var addToMap = parameters.Has("add_to_map")
                    ? parameters.GetBool("add_to_map")
                    : addToMapByDefault;

                var started = DateTime.Now;
                var result = await GeoprocessingCommands
                    .RunNamedAsync(toolName, named, addToMap).ConfigureAwait(false);

                if (result.IsFailed)
                {
                    var messages = result.ErrorMessages != null && result.ErrorMessages.Any()
                        ? string.Join("; ", result.ErrorMessages.Select(m => m.Text))
                        : "the tool reported no error text";
                    throw new InvalidOperationException($"{command} failed: {messages}");
                }

                var mapReady = true;
                if (addToMap)
                {
                    var outputTexts = result.Values?
                        .Select(value => value?.ToString())
                        .Where(text => !string.IsNullOrEmpty(text))
                        .ToList() ?? new List<string>();
                    mapReady = await WaitForMapMemberAsync(parameters, outputTexts)
                        .ConfigureAwait(false);
                }

                if (afterSuccess != null)
                {
                    var outputTexts = result.Values?
                        .Select(value => value?.ToString())
                        .Where(text => !string.IsNullOrEmpty(text))
                        .ToList() ?? new List<string>();
                    await afterSuccess(parameters, outputTexts).ConfigureAwait(false);
                }

                return new Dictionary<string, object>
                {
                    ["command"] = command,
                    ["tool"] = toolName,
                    ["outputs"] = result.Values?
                        .Select(value => value?.ToString())
                        .Where(text => !string.IsNullOrEmpty(text))
                        .ToList(),
                    ["elapsed_seconds"] = Math.Round((DateTime.Now - started).TotalSeconds, 2),
                    ["messages"] = result.Messages?.Select(m => m.Text).Take(20).ToList(),
                    ["map_ready"] = mapReady,
                };
            }, lane: ExecutionLane.Geoprocessing);
        }

        // --- fields support for create_feature_class / create_table ----------
        //
        // CreateFeatureclass and CreateTable accept no fields parameter in
        // arcpy, although the tool catalog documents one. The fields are
        // therefore added here, one AddField per field, and read back before
        // success is reported: a dataset with the wrong columns must never
        // come back as a success (D1, live-test 2026-11-16 and 2026-11-18).

        private static readonly Dictionary<string, string[]> FieldTypeAliases =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                // Keys are the GP/CreateField type names callers pass; values
                // are the names ArcGIS.Core Field.FieldType reports (Describe).
                ["TEXT"] = new[] { "String", "Text" },
                ["SHORT"] = new[] { "SmallInteger", "Short" },
                ["LONG"] = new[] { "Integer", "Long" },
                ["BIGINTEGER"] = new[] { "BigInteger", "BigInteger64" },
                ["FLOAT"] = new[] { "Single", "Float" },
                ["DOUBLE"] = new[] { "Double" },
                ["DATE"] = new[] { "Date" },
                ["GUID"] = new[] { "GUID" },
                ["BLOB"] = new[] { "Blob" },
            };

        private sealed class FieldRequest
        {
            public string Name;
            public string Type;
            public string Precision;
            public string Scale;
            public string Length;
            public string Alias;
        }

        private static List<FieldRequest> ParseFieldRequests(Params parameters)
        {
            var requests = new List<FieldRequest>();

            foreach (var field in parameters.GetObjectList("fields"))
            {
                var name = field.Require("name").Trim();
                if (!Regex.IsMatch(name, "^[A-Za-z_][A-Za-z0-9_]*$"))
                    throw new ArgumentException(
                        $"Invalid field name '{name}': use letters, digits and "
                        + "underscores, starting with a letter or underscore.");

                var type = field.GetString("type", "TEXT").Trim().ToUpperInvariant();
                if (!FieldTypeAliases.ContainsKey(type))
                    throw new ArgumentException(
                        $"Unsupported field type '{type}' for '{name}'. Supported: "
                        + string.Join(", ", FieldTypeAliases.Keys) + ".");

                requests.Add(new FieldRequest
                {
                    Name = name,
                    Type = type,
                    Precision = field.GetString("precision"),
                    Scale = field.GetString("scale"),
                    Length = field.GetString("length"),
                    Alias = field.GetString("alias"),
                });
            }

            return requests;
        }

        private static async Task ApplyAndVerifyFieldsAsync(Params parameters,
                                                            string datasetPath)
        {
            if (!parameters.Has("fields")) return;
            var requests = ParseFieldRequests(parameters);
            if (requests.Count == 0) return;

            if (string.IsNullOrEmpty(datasetPath))
            {
                throw new InvalidOperationException(
                    "fields: the tool created a dataset but returned no output path, "
                    + "so the requested fields cannot be applied or verified.");
            }

            // Capture the map that received the output -- and the objects that
            // reference it -- before any field work. By the time a failure
            // happens the active view may have changed, and cleanup has to act
            // on the map the layer/table actually lives in.
            var captured = await CaptureCreatedOutputAsync(datasetPath).ConfigureAwait(false);

            var added = new List<string>();
            foreach (var request in requests)
            {
                bool failed = false;
                var failureText = "";
                try
                {
                    var result = await GeoprocessingCommands.RunNamedAsync(
                        "management.AddField",
                        new Dictionary<string, object>
                        {
                            ["in_table"] = datasetPath,
                            ["field_name"] = request.Name,
                            ["field_type"] = request.Type,
                            ["field_precision"] = request.Precision,
                            ["field_scale"] = request.Scale,
                            ["field_length"] = request.Length,
                            ["field_alias"] = request.Alias,
                            ["field_is_nullable"] = "NULLABLE",
                            ["field_domain"] = null,
                        },
                        addToMap: false).ConfigureAwait(false);

                    if (result.IsFailed)
                    {
                        failed = true;
                        failureText = result.ErrorMessages != null
                                      && result.ErrorMessages.Any()
                            ? string.Join("; ", result.ErrorMessages.Select(m => m.Text))
                            : "the tool reported no error text";
                    }
                }
                catch (Exception exception)
                {
                    failed = true;
                    failureText = "threw: " + exception.Message;
                }

                if (failed)
                {
                    var cleanup = await DeleteHalfCreatedAsync(datasetPath, captured)
                        .ConfigureAwait(false);
                    throw new InvalidOperationException(
                        $"fields: adding '{request.Name}' failed ({failureText}). "
                        + (cleanup.DatasetDeleted
                            ? "The dataset created by this call was deleted so no "
                              + "half-configured dataset is left behind. "
                            : "The dataset created by this call could not be deleted "
                              + "and may still exist. ")
                        + "Fields added before the failure: "
                        + (added.Count == 0 ? "(none)" : string.Join(", ", added)) + "."
                        + CleanupSuffix(cleanup.Problems));
                }
                added.Add(request.Name);
            }

            // Read back and verify -- never report success for a schema the
            // caller did not ask for.
            Dictionary<string, object> info;
            try
            {
                info = await QueuedTask.Run(
                    () => CatalogCommands.DescribeDataset(datasetPath))
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                var cleanup = await DeleteHalfCreatedAsync(datasetPath, captured)
                    .ConfigureAwait(false);
                throw new InvalidOperationException(
                    "fields: the dataset was created but could not be read back "
                    + $"for verification ({exception.Message}). "
                    + (cleanup.DatasetDeleted
                        ? "It was deleted."
                        : "Deleting it failed — it may still exist.")
                    + CleanupSuffix(cleanup.Problems));
            }

            var actual = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var actualLengths = new Dictionary<string, object>(
                StringComparer.OrdinalIgnoreCase);
            if (info != null && info.TryGetValue("fields", out var raw)
                && raw is List<Dictionary<string, object>> fieldList)
            {
                foreach (var entry in fieldList)
                {
                    var fieldName = entry.TryGetValue("name", out var n) ? n?.ToString() : null;
                    if (string.IsNullOrEmpty(fieldName)) continue;
                    actual[fieldName] = entry.TryGetValue("type", out var t)
                        ? t?.ToString() : null;
                    actualLengths[fieldName] = entry.TryGetValue("length", out var l)
                        ? l : null;
                }
            }

            var problems = new List<string>();
            foreach (var request in requests)
            {
                if (!actual.TryGetValue(request.Name, out var actualType))
                {
                    problems.Add($"'{request.Name}' is missing");
                    continue;
                }

                var aliases = FieldTypeAliases[request.Type];
                if (!aliases.Any(a => string.Equals(a, actualType,
                                                    StringComparison.OrdinalIgnoreCase)))
                {
                    problems.Add($"'{request.Name}' is {actualType}, requested {request.Type}");
                    continue;
                }

                if (string.Equals(request.Type, "TEXT", StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(request.Length, out var wantedLength))
                {
                    var actualLength = actualLengths.TryGetValue(request.Name, out var l)
                        ? Convert.ToInt32(l ?? 0) : 0;
                    if (actualLength != wantedLength)
                        problems.Add($"'{request.Name}' length is {actualLength}, "
                                     + $"requested {wantedLength}");
                }
            }

            if (problems.Count > 0)
            {
                var cleanup = await DeleteHalfCreatedAsync(datasetPath, captured)
                    .ConfigureAwait(false);
                throw new InvalidOperationException(
                    "fields: verification failed after creation: "
                    + string.Join("; ", problems)
                    + (cleanup.DatasetDeleted
                        ? ". The dataset was deleted (fail-loud, no half-configured "
                          + "dataset is kept)."
                        : ". Deleting the dataset failed — it may still exist.")
                    + CleanupSuffix(cleanup.Problems));
            }
        }

        private sealed class CleanupResult
        {
            public bool DatasetDeleted;
            public readonly List<string> Problems = new List<string>();
        }

        /// <summary>
        /// The layers and standalone tables that reference this call's new
        /// dataset, each bound to the map it lives in, captured before field
        /// work begins. Scanning every map in the project -- rather than only
        /// the active view -- means cleanup never depends on which map the
        /// user happens to be looking at when a failure happens.
        /// </summary>
        private sealed class CreatedOutput
        {
            public readonly List<KeyValuePair<Map, Layer>> Layers =
                new List<KeyValuePair<Map, Layer>>();
            public readonly List<KeyValuePair<Map, StandaloneTable>> Tables =
                new List<KeyValuePair<Map, StandaloneTable>>();
            public readonly List<string> Problems = new List<string>();
        }

        /// <summary>
        /// Resolve, while the dataset still exists, which map members reference
        /// it and which map each lives in. Later failures act on this capture.
        /// </summary>
        private static async Task<CreatedOutput> CaptureCreatedOutputAsync(string datasetPath)
        {
            var captured = new CreatedOutput();
            try
            {
                await QueuedTask.Run(() =>
                {
                    var project = Project.Current;
                    if (project == null) return;
                    foreach (var item in project.GetItems<MapProjectItem>())
                    {
                        var map = item.GetMap();
                        if (map == null) continue;
                        foreach (var layer in map.GetLayersAsFlattenedList().ToList())
                        {
                            if (!(layer is BasicFeatureLayer basic)) continue;
                            try
                            {
                                using (var table = basic.GetTable())
                                {
                                    if (PathEquals(table?.GetPath()?.LocalPath, datasetPath))
                                        captured.Layers.Add(
                                            new KeyValuePair<Map, Layer>(map, layer));
                                }
                            }
                            catch (Exception exception)
                            {
                                captured.Problems.Add(
                                    $"reading the data source of layer '{layer.Name}' "
                                    + $"in map '{map.Name}' failed: " + exception.Message);
                            }
                        }
                        foreach (var table in map.GetStandaloneTablesAsFlattenedList().ToList())
                        {
                            try
                            {
                                using (var source = table.GetTable())
                                {
                                    if (PathEquals(source?.GetPath()?.LocalPath, datasetPath))
                                        captured.Tables.Add(
                                            new KeyValuePair<Map, StandaloneTable>(map, table));
                                }
                            }
                            catch (Exception exception)
                            {
                                captured.Problems.Add(
                                    $"reading the data source of table '{table.Name}' "
                                    + $"in map '{map.Name}' failed: " + exception.Message);
                            }
                        }
                    }
                }).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                captured.Problems.Add("inspecting the project maps failed: " + exception.Message);
            }
            return captured;
        }

        /// <summary>
        /// Remove a dataset this call just created, plus the map layers and
        /// standalone tables captured for it before field work began. Objects
        /// are removed by identity from the map they were captured on, so
        /// cleanup never guesses by name and never depends on which view is
        /// active when the failure happens; every failed step is returned so
        /// the throw that follows can report it.
        /// </summary>
        private static async Task<CleanupResult> DeleteHalfCreatedAsync(
            string datasetPath, CreatedOutput captured)
        {
            var cleanup = new CleanupResult();
            cleanup.Problems.AddRange(captured.Problems);

            // Delete the dataset itself.
            try
            {
                var result = await GeoprocessingCommands.RunNamedAsync(
                    "management.Delete",
                    new Dictionary<string, object> { ["in_data"] = datasetPath },
                    addToMap: false).ConfigureAwait(false);
                if (result.IsFailed)
                {
                    var text = result.ErrorMessages != null && result.ErrorMessages.Any()
                        ? string.Join("; ", result.ErrorMessages.Select(m => m.Text))
                        : "the tool reported no error text";
                    cleanup.Problems.Add("deleting the dataset failed: " + text);
                }
                else
                {
                    cleanup.DatasetDeleted = true;
                }
            }
            catch (Exception exception)
            {
                cleanup.Problems.Add("deleting the dataset failed: " + exception.Message);
            }

            // Remove exactly the captured objects, each from the map it lives in.
            if (captured.Layers.Count > 0 || captured.Tables.Count > 0)
            {
                try
                {
                    await QueuedTask.Run(() =>
                    {
                        foreach (var pair in captured.Layers)
                        {
                            try { pair.Key.RemoveLayer(pair.Value); }
                            catch (Exception exception)
                            {
                                cleanup.Problems.Add(
                                    $"removing layer '{pair.Value.Name}' from map "
                                    + $"'{pair.Key.Name}' failed: " + exception.Message);
                            }
                        }
                        foreach (var group in captured.Tables.GroupBy(p => p.Key))
                        {
                            try
                            {
                                group.Key.RemoveStandaloneTables(
                                    group.Select(p => p.Value).ToList());
                            }
                            catch (Exception exception)
                            {
                                cleanup.Problems.Add(
                                    $"removing standalone tables from map "
                                    + $"'{group.Key.Name}' failed: " + exception.Message);
                            }
                        }
                    }).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    cleanup.Problems.Add("removing map objects failed: " + exception.Message);
                }
            }

            return cleanup;
        }

        private static bool PathEquals(string localPath, string datasetPath)
        {
            if (string.IsNullOrEmpty(localPath) || string.IsNullOrEmpty(datasetPath))
                return false;
            try
            {
                return string.Equals(Path.GetFullPath(localPath),
                                     Path.GetFullPath(datasetPath),
                                     StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return string.Equals(localPath, datasetPath,
                                     StringComparison.OrdinalIgnoreCase);
            }
        }

        private static string CleanupSuffix(List<string> problems) =>
            problems == null || problems.Count == 0
                ? ""
                : " Cleanup problems: " + string.Join("; ", problems) + ".";
    }
}
