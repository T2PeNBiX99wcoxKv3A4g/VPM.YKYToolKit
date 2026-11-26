using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    /// <summary>
    ///     Legacy storage for the extra transform settings. New values live in
    ///     <see cref="EnhancedTransformSettings" /> components on the GameObject; this asset is only read to move old
    ///     entries over the first time an object is changed.
    /// </summary>
    public class EnhancedTransformDatabase : ScriptableObject
    {
        private const string Path = "Assets/EnhancedTransformDatabase.asset";

        public List<EnhancedTransformData> entries = new();

        internal static EnhancedTransformData? Find(string id)
        {
            var database = AssetDatabase.LoadAssetAtPath<EnhancedTransformDatabase>(Path);
            return database?.entries.Find(entry => entry != null && entry.id == id);
        }

        internal static void Remove(string id)
        {
            var database = AssetDatabase.LoadAssetAtPath<EnhancedTransformDatabase>(Path);
            if (database == null) return;
            if (database.entries.RemoveAll(entry => entry != null && entry.id == id) == 0) return;

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
        }
    }
}
