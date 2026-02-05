using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using io.github.ykysnk.utils.NonUdon;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [Serializable]
    [PublicAPI]
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
    [PublicAPI]
    internal class ImportSession
    {
        public long unixSeconds;
        public List<ImportRecord> records = new();

        internal ImportSession() => unixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    internal static class ImportHistoryManager
    {
        private const string ImportHistoryEditorKey = "YKYToolkit/ImportWatcher/ImportHistory";
        private const string MaxSessionsEditorKey = "YKYToolkit/ImportWatcher/MaxSessions";
        private const int DefaultMaxSessions = 100;

        internal static int MaxSessions
        {
            get => EditorPrefs.GetInt(MaxSessionsEditorKey, DefaultMaxSessions);
            set => EditorPrefs.SetInt(MaxSessionsEditorKey, value);
        }

        public static void AddSession(ImportSession session)
        {
            var list = LoadInternal();
            list.Add(session);
            Trim(list);
            SaveInternal(list);
        }

        public static List<ImportSession> All() => LoadInternal();

        public static void Clear()
        {
            SaveInternal(new());
        }

        private static List<ImportSession> LoadInternal()
        {
            var json = PlayerPrefs.GetString(ImportHistoryEditorKey);
            return !JsonUtils.TryFromJson<ListWrapper<ImportSession>>(json, out var list, out _) ? new() : list!.items;
        }

        private static void SaveInternal(List<ImportSession> list)
        {
            Trim(list);
            if (!JsonUtils.TryToJson(Wrapper.Create(list), out var json, out _)) return;
            PlayerPrefs.SetString(ImportHistoryEditorKey, json);
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