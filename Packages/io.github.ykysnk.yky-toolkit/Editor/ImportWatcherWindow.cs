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

        [SerializeField] private VisualTreeAsset? uxml;

        private readonly List<ImportSession> _sessions = new();
        private readonly List<DateGroup> _dates = new();
        private readonly List<TimeGroup> _times = new();

        private ListView? _dateList;
        private ListView? _timeList;
        private VisualElement? _dateColumn;
        private VisualElement? _timeColumn;
        private TreeView? _tree;
        private Label? _detailTitle;
        private Label? _emptyLabel;

        // Ids keep increasing across selections, so the tree never mistakes an old row for a new one.
        private int _nodeIdSeed;

        private string _filter = string.Empty;

        private void CreateGUI()
        {
            var tree = uxml!.CloneTree();
            tree.UILocalize(false);
            tree.style.flexGrow = 1;
            rootVisualElement.Add(tree);

            _dateList = tree.Q<ListView>("importLogList");
            _timeList = tree.Q<ListView>("importLogTimes");
            _dateColumn = tree.Q<VisualElement>("dateColumn");
            _timeColumn = tree.Q<VisualElement>("timeColumn");
            _tree = tree.Q<TreeView>("importLogTree");
            _detailTitle = tree.Q<Label>("importLogDetailTitle");
            _emptyLabel = tree.Q<Label>("importLogEmpty");

            _dateList.makeItem = CreateDateRow;
            _dateList.bindItem = BindDateRow;
            _dateList.selectionChanged += _ => ShowSelectedDate();
            _dateList.LocalizeEmptyLabel();

            _timeList.makeItem = CreateTimeRow;
            _timeList.bindItem = BindTimeRow;
            _timeList.selectionChanged += _ => ShowSelectedSession();

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

            var search = tree.Q<ToolbarSearchField>("search");
            search.SetupSearchField();
            search.RegisterValueChangedCallback(evt =>
            {
                _filter = evt.newValue ?? string.Empty;
                RebuildLog();
            });

            RefreshImportLog();
        }

        private static VisualElement CreateDateRow()
        {
            var row = new VisualElement();
            row.AddToClassList("import-log-date");

            var label = new Label();
            label.AddToClassList("import-log-date__label");
            row.Add(label);

            return row;
        }

        private void BindDateRow(VisualElement element, int index)
        {
            var label = element.Q<Label>(className: "import-log-date__label");
            if (index < 0 || index >= _dates.Count)
            {
                element.userData = null;
                label.text = string.Empty;
                return;
            }

            var group = _dates[index];
            element.userData = group;
            label.text = group.Date.ToString(DateFormat);
        }

        private static VisualElement CreateTimeRow()
        {
            var row = new VisualElement();
            row.AddToClassList("import-log-time");

            var label = new Label();
            label.AddToClassList("import-log-time__label");
            row.Add(label);

            return row;
        }

        private void BindTimeRow(VisualElement element, int index)
        {
            var label = element.Q<Label>(className: "import-log-time__label");
            if (index < 0 || index >= _times.Count)
            {
                element.userData = null;
                label.text = string.Empty;
                return;
            }

            var group = _times[index];
            element.userData = group;
            label.text = group.LocalTime.ToString(TimeFormat);
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
                if (e.clickCount != 2 || row.userData is not Node node) return;
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

            var icon = element.Q<Image>("icon");
            var label = element.Q<Label>("name");

            var id = _tree.GetIdForIndex(index);
            var node = _tree.GetItemDataForId<Node>(id);
            if (node == null)
            {
                // A recycled row can be bound before its item resolves; clear it instead of leaving a blank line.
                element.userData = null;
                icon.image = null;
                label.text = string.Empty;
                label.tooltip = null;
                return;
            }

            element.userData = node;

            label.text = node.Name;
            label.tooltip = node.FullPath;

            // A node with children is a folder, so it gets a folder icon even when the folder no longer exists.
            if (node.Children.Count > 0)
            {
                icon.image = AssetDatabase.GetCachedIcon(node.FullPath) ??
                             EditorGUIUtility.FindTexture("Folder Icon");
                label.style.color = StyleKeyword.Null;
                return;
            }

            if (AssetDatabase.GetMainAssetTypeAtPath(node.FullPath) != null)
            {
                icon.image = AssetDatabase.GetCachedIcon(node.FullPath) ??
                             InternalEditorUtility.GetIconForFile(node.FullPath);
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
            _sessions.Clear();
            _sessions.AddRange(ImportHistoryManager.All());
            _sessions.Reverse();

            RebuildLog();
        }

        private void RebuildLog()
        {
            var selectedDate = _dateList != null && _dateList.selectedIndex >= 0 &&
                               _dateList.selectedIndex < _dates.Count
                ? _dates[_dateList.selectedIndex].Date
                : (DateTime?)null;

            _dates.Clear();
            var map = new Dictionary<DateTime, DateGroup>();

            foreach (var session in _sessions)
            {
                var records = session.records.Where(MatchesFilter).ToList();
                if (records.Count == 0) continue;

                var date = LocalTimeOf(session.unixSeconds).Date;
                if (!map.TryGetValue(date, out var dateGroup))
                {
                    dateGroup = new DateGroup(date);
                    map[date] = dateGroup;
                    _dates.Add(dateGroup);
                }

                // A single import can arrive as several sessions in the same second, so a time groups them.
                var timeGroup = dateGroup.Times.FirstOrDefault(group => group.UnixSeconds == session.unixSeconds);
                if (timeGroup == null)
                {
                    timeGroup = new TimeGroup(session.unixSeconds);
                    dateGroup.Times.Add(timeGroup);
                }

                timeGroup.Records.AddRange(records);
            }

            if (_dateList == null) return;

            _dateList.itemsSource = _dates;
            _dateList.Rebuild();

            var hasDates = _dates.Count > 0;
            if (_emptyLabel != null)
                _emptyLabel.style.display = hasDates ? DisplayStyle.None : DisplayStyle.Flex;
            if (_dateColumn != null)
                _dateColumn.style.display = hasDates ? DisplayStyle.Flex : DisplayStyle.None;
            if (_timeColumn != null)
                _timeColumn.style.display = hasDates ? DisplayStyle.Flex : DisplayStyle.None;
            if (_tree != null) _tree.style.display = hasDates ? DisplayStyle.Flex : DisplayStyle.None;
            if (_detailTitle != null)
                _detailTitle.style.display = hasDates ? DisplayStyle.Flex : DisplayStyle.None;

            if (!hasDates)
            {
                ClearTimes();
                ClearDetail();
                return;
            }

            var index = selectedDate.HasValue ? _dates.FindIndex(group => group.Date == selectedDate.Value) : -1;
            _dateList.SetSelection(index >= 0 ? index : 0);
            ShowSelectedDate();
        }

        private void ClearTimes()
        {
            _times.Clear();
            if (_timeList == null) return;

            _timeList.itemsSource = _times;
            _timeList.Rebuild();
        }

        private bool MatchesFilter(ImportRecord record)
        {
            if (string.IsNullOrEmpty(_filter)) return true;

            return record.path?.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   record.guid?.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void ShowSelectedDate()
        {
            if (_timeList == null) return;

            _times.Clear();

            var group = _dates.ElementAtOrDefault(_dateList?.selectedIndex ?? -1);
            if (group != null) _times.AddRange(group.Times);

            _timeList.itemsSource = _times;
            _timeList.Rebuild();

            if (_times.Count == 0)
            {
                ClearDetail();
                return;
            }

            _timeList.SetSelection(0);
            ShowSelectedSession();
        }

        private void ShowSelectedSession()
        {
            if (_tree == null) return;

            var group = _times.ElementAtOrDefault(_timeList?.selectedIndex ?? -1);
            if (group == null)
            {
                ClearDetail();
                return;
            }

            if (_detailTitle != null)
                _detailTitle.text = $"{group.LocalTime:HH:mm:ss}  ({group.Records.Count})";

            _tree.ClearSelection();
            _tree.SetRootItems(BuildTree(group.Records, ref _nodeIdSeed).Select(CreateTreeViewItem).ToList());
            _tree.schedule.Execute(() =>
            {
                // Expansion state does not follow new items, so the files stay hidden until every item is expanded.
                _tree.ExpandAll();
                _tree.ScrollToItem(0);
            }).ExecuteLater(0);
        }

        private void ClearDetail()
        {
            _tree?.SetRootItems(new List<TreeViewItemData<Node>>());
            if (_detailTitle != null) _detailTitle.text = string.Empty;
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
            return rootNodes.Select(Compact).ToList();
        }

        /// <summary>
        ///     Collapses a chain of folders that each hold a single folder into one row, so a deep import path reads as
        ///     one line and the file that changed is visible without scrolling.
        /// </summary>
        private static Node Compact(Node node)
        {
            var name = node.Name;
            var fullPath = node.FullPath;
            var current = node;

            while (current.Children.Count == 1 && current.Children[0].Children.Count > 0)
            {
                var child = current.Children[0];
                name = $"{name}/{child.Name}";
                fullPath = child.FullPath;
                current = child;
            }

            var compact = new Node(name, fullPath, node.Id);
            foreach (var child in current.Children)
                compact.Children.Add(Compact(child));

            return compact;
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

        private static DateTime LocalTimeOf(long unixSeconds)
        {
            try
            {
                return DateTimeOffset.FromUnixTimeSeconds(unixSeconds).LocalDateTime;
            }
            catch (ArgumentOutOfRangeException)
            {
                // A corrupt history entry should not stop the whole log from opening.
                return new DateTime(1970, 1, 1).ToLocalTime();
            }
        }

        [MenuItem("Tools/YKYToolkit/Import Watcher", false, Util.Three)]
        private static void ShowWindow()
        {
            var window = GetWindow<ImportWatcherWindow>();
            window.titleContent = ToolkitIcons.Content(Title, "import-watcher");
            window.minSize = new Vector2(640, 360);
        }

        private sealed class DateGroup
        {
            internal readonly DateTime Date;
            internal readonly List<TimeGroup> Times = new();

            internal DateGroup(DateTime date) => Date = date;
        }

        private sealed class TimeGroup
        {
            internal readonly DateTime LocalTime;
            internal readonly List<ImportRecord> Records = new();
            internal readonly long UnixSeconds;

            internal TimeGroup(long unixSeconds)
            {
                UnixSeconds = unixSeconds;
                LocalTime = LocalTimeOf(unixSeconds);
            }
        }

        private class Node
        {
            public readonly List<Node> Children = new();
            public readonly string FullPath;
            public readonly int Id;
            public readonly string Name;

            public Node(string name, string fullPath, int id)
            {
                Name = name;
                FullPath = fullPath;
                Id = id;
            }
        }
    }
}
