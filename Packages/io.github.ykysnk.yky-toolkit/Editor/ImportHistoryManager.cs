using System;
using System.Collections.Generic;
using System.Linq;
using io.github.ykysnk.utils.NonUdon;
using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [Serializable]
    [PublicAPI]
    internal class ImportRecord
    {
        public string guid;

        internal ImportRecord(string guid) => this.guid = guid;
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
        private const string EditorKey = "YKYToolkit/ImportWatcher/ImportHistory";
        private const int MaxSessions = 100;

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
            var json = PlayerPrefs.GetString(EditorKey);
            if (!JsonUtils.TryFromJson<ListWrapper<ImportSession>>(json, out var list, out _))
                return new();
            return list?.items ?? new List<ImportSession>();
        }

        private static void SaveInternal(List<ImportSession> list)
        {
            Trim(list);
            if (!JsonUtils.TryToJson(Wrapper.Create(list), out var json, out _)) return;
            PlayerPrefs.SetString(EditorKey, json);
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