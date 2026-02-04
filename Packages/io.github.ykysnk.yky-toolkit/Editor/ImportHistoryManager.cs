using System;
using System.Collections.Generic;
using System.Linq;
using io.github.ykysnk.utils.NonUdon;
using JetBrains.Annotations;
using UnityEditor;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [Serializable]
    [PublicAPI]
    internal class ImportRecord
    {
        public string path = string.Empty;
        public string guid = string.Empty;
        public string name = string.Empty;
        public bool isFolder;
        public string? iconGuid;
    }

    [Serializable]
    [PublicAPI]
    internal class ImportSession
    {
        public long unixSeconds;
        public List<ImportRecord> records = new();
    }

    internal static class ImportHistoryManager
    {
        private const string EditorKey = "YKYToolkit/ImportWatcher/ImportHistoryV2";
        private const string KnownGuidsKey = "YKYToolkit/ImportWatcher/KnownGuids";
        private const int MaxSessions = 100;

        private static HashSet<string>? _knownGuids;

        public static void AddSession(ImportSession session)
        {
            var list = LoadInternal();
            list.Add(session);
            Trim(list);
            SaveInternal(list);
        }

        public static List<ImportSession> AllSessions() => LoadInternal();

        public static void Clear()
        {
            SaveInternal(new());
        }

        public static bool IsNewGuid(string guid)
        {
            EnsureKnownGuidsInitialized();
            if (string.IsNullOrEmpty(guid)) return false;
            if (_knownGuids!.Contains(guid)) return false;
            _knownGuids.Add(guid);
            SaveKnownGuids();
            return true;
        }

        private static void EnsureKnownGuidsInitialized()
        {
            if (_knownGuids != null) return;

            _knownGuids = new();
            var json = EditorPrefs.GetString(KnownGuidsKey);
            if (JsonUtils.TryFromJson<ListWrapper<string>>(json, out var list, out _))
                foreach (var g in list!.items)
                    _knownGuids.Add(g);

            if (_knownGuids.Count == 0)
            {
                var all = AssetDatabase.FindAssets("");
                foreach (var g in all)
                    _knownGuids.Add(g);
                SaveKnownGuids();
            }
        }

        private static void SaveKnownGuids()
        {
            if (_knownGuids == null) return;
            if (!JsonUtils.TryToJson(Wrapper.Create(_knownGuids.ToList()), out var json, out _)) return;
            EditorPrefs.SetString(KnownGuidsKey, json);
        }

        private static List<ImportSession> LoadInternal()
        {
            var json = EditorPrefs.GetString(EditorKey);
            if (!JsonUtils.TryFromJson<ListWrapper<ImportSession>>(json, out var list, out _))
                return new();
            return list?.items ?? new List<ImportSession>();
        }

        private static void SaveInternal(List<ImportSession> list)
        {
            Trim(list);
            if (!JsonUtils.TryToJson(Wrapper.Create(list), out var json, out _)) return;
            EditorPrefs.SetString(EditorKey, json);
        }

        private static void Trim(List<ImportSession> list)
        {
            if (list.Count <= MaxSessions) return;
            var skip = list.Count - MaxSessions;
            var trimmed = list.Skip(skip).ToList();
            list.Clear();
            list.AddRange(trimmed);
        }
    }
}