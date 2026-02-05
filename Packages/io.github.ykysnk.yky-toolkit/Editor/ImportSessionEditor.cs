using System;
using System.Collections.Generic;
using System.Linq;
using io.github.ykysnk.utils.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [CustomPropertyDrawer(typeof(ImportSession))]
    public class ImportSessionEditor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                AssetDatabase.GUIDToAssetPath("1456c6e22b467c241b975a27b5777f89"));
            if (uxml == null) return BasicEditor.CreateUxmlImportErrorUI();

            var segmentUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                AssetDatabase.GUIDToAssetPath("cda28001015e4dd2963c16ddc38b7b1b"));

            var tree = uxml.CloneTree();
            InternalLocalizationExtensions.Helper.UILocalize(tree, false);
            tree.Bind(property.serializedObject);

            var unixSecondsProperty = property.FindPropertyRelative("unixSeconds");
            var time = DateTimeOffset.FromUnixTimeSeconds(unixSecondsProperty.longValue).LocalDateTime
                .ToString("yyyy-MM-dd HH:mm:ss");

            var recordsProperty = property.FindPropertyRelative("records");
            var foldout = tree.Q<Foldout>("foldout");
            foldout.text = $"{time} ({recordsProperty.arraySize})";
            foldout.value = true;
            foldout.style.unityFontStyleAndWeight = FontStyle.Bold;

            var rootNodes = new List<Node>();
            var allNodes = new Dictionary<string, Node>();
            var nextId = 0;

            for (var i = 0; i < recordsProperty.arraySize; i++)
            {
                var recordProperty = recordsProperty.GetArrayElementAtIndex(i);
                var path = recordProperty.FindPropertyRelative("path").stringValue;
                if (string.IsNullOrEmpty(path)) continue;

                var parts = path.Split('/');
                var currentPath = "";
                Node? parent = null;

                for (var j = 0; j < parts.Length; j++)
                {
                    var part = parts[j];
                    if (string.IsNullOrEmpty(part)) continue;

                    if (j > 0) currentPath += "/";
                    currentPath += part;

                    if (!allNodes.TryGetValue(currentPath, out var node))
                    {
                        node = new(part, currentPath, nextId++);
                        allNodes.Add(currentPath, node);

                        if (parent == null)
                            rootNodes.Add(node);
                        else
                            parent.Children.Add(node);
                    }

                    parent = node;
                }
            }

            var nodesTreeView = tree.Q<TreeView>("nodes");
            nodesTreeView.makeItem =
                () => segmentUxml.CloneTree().Q<VisualElement>(className: "import-log-item__segment");

            nodesTreeView.bindItem = (element, id) =>
            {
                var node = nodesTreeView.GetItemDataForId<Node>(id);
                if (node == null) return;

                var icon = element.Q<Image>("icon");
                var label = element.Q<Label>("name");

                label.text = node.Name;
                label.tooltip = node.Path;

                var asset = AssetDatabase.LoadAssetAtPath<Object>(node.Path);
                if (asset != null)
                {
                    icon.image = AssetDatabase.GetCachedIcon(node.Path);
                    label.style.color = StyleKeyword.Null;
                }
                else
                {
                    label.style.color = Color.red;
                    icon.image = InternalEditorUtility.GetIconForFile(node.Path);
                }

                element.UnregisterCallback<MouseDownEvent>(OnMouseDown);
                element.RegisterCallback<MouseDownEvent>(OnMouseDown);
                return;

                void OnMouseDown(MouseDownEvent e)
                {
                    if (e.clickCount != 2) return;
                    e.StopPropagation();
                    var a = AssetDatabase.LoadAssetAtPath<Object>(node.Path);
                    if (a == null) return;
                    EditorGUIUtility.PingObject(a);
                    Selection.activeObject = a;
                }
            };

            nodesTreeView.SetRootItems(rootNodes.Select(CreateTreeViewItem).ToList());

            return tree;

            TreeViewItemData<Node> CreateTreeViewItem(Node node)
            {
                var children = node.Children.Select(CreateTreeViewItem).ToList();
                return new(node.Id, node, children);
            }
        }

        private class Node
        {
            public readonly List<Node> Children = new();
            public readonly int Id;
            public readonly string Name;
            public readonly string Path;

            public Node(string name, string path, int id)
            {
                Name = name;
                Path = path;
                Id = id;
            }
        }
    }
}