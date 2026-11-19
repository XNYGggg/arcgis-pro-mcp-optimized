using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGISProMCP.Bridge;

namespace ArcGISProMCP.Commands
{
    /// <summary>
    /// Changing data: rows and schema.
    ///
    /// Row edits go through EditOperation rather than a cursor, so they land
    /// in ArcGIS Pro's own edit session and can be undone from the ribbon like
    /// anything the user did by hand.
    /// </summary>
    internal static class EditCommands
    {
        private const string Group = "data";

        public static void Register()
        {
            CommandRouter.RegisterAsync("insert_features", Group,
                "Insert rows into a layer or table.", InsertFeaturesAsync);
            CommandRouter.RegisterAsync("update_features", Group,
                "Update rows matching a where clause.", UpdateFeaturesAsync);
            CommandRouter.RegisterAsync("delete_features", Group,
                "Delete rows matching a where clause.", DeleteFeaturesAsync);
            CommandRouter.Register("list_fields", "schema",
                "List a layer's fields with type, alias and length.", ListFields);
            CommandRouter.RegisterAsync("save_edits", Group,
                "Commit pending edits. ArcGIS Pro holds edits open so they can be "
                + "undone, which also keeps the data locked until they are saved.",
                SaveEditsAsync);
            CommandRouter.RegisterAsync("discard_edits", Group,
                "Throw away pending edits.", DiscardEditsAsync);
        }

        // The edit bodies below run on the MCT through QueuedTask. Committing
        // pending edits touches UI-owned objects, so it is awaited afterwards
        // on the caller's thread; .Wait() from an MCT command deadlocks Pro.
        private static async Task<object> InsertFeaturesAsync(Params parameters)
        {
            var result = await QueuedTask.Run(() => InsertFeatures(parameters));
            await MaybeSaveEditsAsync(parameters).ConfigureAwait(false);
            return result;
        }

        private static async Task<object> UpdateFeaturesAsync(Params parameters)
        {
            var result = await QueuedTask.Run(() => UpdateFeatures(parameters));
            await MaybeSaveEditsAsync(parameters).ConfigureAwait(false);
            return result;
        }

        private static async Task<object> DeleteFeaturesAsync(Params parameters)
        {
            var result = await QueuedTask.Run(() => DeleteFeatures(parameters));
            await MaybeSaveEditsAsync(parameters).ConfigureAwait(false);
            return result;
        }

        private static BasicFeatureLayer Layer(Params parameters)
        {
            var map = MapHelpers.ResolveMap(parameters);
            return MapHelpers.RequireFeatureLayer(map, parameters.Require("layer_name"));
        }

        private static Geometry GeometryFrom(Params feature, SpatialReference reference)
        {
            var raw = feature.Raw("geometry");
            if (raw.ValueKind == System.Text.Json.JsonValueKind.Undefined
                || raw.ValueKind == System.Text.Json.JsonValueKind.Null) return null;

            if (raw.ValueKind == System.Text.Json.JsonValueKind.String)
                return GeometryEngine.Instance.ImportFromWKT(0, raw.GetString(), reference);

            if (raw.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                var numbers = raw.EnumerateArray()
                    .Where(v => v.ValueKind == System.Text.Json.JsonValueKind.Number)
                    .Select(v => v.GetDouble()).ToArray();
                if (numbers.Length >= 2)
                    return MapPointBuilderEx.CreateMapPoint(numbers[0], numbers[1], reference);
            }
            throw new ArgumentException(
                "geometry must be WKT text or an [x, y] pair.");
        }

        private static Dictionary<string, object> AttributesFrom(Params feature)
        {
            var attributes = new Dictionary<string, object>();
            var raw = feature.Raw("attributes");
            if (raw.ValueKind != System.Text.Json.JsonValueKind.Object) return attributes;

            foreach (var property in raw.EnumerateObject())
            {
                switch (property.Value.ValueKind)
                {
                    case System.Text.Json.JsonValueKind.String:
                        attributes[property.Name] = property.Value.GetString(); break;
                    case System.Text.Json.JsonValueKind.Number:
                        attributes[property.Name] = property.Value.GetDouble(); break;
                    case System.Text.Json.JsonValueKind.True:
                        attributes[property.Name] = true; break;
                    case System.Text.Json.JsonValueKind.False:
                        attributes[property.Name] = false; break;
                    case System.Text.Json.JsonValueKind.Null:
                        attributes[property.Name] = null; break;
                    default:
                        attributes[property.Name] = property.Value.GetRawText(); break;
                }
            }
            return attributes;
        }

