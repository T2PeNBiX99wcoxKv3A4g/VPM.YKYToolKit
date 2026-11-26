using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using io.github.ykysnk.utils.NonUdon;
using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [Serializable]
    internal class ImportRecord
    {
        public string guid;
        public string name;
        public string path;

        internal ImportRecord(string guid)
        {
            this.guid = guid;
            path = AssetDatabase.GUIDToAssetPath(guid);
            name = Path.GetFileName(path);
        }
    }

    [Serializable]
    internal class ImportSession
    {
        public long unixSeconds;
        public List<ImportRecord> records = new();

        internal ImportSession() => unixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    internal static class ImportHistoryManager
    {
        private const string HistoryFileName = "ImportWatcherHistory.json";
        private const string MaxSessionsEditorKey = "YKYToolkit/ImportWatcher/MaxSessions";
        private const int DefaultMaxSessions = 100;

        /// <summary>Raised after the history was written, so an open window can refresh itself.</summary>
        internal static event Action? Changed;

        internal static int MaxSessions
        {
            get => EditorPrefs.GetInt(MaxSessionsEditorKey, DefaultMaxSessions);
            set => EditorPrefs.SetInt(MaxSessionsEditorKey, value);
        }

        internal static void ResetSettings() => MaxSessions = DefaultMaxSessions;

        public static void AddSession(ImportSession session)
        {
            var list = LoadInternal();
            list.Add(session);
            Trim(list);
            SaveInternal(list);
            Changed?.Invoke();
        }

        public static List<ImportSession> All() => LoadInternal();

        public static void Clear()
        {
            SaveInternal(new());
            Changed?.Invoke();
        }

        private static List<ImportSession> LoadInternal()
        {
            var json = ProjectHistoryStore.Read(HistoryFileName);
            if (JsonUtils.TryFromJson<ListWrapper<ImportSession>>(json, out var list, out var exception))
                return list!.items;

            // A file that exists but cannot be parsed would otherwise look like an empty history.
            if (!string.IsNullOrEmpty(json))
                Debug.LogWarning($"[YKYToolkit] Could not read {HistoryFileName}: {exception?.Message}");

            return new();
        }

        private static void SaveInternal(List<ImportSession> list)
        {
            Trim(list);
            if (!JsonUtils.TryToJson(Wrapper.Create(list), out var json, out _)) return;
            ProjectHistoryStore.Write(HistoryFileName, json);
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