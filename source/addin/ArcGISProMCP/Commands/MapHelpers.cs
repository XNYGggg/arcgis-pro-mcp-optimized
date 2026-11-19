using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Mapping;
using ArcGISProMCP.Bridge;

namespace ArcGISProMCP.Commands
{
    /// <summary>
    /// Lookups and serialisation shared by the command modules.
    /// Everything here assumes it is already on the MCT.
    /// </summary>
    internal static class MapHelpers
    {
        public static Project RequireProject()
        {
            var project = Project.Current;
            if (project == null)
                throw new InvalidOperationException(
                    "No project is open in ArcGIS Pro. Open one and try again.");
            return project;
        }

        /// <summary>The map named in map_name, or the one the user is looking at.</summary>
        public static Map ResolveMap(Params parameters)
        {
            var requested = parameters?.GetString("map_name");
            var project = RequireProject();
            var items = project.GetItems<MapProjectItem>().ToList();

            if (!string.IsNullOrWhiteSpace(requested))
            {
                var match = items.FirstOrDefault(
                    item => string.Equals(item.Name, requested, StringComparison.OrdinalIgnoreCase));
                if (match == null)
                {
                    throw new ArgumentException(
                        $"Map not found: {requested}. Available maps: "
                        + (items.Count == 0 ? "(none)" : string.Join(", ", items.Select(i => i.Name))));
                }
                return match.GetMap();
            }

            var active = MapView.Active?.Map;
            if (active != null) return active;

            var first = items.FirstOrDefault();
            if (first == null)
                throw new InvalidOperationException("The project has no maps.");
            return first.GetMap();
        }

        public static MapView RequireActiveView(Map map)
        {
            var view = MapView.Active;
            if (view == null)
                throw new InvalidOperationException(
                    "No map view is active in ArcGIS Pro. Open the map's tab and try again.");
            if (map != null && view.Map != null && view.Map.URI != map.URI)
                throw new InvalidOperationException(
                    $"The active view is showing '{view.Map.Name}', not '{map.Name}'. "
                    + "Activate that map's tab first.");
            return view;
        }

        public static Layer FindLayer(Map map, string name, bool required = true)
        {
            var layers = map.GetLayersAsFlattenedList();
            var match = layers.FirstOrDefault(l => l.Name == name)
                        ?? layers.FirstOrDefault(
                            l => string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase));
            if (match != null) return match;

            // A dataset path, as long as exactly one loaded layer points at
            // it: the read commands accept paths, so layer property and
            // symbology commands should too (I1/R1, live test 2026-11-18).
            if (name.IndexOf('\\') >= 0 || name.IndexOf('/') >= 0)
            {
                var wanted = FullPath(name);
                var matches = layers.Where(l =>
                {
                    var path = l.GetPath()?.LocalPath;
                    return path != null && string.Equals(
                        FullPath(path), wanted, StringComparison.OrdinalIgnoreCase);
                }).ToList();

                if (matches.Count == 1) return matches[0];
                if (matches.Count > 1)
                    throw new ArgumentException(
                        $"'{name}' matches {matches.Count} loaded layers: "
                        + string.Join(", ", matches.Select(l => l.Name))
                        + ". Use the layer name instead.");
            }

            if (!required) return null;

            throw new ArgumentException(
                $"Layer not found in map '{map.Name}': {name}. Layers present: "
                + (layers.Count == 0 ? "(none)" : string.Join(", ", layers.Select(l => l.Name))));
        }

        private static string FullPath(string path)
        {
            try { return Path.GetFullPath(path); }
            catch { return path; }
        }

        public static BasicFeatureLayer RequireFeatureLayer(Map map, string name)
        {
            if (FindLayer(map, name) is BasicFeatureLayer layer) return layer;
            throw new ArgumentException($"Layer '{name}' holds no features.");
        }

        /// <summary>
        /// A layer's extent for zooming and framing: any layer type, group
        /// layers by union of their contents, and null when nothing answers.
        /// </summary>
        public static Envelope ExtentOf(Layer layer, int depth = 0)
        {
            if (layer == null) return null;
            try
            {
                var extent = layer.QueryExtent();
                if (extent != null && !extent.IsEmpty) return extent;
            }
            catch
            {
                // Raster/TIN layers may not answer QueryExtent; fall through.
            }

            if (layer is GroupLayer group && depth < 10)
            {
                Envelope union = null;
                foreach (var child in group.Layers)
                {
                    var extent = ExtentOf(child, depth + 1);
                    if (extent == null) continue;
                    union = union == null ? extent : union.Union(extent);
                }
                return union;
            }
            return null;
        }

