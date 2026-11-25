using io.github.ykysnk.Localization.Editor;
using System.Collections.Generic;
using System.Linq;
using io.github.ykysnk.utils;
using io.github.ykysnk.utils.Editor;
using io.github.ykysnk.utils.Editor.Extensions;
using io.github.ykysnk.utils.Extensions;
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

        private static UpmInstallerWindow? Instance { get; set; }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            SavePackageList();
            _serializedObject?.Dispose();
        }

        private SerializedObject? _serializedObject;

        private void CreateGUI()
        {
            Instance = this;
            _serializedObject?.Dispose();
            _serializedObject = new SerializedObject(this);
            var tree = uxml!.CloneTree();
            tree.UILocalize(false);
            tree.Bind(_serializedObject);
            rootVisualElement.Add(tree);
            LoadPackageList();

            var packagesField = tree.Q<ListView>("packages");
            var makerField = tree.Q<ListView>("packageListMaker");
            var packageListsField = tree.Q<ListView>("packageLists");

            var installButton = tree.Q<Button>("install");
            installButton.clicked += () =>
            {
                var entries = ReadEntries("packages");
                if (entries.Count == 0) return;
                UpmInstaller.Install(entries.Select(x => x.FullName).ToArray());
            };

            var removeButton = tree.Q<Button>("remove");
            removeButton.clicked += () =>
            {
                var entries = ReadEntries("packages");
                if (entries.Count == 0) return;
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
                packageLists.Add(wrapper);
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
                    packageLists.Add(result);
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
                AppendEntries("packages", new[] { new UpmInstallerPackage("", "") });
                packagesField.Rebuild();
            };
            tree.Q<Button>("removePackage").clicked += () =>
            {
                RemoveEntries("packages", packagesField.selectedIndices);
                packagesField.Rebuild();
            };

            tree.Q<Button>("addMaker").clicked += () =>
            {
                AppendEntries("packageListMaker", new[] { new UpmInstallerPackage("", "") });
                makerField.Rebuild();
            };
            tree.Q<Button>("removeMaker").clicked += () =>
            {
                RemoveEntries("packageListMaker", makerField.selectedIndices);
                makerField.Rebuild();
            };

            packageListsField.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                if (!packageListsField.selectedIndices.Any() || packageListsField.selectedIndex < 0)
                    return;

                evt.menu.AppendAction("label.upm_installer_window.copy".S(), _ => CopySelected());
                evt.menu.AppendAction("label.upm_installer_window.remove".S(), _ => RemoveSelected());
            }));

            removeSelectedButton.clicked += RemoveSelected;
            return;

            void RemoveSelected()
            {
                foreach (var index in packageListsField.selectedIndices.OrderByDescending(i => i))
                    packageLists.RemoveAt(index);

                packageListsField.Rebuild();
            }

            void CopySelected()
            {
                if (!JsonUtils.TryToJson(packageLists[packageListsField.selectedIndex], out var json, out var exception))
                {
                    Utils.LogError(nameof(UpmInstallerWindow),
                        $"Copy package list failed. {exception!.Message}\n{exception.StackTrace}");
                    return;
                }

                EditorGUIUtility.systemCopyBuffer = json;
            }
        }

        private SerializedProperty PackagesProperty => _serializedObject!.FindProperty("packages");

        private SerializedProperty MakerProperty => _serializedObject!.FindProperty("packageListMaker");

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
            Instance.packages.AddRange(wrapper.wrapPackages);
            RebuildList(Instance.packages);
            Instance.SavePackageList();
        }

        private static void RebuildList(List<UpmInstallerPackage> packages) =>
            packages.Rebuild(x => string.IsNullOrEmpty(x.FullName));

        [MenuItem("Tools/YKYToolkit/UPM Installer", false, Util.Three)]
        private static void ShowWindow()
        {
            var window = GetWindow<UpmInstallerWindow>();
            window.titleContent = EditorGUIUtils.IconContent(Title, "package manager");
            window.minSize = new(620, 420);
        }
    }
}