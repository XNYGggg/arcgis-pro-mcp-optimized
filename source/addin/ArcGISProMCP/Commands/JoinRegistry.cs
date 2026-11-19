using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ArcGISProMCP.Commands
{
    /// <summary>
    /// Joins created through this bridge, remembered so get_features can read
    /// joined fields (J1/J2, live test 2026-11-18). A Pro join exposes its
    /// fields to a cursor only under the qualified name "Table.Field"; the
    /// layer's own field list does not include them.
    /// </summary>
    internal static class JoinRegistry
    {
        internal sealed class JoinSnapshot
        {
            public string Table;
            public string LayerField;
            public string JoinField;
            public string BasePath;
            public List<string> Fields = new List<string>();
        }

        private sealed class JoinEntry
        {
            public string Table;
            public string LayerField;
            public string JoinField;
            public string BasePath;
            public List<string> Fields = new List<string>();
        }

        private static readonly object Gate = new object();
        private static readonly Dictionary<string, List<JoinEntry>> ByLayer =
            new Dictionary<string, List<JoinEntry>>(StringComparer.OrdinalIgnoreCase);

        public static void Add(string layer, string table, string layerField,
                               string joinField, string basePath = null,
                               List<string> fields = null)
        {
            if (string.IsNullOrWhiteSpace(layer) || string.IsNullOrWhiteSpace(table)) return;
            fields = fields ?? ResolveFields(table) ?? new List<string>();
            lock (Gate)
            {
                if (!ByLayer.TryGetValue(layer, out var list))
                    ByLayer[layer] = list = new List<JoinEntry>();
                list.RemoveAll(e => string.Equals(e.Table, table,
                                                  StringComparison.OrdinalIgnoreCase));
                list.Add(new JoinEntry
                {
                    Table = table,
                    LayerField = layerField,
                    JoinField = joinField,
                    BasePath = basePath,
                    Fields = fields,
                });
            }
        }

        public static List<JoinSnapshot> Snapshot(string layer)
        {
            lock (Gate)
            {
                if (!ByLayer.TryGetValue(layer, out var list)) return new List<JoinSnapshot>();
                return list.Select(e => new JoinSnapshot
                {
                    Table = e.Table,
                    LayerField = e.LayerField,
                    JoinField = e.JoinField,
                    BasePath = e.BasePath,
                    Fields = e.Fields.ToList(),
                }).ToList();
            }
        }

        public static void Remove(string layer)
        {
            if (string.IsNullOrWhiteSpace(layer)) return;
            lock (Gate) ByLayer.Remove(layer);
        }

        public static bool HasJoins(string layer)
        {
            lock (Gate)
                return ByLayer.TryGetValue(layer, out var list) && list.Count > 0;
        }

        /// <summary>
        /// Turn requested field names into names a joined cursor understands.
        /// Fields the join cannot supply come back in missing, with every
        /// candidate (base and qualified joined) offered to the caller.
        /// </summary>
        public static List<string> Qualify(string layer, IEnumerable<string> requested,
                                           IEnumerable<string> baseFields,
                                           out List<string> missing,
                                           out List<string> candidates)
        {
            var baseSet = new HashSet<string>(baseFields, StringComparer.OrdinalIgnoreCase);
            var resolved = new List<string>();
            missing = new List<string>();
            candidates = new List<string>(baseSet);

            List<JoinEntry> joins;
            lock (Gate)
            {
                ByLayer.TryGetValue(layer, out joins);
                joins = joins?.ToList() ?? new List<JoinEntry>();
            }

            foreach (var join in joins)
                candidates.AddRange(join.Fields.Select(f => join.Table + "." + f));

            foreach (var name in requested)
            {
                if (baseSet.Contains(name) || name.Contains('.'))
                {
                    resolved.Add(name);
                    continue;
                }
                var hit = joins.FirstOrDefault(j =>
                    j.Fields.Any(f => string.Equals(f, name, StringComparison.OrdinalIgnoreCase)));
                if (hit != null) resolved.Add(hit.Table + "." + name);
                else missing.Add(name);
            }

            return resolved;
        }

        /// <summary>Qualified names for every joined field, for an unfiltered read.</summary>
        public static List<string> JoinedFields(string layer)
        {
            lock (Gate)
            {
                if (!ByLayer.TryGetValue(layer, out var joins)) return new List<string>();
                return joins.SelectMany(j => j.Fields.Select(f => j.Table + "." + f)).ToList();
            }
        }

        private static List<string> ResolveFields(string table)
        {
            try
            {
                var json = JsonDocument.Parse(
                    "{\"layer_name\":" + JsonSerializer.Serialize(table) + "}");
                using var source = DataSource.Resolve(new Bridge.Params(json.RootElement));
                return source.Definition.GetFields().Select(f => f.Name).ToList();
            }
            catch
            {
                return null;
            }
        }
    }
}