        private static object InsertFeatures(Params parameters)
        {
            using var source = DataSource.Resolve(parameters);
            if (source.Layer == null)
                return InsertIntoSingleTable(parameters, source);

            var layer = source.Layer;
            var features = parameters.GetObjectList("features").ToList();
            if (features.Count == 0)
                throw new ArgumentException(
                    "features is required: a list of {attributes, geometry}.");

            var reference = layer.GetSpatialReference();
            var operation = new EditOperation { Name = $"MCP insert into {layer.Name}" };

            foreach (var feature in features)
            {
                var attributes = AttributesFrom(feature);
                var geometry = GeometryFrom(feature, reference);
                if (geometry != null) attributes["SHAPE"] = geometry;
                if (attributes.Count == 0)
                    throw new ArgumentException("Each feature needs attributes or geometry.");
                operation.Create(layer, attributes);
            }

            if (!operation.Execute())
                throw new InvalidOperationException(
                    $"The insert failed: {operation.ErrorMessage}");

            return new Dictionary<string, object>
            {
                ["layer"] = layer.Name,
                ["inserted"] = features.Count,
                ["edits_saved"] = parameters.GetBool("save_edits"),
            };
        }

        /// <summary>ObjectIDs matching a where clause, so edits can be targeted.</summary>
        private static List<long> ObjectIds(BasicFeatureLayer layer, string where, int limit)
        {
            var filter = new QueryFilter();
            if (!string.IsNullOrWhiteSpace(where)) filter.WhereClause = where;

            var oids = new List<long>();
            using (var table = layer.GetTable())
            {
                filter.SubFields = table.GetDefinition().GetObjectIDField();
                using (var cursor = layer.Search(filter))
                {
                    while (cursor.MoveNext())
                    {
                        using (var row = cursor.Current) oids.Add(row.GetObjectID());
                        if (limit > 0 && oids.Count >= limit) break;
                    }
                }
            }
            return oids;
        }

        private static object UpdateFeatures(Params parameters)
        {
            using var source = DataSource.Resolve(parameters);
            if (source.Layer == null)
                return UpdateSingleTable(parameters, source);

            var layer = source.Layer;
            var where = parameters.GetString("where");
            var attributes = AttributesFrom(new Params(parameters.Root));

            var updates = parameters.GetObject("attributes");
            if ((updates == null || updates.Count == 0) && !parameters.Has("geometry"))
                throw new ArgumentException("Provide attributes and/or geometry to update.");
            if (string.IsNullOrWhiteSpace(where) && !parameters.GetBool("allow_update_all"))
                throw new ArgumentException(
                    "Refusing to update every row: pass a where clause, or set "
                    + "allow_update_all=true to confirm.");

            var values = new Dictionary<string, object>();
            if (updates != null)
            {
                foreach (var pair in updates)
                {
                    values[pair.Key] = pair.Value.ValueKind switch
                    {
                        System.Text.Json.JsonValueKind.String => pair.Value.GetString(),
                        System.Text.Json.JsonValueKind.Number => (object)pair.Value.GetDouble(),
                        System.Text.Json.JsonValueKind.True => true,
                        System.Text.Json.JsonValueKind.False => false,
                        System.Text.Json.JsonValueKind.Null => null,
                        _ => pair.Value.GetRawText(),
                    };
                }
            }
            if (parameters.Has("geometry"))
                values["SHAPE"] = GeometryFrom(parameters, layer.GetSpatialReference());

            var oids = ObjectIds(layer, where, parameters.GetInt("limit"));
            if (oids.Count == 0)
                return new Dictionary<string, object>
                {
                    ["layer"] = layer.Name, ["updated"] = 0, ["where"] = where,
                };

            var operation = new EditOperation { Name = $"MCP update {layer.Name}" };
            foreach (var oid in oids) operation.Modify(layer, oid, values);
            if (!operation.Execute())
                throw new InvalidOperationException(
                    $"The update failed: {operation.ErrorMessage}");

            return new Dictionary<string, object>
            {
                ["layer"] = layer.Name,
                ["updated"] = oids.Count,
                ["where"] = where,
                ["edits_saved"] = parameters.GetBool("save_edits"),
            };
        }

        private static object DeleteFeatures(Params parameters)
        {
            using var source = DataSource.Resolve(parameters);
            if (source.Layer == null)
                return DeleteFromSingleTable(parameters, source);

            var layer = source.Layer;
            var where = parameters.GetString("where");
            if (string.IsNullOrWhiteSpace(where) && !parameters.GetBool("allow_delete_all"))
                throw new ArgumentException(
                    "Refusing to delete every row: pass a where clause, or set "
                    + "allow_delete_all=true to confirm.");

