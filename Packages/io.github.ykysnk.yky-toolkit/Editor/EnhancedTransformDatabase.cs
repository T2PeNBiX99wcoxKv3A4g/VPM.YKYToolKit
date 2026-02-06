using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    public class EnhancedTransformDatabase : ScriptableObject
    {
        private const string Path = "Assets/EnhancedTransformDatabase.asset";
        public List<EnhancedTransformData> entries = new();

        private Dictionary<string, EnhancedTransformData>? _cache;

        private EnhancedTransformData GetOrCreate(string id)
        {
            _cache ??= entries.ToDictionary(e => e.id);
            if (_cache.TryGetValue(id, out var entry)) return entry;
            entry = new(id);
            entries.Add(entry);
            _cache[id] = entry;
            EditorUtility.SetDirty(this);
            return entry;
        }

        private static EnhancedTransformDatabase GetDataBase()
        {
            var db = AssetDatabase.LoadAssetAtPath<EnhancedTransformDatabase>(Path);
            if (db != null) return db;
            db = CreateInstance<EnhancedTransformDatabase>();
            AssetDatabase.CreateAsset(db, Path);
            AssetDatabase.SaveAssets();
            return db;
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