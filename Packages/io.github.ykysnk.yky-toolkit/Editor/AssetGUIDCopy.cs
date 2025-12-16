using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace io.github.ykysnk.ykyToolkit.Editor;

internal static class AssetGuidCopy
{
    private const string MenuPath = $"Assets/{Util.Name}/Asset GUID Copy";

    [MenuItem(MenuPath, false)]
    private static void GuidCopy()
    {
        if (Selection.assetGUIDs.Length < 1) return;
        IEnumerable<string> guidList;

        if (Selection.assetGUIDs.Length < 2)
            guidList = Selection.assetGUIDs;
        else
            guidList = Selection.assetGUIDs.ToDictionary(x => Path.GetFileName(AssetDatabase.GUIDToAssetPath(x)), x => x)
                .Select(x => $"{x.Key} - {x.Value}");

        var allGuid = string.Join("\n", guidList);
        EditorGUIUtility.systemCopyBuffer = allGuid;
    }
}