using System;
using System.Collections.Generic;
using System.Linq;
using io.github.ykysnk.Localization.Editor;
using io.github.ykysnk.utils.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    /// <summary>
    ///     Body of the delete confirmation: one row per asset with an object field that pings the asset and a read-only
    ///     path field. A list rather than a message, so a large selection cannot run into the label length limit.
    /// </summary>
    internal sealed class DeleteDialogContent : DialogContent
    {
        private const string StyleSheetPath =
            "Packages/io.github.ykysnk.yky-toolkit/Editor/DeleteDialogContent.uss";

        private readonly List<string> _paths;
        private readonly HashSet<string> _restricted;

        internal DeleteDialogContent(IEnumerable<string> paths, IEnumerable<string>? restricted = null)
        {
            _paths = paths.ToList();
            _restricted = new HashSet<string>(restricted ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);
        }

        public override Vector2 Size => new(660, 470);

        public override VisualElement CreateContent()
        {
            var root = new VisualElement();
            root.AddToClassList("delete-dialog");

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            if (styleSheet != null) root.styleSheets.Add(styleSheet);

            var warning = new Label((_restricted.Count > 0
                ? "label.new_delete.restricted_warning"
                : "label.new_delete.warning").S());
            warning.AddToClassList("delete-dialog__warning");
            warning.EnableInClassList("delete-dialog__warning--error", _restricted.Count > 0);
            root.Add(warning);

            var list = new ListView
            {
                itemsSource = _paths,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                selectionType = SelectionType.None,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                makeItem = CreateRow,
                bindItem = BindRow
            };
            list.AddToClassList("delete-dialog__list");
            root.Add(list);

            return root;
        }

        private static VisualElement CreateRow()
        {
            var row = new VisualElement();
            row.AddToClassList("delete-dialog__row");

            var objectField = new ObjectField
            {
                name = "object",
                objectType = typeof(Object),
                allowSceneObjects = false
            };
            objectField.AddToClassList("delete-dialog__object");

            // A normal object field opens the object picker; this dialog only needs it to reveal the asset.
            objectField.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0 || row.userData is not string path) return;

                var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (asset == null) return;

                evt.StopImmediatePropagation();
                evt.PreventDefault();
                EditorGUIUtility.PingObject(asset);
                Selection.activeObject = asset;
            }, TrickleDown.TrickleDown);

            var pathField = new TextField
            {
                name = "path",
                isReadOnly = true
            };
            pathField.AddToClassList("delete-dialog__path");

            row.Add(objectField);
            row.Add(pathField);

            return row;
        }

        private void BindRow(VisualElement element, int index)
        {
            if (index < 0 || index >= _paths.Count) return;

            var path = _paths[index];
            element.userData = path;
            element.EnableInClassList("delete-dialog__row--blocked", _restricted.Contains(path));
            element.Q<ObjectField>("object").SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<Object>(path));
            element.Q<TextField>("path").SetValueWithoutNotify(path);
        }
    }
}