            var oids = ObjectIds(layer, where, 0);
            if (oids.Count == 0)
                return new Dictionary<string, object>
                {
                    ["layer"] = layer.Name, ["deleted"] = 0, ["where"] = where,
                };

            var operation = new EditOperation { Name = $"MCP delete from {layer.Name}" };
            operation.Delete(layer, oids);
            if (!operation.Execute())
                throw new InvalidOperationException(
                    $"The delete failed: {operation.ErrorMessage}");

            return new Dictionary<string, object>
            {
                ["layer"] = layer.Name,
                ["deleted"] = oids.Count,
                ["where"] = where,
                ["edits_saved"] = parameters.GetBool("save_edits"),
            };
        }

        /// <summary>
        /// Pro keeps edits pending so the user can undo them, but a dataset
        /// with pending edits is locked -- deleting or overwriting it fails
        /// until they are saved or discarded.
        /// </summary>
        private static async Task<object> SaveEditsAsync(Params parameters)
        {
            var project = Project.Current
                ?? throw new InvalidOperationException("No project is open.");
            var had = project.HasEdits;
            if (had) await MapCommands.SaveEditsOnUiThread().ConfigureAwait(false);
            return new Dictionary<string, object>
            {
                ["saved"] = had,
                ["message"] = had ? "Pending edits committed."
                                  : "There were no pending edits.",
            };
        }

        private static async Task<object> DiscardEditsAsync(Params parameters)
        {
            var project = Project.Current
                ?? throw new InvalidOperationException("No project is open.");
            var had = project.HasEdits;
            if (had) await MapCommands.DiscardEditsOnUiThread().ConfigureAwait(false);
            return new Dictionary<string, object>
            {
                ["discarded"] = had,
                ["message"] = had ? "Pending edits discarded."
                                  : "There were no pending edits.",
            };
        }

        // --- standalone tables and dataset paths (D2, live test 2026-11-18) --
        //
        // EditOperation is layer-shaped. A standalone table (or a table opened
        // by path) has no layer to hang an edit session on, so these rows are
        // written through the Table API. They are immediate: there is no
        // pending edit to undo, and the result says so.

        private static List<long> TableObjectIds(DataSource source, string where, int limit)
        {
            var oidField = source.Definition.GetObjectIDField();
            var filter = new QueryFilter { SubFields = oidField };
            if (!string.IsNullOrWhiteSpace(where)) filter.WhereClause = where;

            var oids = new List<long>();
            using var cursor = source.Search(filter);
            while (cursor.MoveNext())
            {
                using var row = cursor.Current;
                oids.Add(row.GetObjectID());
                if (limit > 0 && oids.Count >= limit) break;
            }
            return oids;
        }

        private static Dictionary<string, object> AttributeValues(Params parameters)
        {
            var values = new Dictionary<string, object>();
            var updates = parameters.GetObject("attributes");
            if (updates == null) return values;
            foreach (var pair in updates)
            {
                values[pair.Key] = pair.Value.ValueKind switch
                {
                    System.Text.Json.JsonValueKind.String => pair.Value.GetString(),
                    System.Text.Json.JsonValueKind.Number => (object)pair.Value.GetDouble(),
                    System.Text.Json.JsonValueKind.True => true,
                    System.Text.Json.JsonValueKind.False => false,
                    System.Text.Json.JsonValueKind.Null => null,
                    _ => pair.Value.GetRawText(),
                };
            }
            return values;
        }

        private static object InsertIntoSingleTable(Params parameters, DataSource source)
        {
            var features = parameters.GetObjectList("features").ToList();
            if (features.Count == 0)
                throw new ArgumentException(
                    "features is required: a list of {attributes, geometry}.");

            var reference = source.SpatialReference;
            var table = source.Table;
            var inserted = 0;
            foreach (var feature in features)
            {
                var attributes = AttributesFrom(feature);
                if (table is FeatureClass)
                {
                    var geometry = GeometryFrom(feature, reference);
                    if (geometry != null) attributes["SHAPE"] = geometry;
                }
                if (attributes.Count == 0)
                    throw new ArgumentException(
                        $"Feature {inserted + 1} needs attributes"
                        + (table is FeatureClass ? " or geometry." : "."));

                using var buffer = table.CreateRowBuffer();
                foreach (var pair in attributes) buffer[pair.Key] = pair.Value;
                using var row = table.CreateRow(buffer);
                row.Store();
                inserted++;
            }

            return new Dictionary<string, object>
            {
                ["layer"] = source.Name,
                ["inserted"] = inserted,
                ["target"] = "table",
                ["edits_saved"] = true,
                ["note"] = "table rows are written directly; there is no edit session to undo",
            };
        }

