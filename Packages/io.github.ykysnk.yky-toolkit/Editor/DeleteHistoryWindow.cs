using System;
using System.Collections.Generic;
using System.IO;
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
    internal class DeleteHistoryWindow : EditorWindow
    {
        private const string DateFormat = "yyyy-MM-dd";
        private const string TimeFormat = "HH:mm:ss";

        private static readonly Color[] TagColors =
        {
            new(0.36f, 0.55f, 0.90f),
            new(0.35f, 0.72f, 0.50f),
            new(0.86f, 0.60f, 0.28f),
            new(0.66f, 0.45f, 0.86f),
            new(0.86f, 0.45f, 0.52f),
            new(0.35f, 0.70f, 0.78f)
        };

        [SerializeField] private VisualTreeAsset? uxml;

        private readonly List<DeleteRecord> _all = new();
        private readonly List<DateGroup> _dates = new();

        private ListView? _dateList;
        private ListView? _recordList;
        private Label? _detailTitle;
        private Label? _emptyLabel;

        private string _filter = string.Empty;
        private bool _historySubscribed;

        private void CreateGUI()
        {
            var tree = uxml!.CloneTree();
            tree.UILocalize(false);
            tree.style.flexGrow = 1;
            rootVisualElement.Add(tree);

            _dateList = tree.Q<ListView>("dateList");
            _recordList = tree.Q<ListView>("recordList");
            _detailTitle = tree.Q<Label>("detailTitle");
            _emptyLabel = tree.Q<Label>("emptyLabel");

            _dateList.makeItem = CreateDateRow;
            _dateList.bindItem = BindDateRow;
            _dateList.selectionChanged += _ => ShowSelectedDate();
            _dateList.LocalizeEmptyLabel();

            _recordList.makeItem = CreateRecordRow;
            _recordList.bindItem = BindRecordRow;

            tree.Q<ToolbarButton>("refreshButton").clicked += Refresh;
            tree.Q<ToolbarButton>("clearButton").clicked += () => UniTask.Create(async () =>
            {
                if (!await EditorUtils.DisplayDialogAsync(
                        "label.delete_history_window.clear_delete_history_title".S(),
                        "label.delete_history_window.clear_delete_history_message".S(),
                        "label.clear".S(),
                        "label.cancel".S()))
                    return;

                DeleteHistoryManager.Clear();
                Refresh();
            });

            var search = tree.Q<ToolbarSearchField>("search");
            search.SetupSearchField();
            search.RegisterValueChangedCallback(evt =>
            {
                _filter = evt.newValue ?? string.Empty;
                RebuildDates();
            });

            Refresh();

            if (_historySubscribed) return;
            _historySubscribed = true;
            DeleteHistoryManager.Changed += Refresh;
            rootVisualElement.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                DeleteHistoryManager.Changed -= Refresh;
                _historySubscribed = false;
            });
        }

        private static VisualElement CreateDateRow()
        {
            var row = new VisualElement();
            row.AddToClassList("delete-history-date");

            var label = new Label();
            label.AddToClassList("delete-history-date__label");

            var count = new Label();
            count.AddToClassList("yky-count");

            row.Add(label);
            row.Add(count);

            return row;
        }

        private void BindDateRow(VisualElement element, int index)
        {
            var label = element.Q<Label>(className: "delete-history-date__label");
            var count = element.Q<Label>(className: "yky-count");

            if (index < 0 || index >= _dates.Count)
            {
                element.userData = null;
                label.text = string.Empty;
                count.text = string.Empty;
                return;
            }

            var group = _dates[index];
            element.userData = group;
            label.text = group.Date.ToString(DateFormat);
            count.text = group.Records.Count.ToString();
        }

        private VisualElement CreateRecordRow()
        {
            var row = new VisualElement();
            row.AddToClassList("delete-history-row");

            var icon = new Image { name = "icon" };
            icon.AddToClassList("delete-history-row__icon");

            var text = new VisualElement();
            text.AddToClassList("delete-history-row__text");

            var nameRow = new VisualElement();
            nameRow.AddToClassList("delete-history-row__name-row");

            var name = new Label { name = "name" };
            name.AddToClassList("delete-history-row__name");

            var extension = new Label { name = "extension" };
            extension.AddToClassList("delete-history-row__ext");

            var folder = new Label { name = "folder" };
            folder.AddToClassList("delete-history-row__folder");

            var folderIcon = new Image
            {
                name = "folderIcon",
                image = EditorGUIUtility.FindTexture("Folder Icon")
            };
            folderIcon.AddToClassList("delete-history-row__folder-icon");

            var folderRow = new VisualElement();
            folderRow.AddToClassList("delete-history-row__folder-row");
            folderRow.Add(folderIcon);
            folderRow.Add(folder);

            var time = new Label { name = "time" };
            time.AddToClassList("delete-history-row__time");

            nameRow.Add(name);
            nameRow.Add(extension);
            text.Add(nameRow);
            text.Add(folderRow);
            row.Add(icon);
            row.Add(text);
            row.Add(time);

            // Deleting a file cannot be undone, but the folder it lived in often still exists.
            row.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.clickCount != 2 || row.userData is not DeleteRecord record) return;

                var folderPath = Path.GetDirectoryName(record.path)?.Replace('\\', '/');
                if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath)) return;

                var asset = AssetDatabase.LoadAssetAtPath<Object>(folderPath);
                if (asset == null) return;

                e.StopPropagation();
                EditorGUIUtility.PingObject(asset);
                Selection.activeObject = asset;
            });

            row.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                if (row.userData is not DeleteRecord record) return;

                evt.menu.AppendAction("label.delete_record.copy_path".S(),
                    _ => EditorGUIUtility.systemCopyBuffer = record.path);
                evt.menu.AppendAction("label.delete_record.copy_guid".S(),
                    _ => EditorGUIUtility.systemCopyBuffer = record.guid);
            }));

            return row;
        }

        private void BindRecordRow(VisualElement element, int index)
        {
            var name = element.Q<Label>("name");
            var extension = element.Q<Label>("extension");
            var folder = element.Q<Label>("folder");
            var folderRow = element.Q<VisualElement>(className: "delete-history-row__folder-row");
            var time = element.Q<Label>("time");
            var icon = element.Q<Image>("icon");

            var group = _dates.ElementAtOrDefault(_dateList?.selectedIndex ?? -1);
            var record = group != null && index >= 0 && index < group.Records.Count
                ? group.Records[index]
                : (DeleteRecord?)null;

            if (record == null)
            {
                element.userData = null;
                name.text = string.Empty;
                extension.text = string.Empty;
                extension.style.display = DisplayStyle.None;
                folder.text = string.Empty;
                folderRow.style.display = DisplayStyle.None;
                time.text = string.Empty;
                icon.image = null;
                return;
            }

            var value = record.Value;
            element.userData = value;

            var fileName = Path.GetFileName(value.path);
            name.text = string.IsNullOrEmpty(fileName) ? value.path : fileName;
            name.tooltip = value.path;

            var directory = Path.GetDirectoryName(value.path)?.Replace('\\', '/');
            folder.text = directory ?? string.Empty;
            folderRow.style.display = string.IsNullOrEmpty(directory) ? DisplayStyle.None : DisplayStyle.Flex;

            var extensionText = string.IsNullOrEmpty(value.extension) ? Path.GetExtension(value.path) : value.extension;
            extension.text = extensionText;
            if (string.IsNullOrEmpty(extensionText))
            {
                extension.style.display = DisplayStyle.None;
            }
            else
            {
                extension.style.display = DisplayStyle.Flex;
                var color = TagColor(extensionText);
                extension.style.color = color;
                extension.style.backgroundColor = new Color(color.r, color.g, color.b, 0.18f);
            }

            time.text = DateTimeOffset.FromUnixTimeSeconds(value.unixSeconds).LocalDateTime.ToString(TimeFormat);

            // A deleted path cannot be queried any more, so a folder is guessed from the missing extension.
            icon.image = LooksLikeFolder(value)
                ? EditorGUIUtility.FindTexture("Folder Icon") ??
                  InternalEditorUtility.GetIconForFile(value.path)
                : InternalEditorUtility.GetIconForFile(value.path) ??
                  EditorGUIUtility.FindTexture("DefaultAsset Icon");
        }

        private static bool LooksLikeFolder(DeleteRecord record)
        {
            if (AssetDatabase.IsValidFolder(record.path)) return true;

            var extension = string.IsNullOrEmpty(record.extension) ? Path.GetExtension(record.path) : record.extension;
            if (!string.IsNullOrEmpty(extension)) return false;

            return !Path.GetFileName(record.path).Contains('.');
        }

        private static Color TagColor(string extension)
        {
            var hash = 17;
            foreach (var c in extension)
                hash = hash * 31 + c;

            return TagColors[(hash & 0x7fffffff) % TagColors.Length];
        }

        private void Refresh()
        {
            _all.Clear();
            _all.AddRange(DeleteHistoryManager.All());
            _all.Reverse();

            RebuildDates();
        }

        private void RebuildDates()
        {
            var selectedDate = _dateList != null && _dateList.selectedIndex >= 0 &&
                               _dateList.selectedIndex < _dates.Count
                ? _dates[_dateList.selectedIndex].Date
                : (DateTime?)null;

            _dates.Clear();

            var map = new Dictionary<DateTime, DateGroup>();
            foreach (var record in _all)
            {
                if (!MatchesFilter(record)) continue;

                var date = DateTimeOffset.FromUnixTimeSeconds(record.unixSeconds).LocalDateTime.Date;
                if (!map.TryGetValue(date, out var group))
                {
                    group = new DateGroup(date);
                    map[date] = group;
                    _dates.Add(group);
                }

                group.Records.Add(record);
            }

            if (_dateList == null) return;

            _dateList.itemsSource = _dates;
            _dateList.Rebuild();

            var hasDates = _dates.Count > 0;
            if (_emptyLabel != null)
                _emptyLabel.style.display = hasDates ? DisplayStyle.None : DisplayStyle.Flex;
            if (_recordList != null)
                _recordList.style.display = hasDates ? DisplayStyle.Flex : DisplayStyle.None;
            if (_detailTitle != null)
                _detailTitle.style.display = hasDates ? DisplayStyle.Flex : DisplayStyle.None;

            if (!hasDates)
            {
                ClearDetail();
                return;
            }

            var index = selectedDate.HasValue ? _dates.FindIndex(group => group.Date == selectedDate.Value) : -1;
            _dateList.SetSelection(index >= 0 ? index : 0);
            ShowSelectedDate();
        }

        private bool MatchesFilter(DeleteRecord record)
        {
            if (string.IsNullOrEmpty(_filter)) return true;

            return record.path?.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   record.guid?.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   record.extension?.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void ShowSelectedDate()
        {
            if (_recordList == null) return;

            var group = _dates.ElementAtOrDefault(_dateList?.selectedIndex ?? -1);
            if (group == null)
            {
                ClearDetail();
                return;
            }

            if (_detailTitle != null)
                _detailTitle.text = $"{group.Date:yyyy-MM-dd}  ({group.Records.Count})";

            _recordList.itemsSource = group.Records;
            _recordList.Rebuild();
        }

        private void ClearDetail()
        {
            if (_recordList != null)
            {
                _recordList.itemsSource = new List<DeleteRecord>();
                _recordList.Rebuild();
            }

            if (_detailTitle != null) _detailTitle.text = string.Empty;
        }

        [MenuItem("Tools/YKYToolkit/Delete History", false, Util.ThreeAfter)]
        private static void ShowWindow()
        {
            var window = GetWindow<DeleteHistoryWindow>();
            window.titleContent = ToolkitIcons.Content("Delete History", "delete-history");
            window.minSize = new Vector2(620, 380);
        }

        private sealed class DateGroup
        {
            internal readonly DateTime Date;
            internal readonly List<DeleteRecord> Records = new();

            internal DateGroup(DateTime date) => Date = date;
        }
    }
}
