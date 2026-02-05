using System;
using System.Collections.Generic;
using System.Linq;
using io.github.ykysnk.utils.NonUdon;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [Serializable]
    [PublicAPI]
    internal struct DeleteRecord
    {
        public string path;
        public string guid;
        public string extension;
        public long unixSeconds;
    }

    internal static class DeleteHistoryManager
    {
        private const string DeleteHistoryEditorKey = "YKYToolkit/DeleteHistory";
        private const string MaxRecordsEditorKey = "YKYToolkit/DeleteHistory/MaxRecords";
        private const int DefaultMaxRecords = 200;

        internal static int MaxRecords
        {
            get => EditorPrefs.GetInt(MaxRecordsEditorKey, DefaultMaxRecords);
            set => EditorPrefs.SetInt(MaxRecordsEditorKey, value);
        }

        public static void Add(DeleteRecord record)
        {
            var list = LoadInternal();
            list.Add(record);
            Trim(list);
            SaveInternal(list);
        }

        public static List<DeleteRecord> All() => LoadInternal();

        public static void Clear()
        {
            SaveInternal(new());
        }

        private static List<DeleteRecord> LoadInternal()
        {
            var json = PlayerPrefs.GetString(DeleteHistoryEditorKey);
            return !JsonUtils.TryFromJson<ListWrapper<DeleteRecord>>(json, out var list, out _) ? new() : list!.items;
        }

        private static void SaveInternal(List<DeleteRecord> list)
        {
            Trim(list);
            if (!JsonUtils.TryToJson(Wrapper.Create(list), out var json, out _)) return;
            PlayerPrefs.SetString(DeleteHistoryEditorKey, json);
        }

        private static void Trim(List<DeleteRecord> list)
        {
            if (list.Count <= MaxRecords) return;
            var skip = list.Count - MaxRecords;
            var trimmed = list.Skip(skip).ToList();
            list.Clear();
            list.AddRange(trimmed);
        }
    }
}