using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Mapping;
using ArcGISProMCP.Bridge;

namespace ArcGISProMCP.Commands
{
    /// <summary>Project and map housekeeping, bookmarks, and GP environments.</summary>
    internal static class MapCommands
    {
        private const string Group = "project";

        /// <summary>
        /// Geoprocessing environments. ArcGIS Pro has no ambient arcpy.env for
        /// an add-in, so these are held here and passed to each tool run.
        /// </summary>
        public static readonly Dictionary<string, object> Environment =
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The environment keys this bridge forwards to tool runs, mapped to
        /// the GP environment names. Anything set through set_environment but
        /// missing here is still stored (and reported by get_environment under
        /// _unsupported) but never reaches a tool.
        /// </summary>
        public static readonly Dictionary<string, string> GpEnvironmentNames =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["workspace"] = "workspace",
                ["outputCoordinateSystem"] = "outputCoordinateSystem",
                ["extent"] = "extent",
                ["mask"] = "mask",
                ["cellSize"] = "cellSize",
                ["snapRaster"] = "snapRaster",
                ["overwriteOutput"] = "overwriteoutput",
                ["parallelProcessingFactor"] = "parallelProcessingFactor",
                ["scratchWorkspace"] = "scratchWorkspace",
                ["geographicTransformations"] = "geographicTransformations",
                ["resamplingMethod"] = "resamplingMethod",
            };

        public static void Register()
        {
            // Saving is a UI-thread operation, not an MCT one, so it is
            // registered async and dispatched explicitly.
            CommandRouter.RegisterAsync("save_project", Group,
                "Save the project, or save a copy elsewhere.", SaveProjectAsync);
            CommandRouter.RegisterAsync("create_map", Group,
                "Create a new map or scene. Created without a basemap unless one "
                + "is named (basemap=\"project_default\" keeps the old default).",
                CreateMapAsync);
            CommandRouter.Register("remove_map", Group,
                "Delete a map from the project.", RemoveMap);
            CommandRouter.RegisterAsync("activate_map", Group,
                "Open a map's view in the ArcGIS Pro UI.", ActivateMapAsync);
            CommandRouter.RegisterAsync("set_map_properties", Group,
                "Rename a map or change its coordinate system.",
                SetMapPropertiesAsync);
            CommandRouter.Register("get_environment", Group,
                "Read the geoprocessing environment: settings, which keys are "
                + "forwarded to tools (_forwarded) and which are not (_unsupported).",
                GetEnvironment, runOnMct: false);
            CommandRouter.Register("set_environment", Group,
                "Set geoprocessing environment values applied to later tool runs.",
                SetEnvironment, runOnMct: false);

            CommandRouter.Register("list_bookmarks", "view",
                "List the spatial bookmarks on a map.", ListBookmarks);
            CommandRouter.Register("create_bookmark", "view",
                "Save the current view as a named bookmark.", CreateBookmark);
            CommandRouter.Register("apply_bookmark", "view",
                "Zoom the map view to a bookmark.", ApplyBookmark);
            CommandRouter.Register("delete_bookmark", "view",
                "Delete a bookmark from a map.", DeleteBookmark);
        }

        // --- project ---------------------------------------------------------

