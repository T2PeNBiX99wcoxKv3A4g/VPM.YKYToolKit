using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    public class EnhancedTransformDatabase : ScriptableObject
    {
        private const string Path = "Assets/EnhancedTransformDatabase.asset";
        private static EnhancedTransformDatabase? _instance;
        public List<EnhancedTransformData> entries = new();

        private Dictionary<string, EnhancedTransformData>? _cache;

        private EnhancedTransformData GetOrCreate(string id)
        {
            _cache ??= BuildCache();
            if (_cache.TryGetValue(id, out var entry)) return entry;
            entry = new(id);
            entries.Add(entry);
            _cache[id] = entry;
            EditorUtility.SetDirty(this);
            return entry;
        }

        private Dictionary<string, EnhancedTransformData> BuildCache()
        {
            // Indexer loop instead of ToDictionary: a duplicated id in the asset threw and broke the whole inspector.
            var cache = new Dictionary<string, EnhancedTransformData>(entries.Count);

            foreach (var entry in entries)
                if (entry != null)
                    cache[entry.id] = entry;

            return cache;
        }

        private static EnhancedTransformDatabase GetDataBase()
        {
            if (_instance != null) return _instance;

            _instance = AssetDatabase.LoadAssetAtPath<EnhancedTransformDatabase>(Path);
            if (_instance != null) return _instance;

            _instance = CreateInstance<EnhancedTransformDatabase>();
            AssetDatabase.CreateAsset(_instance, Path);
            AssetDatabase.SaveAssets();
            return _instance;
        }

        [PublicAPI]
        public static void Save()
        {
            EditorUtility.SetDirty(GetDataBase());
        }

        [PublicAPI]
        public static EnhancedTransformData Get(Object obj) =>
            GetDataBase().GetOrCreate(GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString());
    }
}