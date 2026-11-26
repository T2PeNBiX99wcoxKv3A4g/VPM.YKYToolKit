using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using io.github.ykysnk.Localization.Editor;
using io.github.ykysnk.utils.Editor;
using io.github.ykysnk.utils.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal class ImportWatcherWindow : EditorWindow
    {
        private const string Title = "Import Watcher";
        private const string DateFormat = "yyyy-MM-dd";
        private const string TimeFormat = "HH:mm:ss";

        private static readonly List<string> RootFolders = new()
        {
            "Packages",
            "ProjectSettings"
        };

        private static readonly Color SessionTimeColor = new(0.47f, 0.65f, 1f);

        [SerializeField] private VisualTreeAsset? uxml;

        private readonly List<DateGroup> _dates = new();

        private ListView? _list;
        private TreeView? _tree;
        private Label? _detailTitle;
        private Label? _emptyLabel;

        private void CreateGUI()
        {
            var tree = uxml!.CloneTree();
            tree.UILocalize(false);
            tree.style.flexGrow = 1;
            rootVisualElement.Add(tree);

            _list = tree.Q<ListView>("importLogList");
            _tree = tree.Q<TreeView>("importLogTree");
            _detailTitle = tree.Q<Label>("importLogDetailTitle");
            _emptyLabel = tree.Q<Label>("importLogEmpty");

            _list.makeItem = CreateDateRow;
            _list.bindItem = BindDateRow;
            _list.selectionChanged += _ => ShowSelectedDate();
            _list.LocalizeEmptyLabel();

            _tree.makeItem = CreateRecordRow;
            _tree.bindItem = BindRecordRow;
            _tree.SetRootItems(new List<TreeViewItemData<Node>>());

            tree.Q<ToolbarButton>("refresh").clicked += RefreshImportLog;
            tree.Q<ToolbarButton>("clear").clicked += () => UniTask.Create(async () =>
            {
                if (await EditorUtils.DisplayDialogAsync("label.import_watcher.clear_import_log_title".S(),
                        "label.import_watcher.clear_import_log_message".S(),
                        "label.clear".S(),
                        "label.cancel".S()))
                {
                    ImportHistoryManager.Clear();
                    RefreshImportLog();
                }
            });
            tree.Q<ToolbarButton>("settings").clicked += ImportWatcherSettingsWindow.ShowWindow;

            RefreshImportLog();
        }

        private static VisualElement CreateDateRow()
        {
            var row = new VisualElement();
            row.AddToClassList("import-log-date");

            var date = new Label();
            date.AddToClassList("import-log-date__label");
            row.Add(date);

            return row;
        }

        private void BindDateRow(VisualElement element, int index)
        {
            if (index < 0 || index >= _dates.Count) return;

            var group = _dates[index];
            element.userData = group;
            element.Q<Label>(className: "import-log-date__label").text = group.Date.ToString(DateFormat);
        }

        private VisualElement CreateRecordRow()
        {
            var row = new VisualElement();
            row.AddToClassList("import-log-item");

            var icon = new Image { name = "icon" };
            icon.AddToClassList("import-log-item__icon");

            var name = new Label { name = "name" };
            name.AddToClassList("import-log-item__name");

            row.Add(icon);
            row.Add(name);

            // Registered once here: bindItem runs for every reuse, so registering it there added a handler per scroll.
            row.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.clickCount != 2 || row.userData is not Node { IsSession: false } node) return;
                e.StopPropagation();
                var asset = AssetDatabase.LoadAssetAtPath<Object>(node.FullPath);
                if (asset == null) return;
                EditorGUIUtility.PingObject(asset);
                Selection.activeObject = asset;
            });

            return row;
        }

        private void BindRecordRow(VisualElement element, int index)
        {
            if (_tree == null) return;

            var id = _tree.GetIdForIndex(index);
            var node = _tree.GetItemDataForId<Node>(id);
            if (node == null) return;

            element.userData = node;

            var icon = element.Q<Image>("icon");
            var label = element.Q<Label>("name");

            if (node.IsSession)
            {
                icon.style.display = DisplayStyle.None;
                label.text = node.Name;
                label.tooltip = null;
                label.style.color = SessionTimeColor;
                label.style.unityFontStyleAndWeight = FontStyle.Bold;
                return;
            }

            icon.style.display = DisplayStyle.Flex;
            label.text = node.Name;
            label.tooltip = node.FullPath;
            label.style.unityFontStyleAndWeight = FontStyle.Normal;

            var type = AssetDatabase.GetMainAssetTypeAtPath(node.FullPath);
            if (type != null || AssetDatabase.IsValidFolder(node.FullPath))
            {
                icon.image = AssetDatabase.GetCachedIcon(node.FullPath);
                label.style.color = StyleKeyword.Null;
            }
            else if (RootFolders.Contains(node.FullPath))
            {
                icon.image = EditorGUIUtility.FindTexture("Folder Icon");
                label.style.color = StyleKeyword.Null;
            }
            else
            {
                icon.image = InternalEditorUtility.GetIconForFile(node.FullPath);
                label.style.color = Color.red;
            }
        }

        private void RefreshImportLog()
        {
            if (_list == null) return;

            _dates.Clear();
            var sessions = ImportHistoryManager.All();
            sessions.Reverse();

            var map = new Dictionary<DateTime, DateGroup>();
            foreach (var session in sessions)
            {
                var date = DateTimeOffset.FromUnixTimeSeconds(session.unixSeconds).LocalDateTime.Date;
                if (!map.TryGetValue(date, out var group))
                {
                    group = new DateGroup(date);
                    map[date] = group;
                    _dates.Add(group);
                }

                group.Sessions.Add(session);
            }

            _list.itemsSource = _dates;
            _list.Rebuild();

            var hasDates = _dates.Count > 0;
            if (_emptyLabel != null)
                _emptyLabel.style.display = hasDates ? DisplayStyle.None : DisplayStyle.Flex;
            if (_tree != null) _tree.style.display = hasDates ? DisplayStyle.Flex : DisplayStyle.None;
            if (_detailTitle != null)
                _detailTitle.style.display = hasDates ? DisplayStyle.Flex : DisplayStyle.None;

            if (!hasDates)
            {
                _tree?.SetRootItems(new List<TreeViewItemData<Node>>());
                if (_detailTitle != null) _detailTitle.text = string.Empty;
                return;
            }

            _list.SetSelection(0);
            ShowSelectedDate();
        }

        private void ShowSelectedDate()
        {
            if (_list == null || _tree == null) return;

            var index = _list.selectedIndex;
            if (index < 0 || index >= _dates.Count)
            {
                _tree.SetRootItems(new List<TreeViewItemData<Node>>());
                if (_detailTitle != null) _detailTitle.text = string.Empty;
                return;
            }

            var group = _dates[index];
            var recordCount = group.Sessions.Sum(session => session.records.Count);

            if (_detailTitle != null)
                _detailTitle.text = $"{group.Date:yyyy-MM-dd}  ({recordCount})";

            var nextId = 0;
            var items = new List<TreeViewItemData<Node>>();
            foreach (var session in group.Sessions)
                items.Add(CreateSessionItem(session, ref nextId));

            _tree.SetRootItems(items);
        }

        private static TreeViewItemData<Node> CreateSessionItem(ImportSession session, ref int nextId)
        {
            var time = DateTimeOffset.FromUnixTimeSeconds(session.unixSeconds).LocalDateTime;
            var children = BuildTree(session.records, ref nextId).Select(CreateTreeViewItem).ToList();
            var node = new Node($"{time:HH:mm:ss}  ({session.records.Count})", string.Empty, nextId++, true);

            return new(node.Id, node, children);
        }

        private static List<Node> BuildTree(List<ImportRecord> records, ref int nextId)
        {
            var rootNodes = new List<Node>();
            var map = new Dictionary<string, Node>();

            foreach (var record in records)
            {
                var path = record.path;
                if (string.IsNullOrEmpty(path)) continue;

                var parts = path.Split('/');
                var current = "";
                Node? parent = null;

                for (var depth = 0; depth < parts.Length; depth++)
                {
                    var part = parts[depth];
                    if (string.IsNullOrEmpty(part)) continue;

                    current = depth == 0 ? part : $"{current}/{part}";

                    if (!map.TryGetValue(current, out var node))
                    {
                        // A counter rather than the path hash: the TreeView needs ids that never collide.
                        node = new(part, current, nextId++);
                        map[current] = node;

                        if (parent == null)
                            rootNodes.Add(node);
                        else
                            parent.Children.Add(node);
                    }

                    parent = node;
                }
            }

            SortTree(rootNodes);
            return rootNodes;
        }

        private static TreeViewItemData<Node> CreateTreeViewItem(Node node)
        {
            var children = node.Children.Select(CreateTreeViewItem).ToList();
            return new(node.Id, node, children);
        }

        private static void SortTree(List<Node> nodes)
        {
            // Folders first, then files, both alphabetically, as the project browser shows them.
            nodes.Sort((a, b) =>
            {
                var aIsFolder = a.Children.Count > 0;
                var bIsFolder = b.Children.Count > 0;
                if (aIsFolder != bIsFolder) return aIsFolder ? -1 : 1;
                return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });

            foreach (var node in nodes)
                SortTree(node.Children);
        }

        [MenuItem("Tools/YKYToolkit/Import Watcher", false, Util.Three)]
        private static void ShowWindow()
        {
            var window = GetWindow<ImportWatcherWindow>();
            window.titleContent = ToolkitIcons.Content(Title, "import-watcher");
            window.minSize = new Vector2(560, 360);
        }

        private sealed class DateGroup
        {
            internal readonly DateTime Date;
            internal readonly List<ImportSession> Sessions = new();

            internal DateGroup(DateTime date) => Date = date;
        }

        private class Node
        {
            public readonly List<Node> Children = new();
            public readonly string FullPath;
            public readonly int Id;
            public readonly string Name;

            public Node(string name, string fullPath, int id, bool isSession = false)
            {
                Name = name;
                FullPath = fullPath;
                Id = id;
                IsSession = isSession;
            }

            public bool IsSession { get; }
        }
    }
}