        /// <summary>
        /// Runs work on ArcGIS Pro's UI thread. Project.SaveAsync and friends
        /// touch UI-owned objects and throw "a different thread owns it" when
        /// called from the MCT, where the other commands run.
        /// </summary>
        private static Task OnUiThread(Func<Task> work)
        {
            var dispatcher = FrameworkApplication.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess()) return work();
            return dispatcher.Invoke(work);
        }

        internal static Task SaveEditsOnUiThread() =>
            OnUiThread(() => Project.Current.SaveEditsAsync());

        /// <summary>
        /// Saves pending edits, then the project. False when either save
        /// throws, so a restart can refuse to discard anything. The SDK's
        /// save methods report failure by exception, not return value.
        /// </summary>
        internal static async Task<bool> SaveAllOnUiThread()
        {
            try
            {
                await OnUiThread(() => Project.Current.SaveEditsAsync())
                    .ConfigureAwait(false);
                await OnUiThread(() => Project.Current.SaveAsync())
                    .ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static Task DiscardEditsOnUiThread() =>
            OnUiThread(() => Project.Current.DiscardEditsAsync());

        private static async Task<object> SaveProjectAsync(Params parameters)
        {
            var project = Project.Current
                ?? throw new InvalidOperationException("No project is open.");
            var saveAs = parameters.GetString("save_as_path");

            if (!string.IsNullOrWhiteSpace(saveAs))
            {
                // SaveAsAsync leaves the original open, matching save_as_path.
                await OnUiThread(() => project.SaveAsAsync(saveAs)).ConfigureAwait(false);
                return new Dictionary<string, object>
                {
                    ["saved_copy"] = saveAs,
                    ["original"] = project.URI,
                };
            }

            await OnUiThread(() => project.SaveAsync()).ConfigureAwait(false);
            return new Dictionary<string, object> { ["saved"] = project.URI };
        }

        private static async Task<object> CreateMapAsync(Params parameters)
        {
            var given = parameters.GetString("name");
            var alias = parameters.GetString("map_name");
            if (!string.IsNullOrWhiteSpace(given) && !string.IsNullOrWhiteSpace(alias)
                && !string.Equals(given, alias, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(
                    "name and map_name were both given and differ; pass one of them.");
            var name = !string.IsNullOrWhiteSpace(given) ? given : alias;
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(
                    "name is required (map_name is accepted as an alias).");

            var type = (parameters.GetString("map_type", "MAP") ?? "MAP").ToUpperInvariant();
            var basemapName = parameters.GetString("basemap", "none") ?? "none";

            var map = await QueuedTask.Run(() => MapFactory.Instance.CreateMap(
                name,
                type == "SCENE" ? MapType.Scene : MapType.Map,
                MapViewingMode.Map,
                ParseBasemap(basemapName))).ConfigureAwait(false);

            var applied = new Dictionary<string, object>
            {
                ["created"] = map.Name,
                ["map_type"] = map.MapType.ToString(),
                ["basemap"] = basemapName,
            };

            if (parameters.Has("epsg"))
            {
                var wkid = parameters.GetInt("epsg");
                await QueuedTask.Run(() => map.SetSpatialReference(
                    SpatialReferenceBuilder.CreateSpatialReference(wkid)))
                    .ConfigureAwait(false);
                applied["epsg"] = wkid;
            }

            if (parameters.GetBool("activate"))
            {
                await OnUiThread(() => ProApp.Panes.CreateMapPaneAsync(map))
                    .ConfigureAwait(false);
                applied["activated"] = true;
            }

            return applied;
        }

        /// <summary>Pro takes a Basemap value, not the layer name arcpy accepts.</summary>
        private static Basemap ParseBasemap(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return Basemap.None;
            switch (name.Replace(" ", string.Empty).ToUpperInvariant())
            {
                // Names follow what ArcGIS Pro shows in its basemap gallery.
                case "NONE": return Basemap.None;
                case "PROJECTDEFAULT":
                case "DEFAULT": return Basemap.ProjectDefault;
                case "IMAGERY": return Basemap.Satellite;
                case "IMAGERYHYBRID": return Basemap.Hybrid;
                case "STREETS": return Basemap.Streets;
                case "NAVIGATION": return Basemap.NavigationVector;
                case "LIGHTGRAYCANVAS": return Basemap.Gray;
                case "DARKGRAYCANVAS": return Basemap.DarkGray;
                case "TERRAIN": return Basemap.Terrain;
                case "OCEANS": return Basemap.Oceans;
                case "OPENSTREETMAP": return Basemap.OpenStreetMap;
                case "NATIONALGEOGRAPHIC": return Basemap.NationalGeographic;
                default: return Basemap.Topographic;
            }
        }

        private static MapProjectItem FindMapItem(string name)
        {
            var item = Project.Current.GetItems<MapProjectItem>().FirstOrDefault(
                i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
            if (item == null) throw new ArgumentException($"Map not found: {name}");
            return item;
        }

        private static object RemoveMap(Params parameters)
        {
            var name = parameters.Require("map_name");
            Project.Current.RemoveItem(FindMapItem(name));
            return new Dictionary<string, object> { ["removed"] = name };
        }

        // The map item is MCT-owned while CreateMapPaneAsync must run on the UI
        // thread. Blocking either thread with .Wait() deadlocks ArcGIS Pro.
        private static async Task<object> ActivateMapAsync(Params parameters)
        {
            var name = parameters.Require("map_name");
            var map = await QueuedTask.Run(() => FindMapItem(name).GetMap());
            await OnUiThread(() => ProApp.Panes.CreateMapPaneAsync(map))
                .ConfigureAwait(false);
            return new Dictionary<string, object> { ["activated"] = map.Name };
        }

        private static async Task<object> SetMapPropertiesAsync(Params parameters)
        {
            var wkid = parameters.Has("epsg") ? (int?)parameters.GetInt("epsg") : null;
            var newName = parameters.GetString("new_name");

            var changed = await QueuedTask.Run(() =>
            {
                // A Map created on the async worker thread cannot be touched
                // from the CIM thread, so resolve it inside the queued task.
                var map = MapHelpers.ResolveMap(parameters);
                var result = new Dictionary<string, object>();
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    map.SetName(newName);
                    result["name"] = map.Name;
                }
                if (wkid.HasValue)
                {
                    var reference = SpatialReferenceBuilder.CreateSpatialReference(wkid.Value);
                    if (reference == null)
                        throw new ArgumentException(
                            $"EPSG:{wkid.Value} is not a known coordinate system.");
                    map.SetSpatialReference(reference);
                    result["epsg"] = wkid.Value;

                    var unchanged = map.GetLayersAsFlattenedList().OfType<Layer>()
                        .Where(layer =>
                        {
                            var sr = layer.GetSpatialReference();
                            return sr != null && sr.Wkid != wkid.Value;
                        })
                        .Select(layer => layer.Name)
                        .ToList();
                    if (unchanged.Count > 0) result["unchanged_layers"] = unchanged;
                }
                result["map"] = map.Name;
                return result;
            }).ConfigureAwait(false);

            var reply = new Dictionary<string, object>
            {
                ["map"] = changed["map"],
                ["changed"] = changed,
            };
            if (wkid.HasValue)
                reply["map_note"] = "Only the map's coordinate system changed; "
                    + "layer data is not reprojected.";
            return reply;
        }

        private static object SetEnvironment(Params parameters)
        {
            var settings = parameters.GetObject("settings")
                ?? throw new ArgumentException("settings is required.");
            var applied = new Dictionary<string, object>();

            foreach (var pair in settings)
            {
                if (pair.Value.ValueKind == System.Text.Json.JsonValueKind.Null)
                {
                    Environment.Remove(pair.Key);
                    applied[pair.Key] = null;
                    continue;
                }
                object value = pair.Value.ValueKind switch
                {
                    JsonValueKind.String => pair.Value.GetString(),
                    JsonValueKind.Number => pair.Value.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Object => EnvironmentExtentText(pair.Value)
                                            ?? pair.Value.GetRawText(),
                    _ => pair.Value.GetRawText(),
                };
                Environment[pair.Key] = value;
                applied[pair.Key] = value;
            }

            return new Dictionary<string, object>
            {
                ["applied"] = applied,
                ["forwarded"] = applied.Keys
                    .Where(GpEnvironmentNames.ContainsKey)
                    .Select(key => GpEnvironmentNames[key])
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(name => name).ToList(),
                ["unsupported"] = applied.Keys
                    .Where(key => !GpEnvironmentNames.ContainsKey(key))
                    .OrderBy(name => name).ToList(),
                ["note"] = "Forwarded keys reach each geoprocessing run; unsupported "
                         + "keys are stored but ignored. A tool still decides whether "
                         + "it honours an environment.",
            };
        }

        private static object GetEnvironment(Params parameters)
        {
            var result = new Dictionary<string, object>(Environment);
            result["_forwarded"] = Environment.Keys
                .Where(GpEnvironmentNames.ContainsKey)
                .Select(key => GpEnvironmentNames[key])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name).ToList();
            result["_unsupported"] = Environment.Keys
                .Where(key => !GpEnvironmentNames.ContainsKey(key))
                .OrderBy(name => name).ToList();
            return result;
        }

        /// <summary>
        /// An {xmin,ymin,xmax,ymax} object becomes the space-separated extent
        /// string a geoprocessing environment expects.
        /// </summary>
        internal static string EnvironmentExtentText(JsonElement value)
        {
            if (value.TryGetProperty("xmin", out var xmin)
                && value.TryGetProperty("ymin", out var ymin)
                && value.TryGetProperty("xmax", out var xmax)
                && value.TryGetProperty("ymax", out var ymax))
            {
                return string.Join(" ", new[] { xmin, ymin, xmax, ymax }
                    .Select(v => v.GetDouble().ToString(
                        "R", System.Globalization.CultureInfo.InvariantCulture)));
            }
            return null;
        }

        // --- bookmarks -------------------------------------------------------

        private static Bookmark FindBookmark(Map map, string name)
        {
            var bookmark = map.GetBookmarks().FirstOrDefault(
                b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase));
            if (bookmark == null)
                throw new ArgumentException(
                    $"Bookmark not found on map '{map.Name}': {name}. Available: "
                    + string.Join(", ", map.GetBookmarks().Select(b => b.Name)));
            return bookmark;
        }

        private static object ListBookmarks(Params parameters)
        {
            var map = MapHelpers.ResolveMap(parameters);
            return new Dictionary<string, object>
            {
                ["map"] = map.Name,
                ["bookmarks"] = map.GetBookmarks().Select(bookmark =>
                    new Dictionary<string, object> { ["name"] = bookmark.Name }).ToList(),
            };
        }

        private static object CreateBookmark(Params parameters)
        {
            var map = MapHelpers.ResolveMap(parameters);
            var view = MapHelpers.RequireActiveView(map);
            var name = parameters.Require("name");
            var description = parameters.GetString("description");

            var bookmark = string.IsNullOrWhiteSpace(description)
                ? map.AddBookmark(view, name)
                : map.AddBookmark(view, name, description);

            return new Dictionary<string, object>
            {
                ["map"] = map.Name,
                ["created_bookmark"] = bookmark.Name,
                ["bookmarks"] = map.GetBookmarks().Select(b => b.Name).ToList(),
            };
        }

        private static object ApplyBookmark(Params parameters)
        {
            var map = MapHelpers.ResolveMap(parameters);
            var view = MapHelpers.RequireActiveView(map);
            var bookmark = FindBookmark(map, parameters.Require("bookmark_name"));
            view.ZoomTo(bookmark);
            return new Dictionary<string, object>
            {
                ["map"] = map.Name,
                ["applied_bookmark"] = bookmark.Name,
                ["scale"] = view.Camera?.Scale,
            };
        }

        private static object DeleteBookmark(Params parameters)
        {
            var map = MapHelpers.ResolveMap(parameters);
            var name = parameters.Require("bookmark_name");
            map.RemoveBookmark(FindBookmark(map, name));
            return new Dictionary<string, object>
            {
                ["map"] = map.Name,
                ["deleted_bookmark"] = name,
            };
        }
    }
}