        // --- serialisation ---------------------------------------------------

        public static Dictionary<string, object> Describe(Layer layer, int? index = null)
        {
            var info = new Dictionary<string, object>
            {
                ["name"] = layer.Name,
                ["visible"] = layer.IsVisible,
                ["is_group"] = layer is GroupLayer,
                ["is_feature_layer"] = layer is FeatureLayer,
                ["is_raster_layer"] = layer is RasterLayer,
                ["is_basemap"] = layer is TiledServiceLayer || layer is VectorTileLayer,
                ["layer_type"] = layer.GetType().Name,
            };
            if (index.HasValue) info["index"] = index.Value;

            try
            {
                if (layer is BasicFeatureLayer basic)
                {
                    info["definition_query"] = string.IsNullOrEmpty(basic.DefinitionQuery)
                        ? null : basic.DefinitionQuery;
                }
                if (layer is FeatureLayer feature)
                {
                    info["geometry_type"] = feature.ShapeType.ToString();
                }
            }
            catch (Exception exception)
            {
                info["describe_warning"] = exception.Message;
            }

            try
            {
                using (var dataset = (layer as BasicFeatureLayer)?.GetTable())
                {
                    if (dataset != null)
                    {
                        info["data_source"] = dataset.GetPath()?.LocalPath ?? dataset.GetName();
                    }
                }
            }
            catch
            {
                // Service layers and joins do not always expose a path.
            }

            return info;
        }

        public static Dictionary<string, object> Describe(SpatialReference reference)
        {
            if (reference == null) return null;
            return new Dictionary<string, object>
            {
                ["name"] = reference.Name,
                ["wkid"] = reference.Wkid,
                ["is_projected"] = reference.IsProjected,
                ["unit"] = reference.Unit?.Name,
            };
        }

        public static Dictionary<string, object> Describe(Envelope envelope)
        {
            if (envelope == null || envelope.IsEmpty) return null;
            return new Dictionary<string, object>
            {
                ["xmin"] = envelope.XMin,
                ["ymin"] = envelope.YMin,
                ["xmax"] = envelope.XMax,
                ["ymax"] = envelope.YMax,
                ["spatial_reference"] = envelope.SpatialReference?.Name,
                ["wkid"] = envelope.SpatialReference?.Wkid,
            };
        }

        public static Dictionary<string, object> Describe(Field field)
        {
            return new Dictionary<string, object>
            {
                ["name"] = field.Name,
                ["alias"] = field.AliasName,
                ["type"] = field.FieldType.ToString(),
                ["length"] = field.Length,
                ["nullable"] = field.IsNullable,
                ["editable"] = field.IsEditable,
            };
        }

        public static string ToWkt(Geometry geometry)
        {
            if (geometry == null) return null;
            return GeometryEngine.Instance.ExportToWKT(WktExportFlags.WktExportDefaults, geometry);
        }

        /// <summary>SHA-256 of a file, upper-case hex; null when it cannot be read.</summary>
        public static string Sha256(string path)
        {
            try
            {
                using (var stream = File.OpenRead(path))
                using (var sha = SHA256.Create())
                    return BitConverter.ToString(sha.ComputeHash(stream))
                                       .Replace("-", string.Empty).ToUpperInvariant();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Wait briefly for an export to appear on disk. Pro's Export can
        /// return before the file has been flushed, and a too-early
        /// File.Exists check silently drops the inline image or its size.
        /// </summary>
        public static bool WaitForFile(string path, int timeoutMs = 2000)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    if (File.Exists(path) && new FileInfo(path).Length > 0)
                        return true;
                }
                catch { }
                System.Threading.Thread.Sleep(50);
            }
            try { return File.Exists(path); } catch { return false; }
        }

        /// <summary>Row values that survive JSON: dates and geometry need help.</summary>
        public static object Normalise(object value)
        {
            switch (value)
            {
                case null: return null;
                case DateTime date: return date.ToString("yyyy-MM-dd HH:mm:ss");
                case Guid guid: return guid.ToString();
                case Geometry geometry: return ToWkt(geometry);
                default: return value;
            }
        }
    }
}