        private static object UpdateSingleTable(Params parameters, DataSource source)
        {
            var where = parameters.GetString("where");
            var values = AttributeValues(parameters);
            if (parameters.Has("geometry"))
                values["SHAPE"] = GeometryFrom(parameters, source.SpatialReference);
            if (values.Count == 0)
                throw new ArgumentException("Provide attributes and/or geometry to update.");
            if (string.IsNullOrWhiteSpace(where) && !parameters.GetBool("allow_update_all"))
                throw new ArgumentException(
                    "Refusing to update every row: pass a where clause, or set "
                    + "allow_update_all=true to confirm.");

            var oids = TableObjectIds(source, where, parameters.GetInt("limit"));
            var oidField = source.Definition.GetObjectIDField();
            var updated = 0;
            foreach (var oid in oids)
            {
                using var cursor = source.Search(new QueryFilter
                {
                    WhereClause = $"{oidField} = {oid}",
                });
                if (!cursor.MoveNext()) continue;
                using var row = cursor.Current;
                foreach (var pair in values) row[pair.Key] = pair.Value;
                row.Store();
                updated++;
            }

            return new Dictionary<string, object>
            {
                ["layer"] = source.Name,
                ["updated"] = updated,
                ["where"] = where,
                ["target"] = "table",
                ["edits_saved"] = true,
            };
        }

        private static object DeleteFromSingleTable(Params parameters, DataSource source)
        {
            var where = parameters.GetString("where");
            if (string.IsNullOrWhiteSpace(where) && !parameters.GetBool("allow_delete_all"))
                throw new ArgumentException(
                    "Refusing to delete every row: pass a where clause, or set "
                    + "allow_delete_all=true to confirm.");

            var oids = TableObjectIds(source, where, 0);
            var oidField = source.Definition.GetObjectIDField();
            var deleted = 0;
            foreach (var oid in oids)
            {
                using var cursor = source.Search(new QueryFilter
                {
                    WhereClause = $"{oidField} = {oid}",
                });
                if (!cursor.MoveNext()) continue;
                using var row = cursor.Current;
                row.Delete();
                deleted++;
            }

            return new Dictionary<string, object>
            {
                ["layer"] = source.Name,
                ["deleted"] = deleted,
                ["where"] = where,
                ["target"] = "table",
                ["edits_saved"] = true,
            };
        }

        /// <summary>Commit straight away when the caller asked for it.</summary>
        private static async Task MaybeSaveEditsAsync(Params parameters)
        {
            if (!parameters.GetBool("save_edits")) return;
            var project = Project.Current;
            if (project != null && project.HasEdits)
                await MapCommands.SaveEditsOnUiThread().ConfigureAwait(false);
        }

        private static object ListFields(Params parameters)
        {
            using var source = DataSource.Resolve(parameters);
            var wildcard = parameters.GetString("wildcard")?.Trim('*');

            List<Dictionary<string, object>> fields;
            try
            {
                fields = source.Definition.GetFields()
                    .Where(field => string.IsNullOrEmpty(wildcard)
                                    || field.Name.IndexOf(
                                        wildcard, StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(MapHelpers.Describe).ToList();
            }
            catch (Exception) when (source.Layer != null
                                    && JoinRegistry.HasJoins(source.Layer.Name))
            {
                // A joined layer's table view fails .NET validation; describe
                // the base dataset by path and append the joined names (J1/J2).
                using var baseSource = DataCommands.OpenByName(
                    source.Layer.GetPath()?.LocalPath);
                if (baseSource == null) throw;

                fields = baseSource.Definition.GetFields()
                    .Where(field => string.IsNullOrEmpty(wildcard)
                                    || field.Name.IndexOf(
                                        wildcard, StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(MapHelpers.Describe).ToList();

                foreach (var join in JoinRegistry.Snapshot(source.Layer.Name))
                {
                    foreach (var name in join.Fields)
                    {
                        if (!string.IsNullOrEmpty(wildcard)
                            && name.IndexOf(wildcard, StringComparison.OrdinalIgnoreCase) < 0)
                            continue;
                        fields.Add(new Dictionary<string, object>
                        {
                            ["name"] = join.Table + "." + name,
                            ["alias"] = name,
                            ["type"] = "Joined",
                            ["length"] = 0,
                            ["nullable"] = true,
                            ["editable"] = false,
                        });
                    }
                }
            }

            return new Dictionary<string, object>
            {
                ["layer"] = source.Name,
                ["fields"] = fields,
            };
        }
    }
}
