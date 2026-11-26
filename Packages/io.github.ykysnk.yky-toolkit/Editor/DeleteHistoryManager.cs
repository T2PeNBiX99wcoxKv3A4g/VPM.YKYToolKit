using System;
using System.Collections.Generic;
using System.Linq;
using io.github.ykysnk.utils.NonUdon;
using UnityEditor;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [Serializable]
    internal struct DeleteRecord
    {
        public string path;
        public string guid;
        public string extension;
        public long unixSeconds;
    }

    internal static class DeleteHistoryManager
    {
        private const string HistoryFileName = "DeleteHistory.json";
        private const string MaxRecordsEditorKey = "YKYToolkit/DeleteHistory/MaxRecords";
        private const int DefaultMaxRecords = 200;

        internal static int MaxRecords
        {
            get => EditorPrefs.GetInt(MaxRecordsEditorKey, DefaultMaxRecords);
            set => EditorPrefs.SetInt(MaxRecordsEditorKey, value);
        }

        internal static void ResetSettings() => MaxRecords = DefaultMaxRecords;

        public static void Add(DeleteRecord record)
        {
            AddRange(new[] { record });
        }

        /// <summary>
        ///     Adds several records with one load and one save. Deleting in bulk used to rewrite the whole history for
        ///     every file, which made large deletes quadratic.
        /// </summary>
        public static void AddRange(IEnumerable<DeleteRecord> records)
        {
            var list = LoadInternal();
            list.AddRange(records);
            SaveInternal(list);
        }

        public static List<DeleteRecord> All() => LoadInternal();

        public static void Clear()
        {
            SaveInternal(new());
        }

        private static List<DeleteRecord> LoadInternal()
        {
            var json = ProjectHistoryStore.Read(HistoryFileName);
            return !JsonUtils.TryFromJson<ListWrapper<DeleteRecord>>(json, out var list, out _) ? new() : list!.items;
        }

        private static void SaveInternal(List<DeleteRecord> list)
        {
            Trim(list);
            if (!JsonUtils.TryToJson(Wrapper.Create(list), out var json, out _)) return;
            ProjectHistoryStore.Write(HistoryFileName, json);
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