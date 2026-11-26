using System.Collections.Generic;
using System.Linq;
using io.github.ykysnk.Localization.Editor;
using io.github.ykysnk.utils;
using io.github.ykysnk.utils.Editor;
using io.github.ykysnk.utils.Editor.Extensions;
using io.github.ykysnk.utils.NonUdon;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal class UpmInstallerWindow : EditorWindow
    {
        private const string Title = "UPM Installer";

        [SerializeField] private VisualTreeAsset? uxml;
        [SerializeField] private List<UpmInstallerPackage> packages = new();
        [SerializeField] private List<UpmInstallerPackage> packageListMaker = new();
        [SerializeField] private List<UpmPackageListWrapper> packageLists = new();
        private ListView? _makerField;
        private ScrollView? _packageListsScroll;
        private ListView? _packagesField;
        private int _selectedPackageListIndex = -1;

        private SerializedObject? _serializedObject;

        private static UpmInstallerWindow? Instance { get; set; }

        private SerializedProperty PackagesProperty => _serializedObject!.FindProperty("packages");

        private SerializedProperty MakerProperty => _serializedObject!.FindProperty("packageListMaker");

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            SavePackageList();
            _serializedObject?.Dispose();
        }

        private void CreateGUI()
        {
            Instance = this;
            _serializedObject?.Dispose();
            _serializedObject = new(this);
            // Load first: a list only picks up the entries that exist when it binds.
            LoadPackageList();

            var tree = uxml!.CloneTree();
            tree.UILocalize(false);
            tree.Bind(_serializedObject);
            rootVisualElement.Add(tree);
            // The template wrapper is not a flex child of the window root by default, so the page would only be as
            // tall as its content instead of filling the window.
            tree.style.flexGrow = 1;

            var packagesField = _packagesField = tree.Q<ListView>("packages");
            var makerField = _makerField = tree.Q<ListView>("packageListMaker");
            _packageListsScroll = tree.Q<ScrollView>("packageLists");
            _selectedPackageListIndex = -1;

            packagesField.LocalizeEmptyLabel();
            makerField.LocalizeEmptyLabel();

            AddPackageMenu(packagesField, "packages");
            AddPackageMenu(makerField, "packageListMaker");

            RebuildPackageListRows();

            var installButton = tree.Q<Button>("install");
            installButton.clicked += () =>
            {
                var entries = ReadEntries("packages").Where(x => !string.IsNullOrEmpty(x.packageName)).ToArray();
                if (entries.Length == 0) return;
                UpmInstaller.Install(entries.Select(x => x.FullName).ToArray());
            };

            var removeButton = tree.Q<Button>("remove");
            removeButton.clicked += () =>
            {
                var entries = ReadEntries("packages").Where(x => !string.IsNullOrEmpty(x.packageName)).ToArray();
                if (entries.Length == 0) return;
                UpmInstaller.Remove(entries.Select(x => x.FullName).ToArray());
            };

            var updateButton = tree.Q<Button>("update");
            updateButton.clicked += () =>
            {
                UpmInstaller.UpdateAsync().WaitEditor(updatePackage =>
                {
                    AppendEntries("packages", updatePackage.Select(x => new UpmInstallerPackage(x)));
                    packagesField.Rebuild();
                });
            };

            var upgradeButton = tree.Q<Button>("upgrade");
            upgradeButton.clicked += UpmInstaller.Upgrade;

            var clearButton = tree.Q<Button>("clear");
            clearButton.clicked += () =>
            {
                PackagesProperty.ClearArray();
                _serializedObject!.ApplyModifiedProperties();
                packagesField.Rebuild();
            };

            var importToMakerButton = tree.Q<Button>("importToMaker");
            importToMakerButton.clicked += () =>
            {
                AppendEntries("packageListMaker", ReadEntries("packages"));
                makerField.Rebuild();
            };

            var createListNameField = tree.Q<TextField>("createListName");
            var createAndCopyButton = tree.Q<Button>("createAndCopy");
            createAndCopyButton.clicked += () =>
            {
                var wrapper = new UpmPackageListWrapper
                {
                    wrapName = string.IsNullOrWhiteSpace(createListNameField.value)
                        ? "New Package List"
                        : createListNameField.value
                };
                wrapper.wrapPackages.AddRange(ReadEntries("packageListMaker"));
                AppendPackageList(wrapper);
                SavePackageList();

                if (JsonUtils.TryToJson(wrapper, out var json, out var exception))
                    EditorGUIUtility.systemCopyBuffer = json;
                else
                {
                    Utils.LogError(nameof(UpmInstallerWindow),
                        $"Failed to copy a package list. {exception!.Message}\n{exception.StackTrace}");
                    return;
                }

                MakerProperty.ClearArray();
                _serializedObject!.ApplyModifiedProperties();
                makerField.Rebuild();
                createListNameField.value = "";
            };

            var importButton = tree.Q<Button>("import");
            importButton.clicked += () =>
            {
                var json = EditorGUIUtility.systemCopyBuffer;

                if (JsonUtils.TryFromJson<UpmPackageListWrapper>(json, out var result, out var exception))
                {
                    Utils.Log(nameof(UpmInstallerWindow),
                        $"Imported package list: {result!.wrapName} - {result.wrapPackages.Count}");
                    AppendPackageList(result);
                    SavePackageList();
                }
                else
                {
                    Utils.LogError(nameof(UpmInstallerWindow),
                        $"Failed to import package list. {exception!.Message}\n{exception.StackTrace}");
                    SavePackageList();
                }
            };

            var removeSelectedButton = tree.Q<Button>("removeSelected");

            // Own add/remove rows: the built-in ListView footer floats over whatever comes next inside a card.
            tree.Q<Button>("addPackage").clicked += () =>
            {
                AppendEntries("packages", new[]
                {
                    new UpmInstallerPackage("", "")
                });
                packagesField.Rebuild();
            };
            tree.Q<Button>("removePackage").clicked += () =>
            {
                RemoveEntries("packages", packagesField.selectedIndices);
                packagesField.Rebuild();
            };

            tree.Q<Button>("addMaker").clicked += () =>
            {
                AppendEntries("packageListMaker", new[]
                {
                    new UpmInstallerPackage("", "")
                });
                makerField.Rebuild();
            };
            tree.Q<Button>("removeMaker").clicked += () =>
            {
                RemoveEntries("packageListMaker", makerField.selectedIndices);
                makerField.Rebuild();
            };

            removeSelectedButton.clicked += RemoveSelected;
            return;

            void RemoveSelected() => RemovePackageList(_selectedPackageListIndex);
        }

        private void RemovePackageList(int index)
        {
            if (index < 0 || index >= packageLists.Count) return;

            RemoveEntries("packageLists", new[]
            {
                index
            });
            _selectedPackageListIndex = -1;
            RebuildPackageListRows();
            SavePackageList();
        }

        private void CopyPackageList(int index)
        {
            if (index < 0 || index >= packageLists.Count) return;

            if (!JsonUtils.TryToJson(packageLists[index], out var json, out var exception))
            {
                Utils.LogError(nameof(UpmInstallerWindow),
                    $"Copy package list failed. {exception!.Message}\n{exception.StackTrace}");
                return;
            }

            EditorGUIUtility.systemCopyBuffer = json;
        }

        private void AddPackageMenu(ListView list, string propertyName)
        {
            list.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                var selection = list.selectedIndices.ToList();
                if (selection.Count == 0) return;

                evt.menu.AppendAction("label.upm_installer_window.copy".S(), _ =>
                {
                    var entries = ReadEntries(propertyName);
                    EditorGUIUtility.systemCopyBuffer = string.Join("\n",
                        selection.Where(i => i >= 0 && i < entries.Count).Select(i => entries[i].FullName));
                });

                evt.menu.AppendAction("label.upm_installer_window.remove".S(), _ =>
                {
                    RemoveEntries(propertyName, selection);
                    list.Rebuild();
                });
            }));
        }

        /// <summary>
        ///     The saved lists are not a virtualized ListView on purpose: each row is a drawer that can expand to show its
        ///     packages, and a virtualized list does not remeasure an item when its drawer grows (the rows then overlap).
        /// </summary>
        private void RebuildPackageListRows()
        {
            if (_packageListsScroll == null) return;

            var container = _packageListsScroll.contentContainer;
            container.Clear();
            _serializedObject!.Update();

            var property = _serializedObject.FindProperty("packageLists");
            if (property.arraySize == 0)
            {
                var empty = new Label("label.list_empty".S());
                empty.AddToClassList("yky-list-empty");
                container.Add(empty);
                return;
            }

            for (var i = 0; i < property.arraySize; i++)
            {
                var index = i;
                var row = new PropertyField(property.GetArrayElementAtIndex(i));
                row.AddToClassList("yky-list-row");
                row.EnableInClassList("yky-list-row--selected", index == _selectedPackageListIndex);
                row.RegisterCallback<PointerDownEvent>(_ => SelectPackageList(index), TrickleDown.TrickleDown);
                row.AddManipulator(new ContextualMenuManipulator(evt =>
                {
                    SelectPackageList(index);
                    evt.menu.AppendAction("label.upm_installer_window.copy".S(), _ => CopyPackageList(index));
                    evt.menu.AppendAction("label.upm_installer_window.remove".S(), _ => RemovePackageList(index));
                }));

                container.Add(row);
                row.Bind(_serializedObject);
            }
        }

        private void SelectPackageList(int index)
        {
            _selectedPackageListIndex = index;
            if (_packageListsScroll == null) return;

            var rows = _packageListsScroll.contentContainer.Children().ToList();
            for (var i = 0; i < rows.Count; i++) rows[i].EnableInClassList("yky-list-row--selected", i == index);
        }

        private void AppendPackageList(UpmPackageListWrapper wrapper)
        {
            var property = _serializedObject!.FindProperty("packageLists");
            property.arraySize++;
            var element = property.GetArrayElementAtIndex(property.arraySize - 1);
            element.FindPropertyRelative("wrapName").stringValue = wrapper.wrapName;

            var packagesProperty = element.FindPropertyRelative("wrapPackages");
            packagesProperty.arraySize = wrapper.wrapPackages.Count;
            for (var i = 0; i < wrapper.wrapPackages.Count; i++)
            {
                var package = packagesProperty.GetArrayElementAtIndex(i);
                package.FindPropertyRelative("packageName").stringValue = wrapper.wrapPackages[i].packageName;
                package.FindPropertyRelative("version").stringValue = wrapper.wrapPackages[i].version;
            }

            _serializedObject.ApplyModifiedProperties();
            _selectedPackageListIndex = property.arraySize - 1;
            RebuildPackageListRows();
        }

        /// <summary>
        ///     The lists are bound to their SerializedProperty, so they have to be edited through it; changing the
        ///     managed list directly never reaches the ListView.
        /// </summary>
        private List<UpmInstallerPackage> ReadEntries(string propertyName)
        {
            var property = _serializedObject!.FindProperty(propertyName);
            var entries = new List<UpmInstallerPackage>(property.arraySize);

            for (var i = 0; i < property.arraySize; i++)
            {
                var element = property.GetArrayElementAtIndex(i);
                entries.Add(new(
                    element.FindPropertyRelative("packageName").stringValue,
                    element.FindPropertyRelative("version").stringValue));
            }

            return entries;
        }

        private void AppendEntries(string propertyName, IEnumerable<UpmInstallerPackage> entries)
        {
            var property = _serializedObject!.FindProperty(propertyName);

            foreach (var entry in entries)
            {
                property.arraySize++;
                var element = property.GetArrayElementAtIndex(property.arraySize - 1);
                element.FindPropertyRelative("packageName").stringValue = entry.packageName;
                element.FindPropertyRelative("version").stringValue = entry.version;
            }

            _serializedObject.ApplyModifiedProperties();
        }

        private void RemoveEntries(string propertyName, IEnumerable<int> indices)
        {
            var property = _serializedObject!.FindProperty(propertyName);

            foreach (var index in indices.OrderByDescending(i => i))
            {
                if (index < 0 || index >= property.arraySize) continue;

                var size = property.arraySize;
                property.DeleteArrayElementAtIndex(index);
                // A class element is nulled first; the second call removes the entry itself.
                if (property.arraySize == size) property.DeleteArrayElementAtIndex(index);
            }

            _serializedObject.ApplyModifiedProperties();
        }

        private void SavePackageList()

        {
            if (!JsonUtils.TryToJson(Wrapper.Create(packageLists), out var json, out _)) return;

            EditorPrefs.SetString("YKYToolkit/UpmInstallerWindowPackageLists", json);
        }

        private void LoadPackageList()
        {
            var json = EditorPrefs.GetString("YKYToolkit/UpmInstallerWindowPackageLists");

            if (!JsonUtils.TryFromJson<ListWrapper<UpmPackageListWrapper>>(json, out var result, out _)) return;

            packageLists.Clear();
            packageLists.AddRange(result!);
        }

        public static void ImportPackages(UpmPackageListWrapper wrapper)
        {
            if (Instance == null) return;
            Utils.Log(nameof(UpmInstallerWindow),
                $"Importing package list: {wrapper.wrapName} - {wrapper.wrapPackages.Count}");
            Instance.AppendEntries("packages", wrapper.wrapPackages.Where(x => !string.IsNullOrEmpty(x.packageName)));
            Instance._packagesField?.Rebuild();
            Instance.SavePackageList();
        }

        [MenuItem("Tools/YKYToolkit/UPM Installer", false, Util.ThreeAfter)]
        private static void ShowWindow()
        {
            var window = GetWindow<UpmInstallerWindow>();
            window.titleContent = ToolkitIcons.Content(Title, "upm-installer");
            window.minSize = new(620, 420);
        }
    }
}