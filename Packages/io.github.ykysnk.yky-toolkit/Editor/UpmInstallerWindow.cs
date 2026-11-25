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

            var installButton = tree.Q<Button>("install");
            installButton.clicked += () =>
            {
                RebuildList(packages);
                if (!packages.Any()) return;
                UpmInstaller.Install(packages.Select(x => x.FullName).ToArray());
            };

            var removeButton = tree.Q<Button>("remove");
            removeButton.clicked += () =>
            {
                RebuildList(packages);
                if (!packages.Any()) return;
                UpmInstaller.Remove(packages.Select(x => x.FullName).ToArray());
            };

            var updateButton = tree.Q<Button>("update");
            updateButton.clicked += () =>
            {
                RebuildList(packages);
                UpmInstaller.UpdateAsync().WaitEditor(updatePackage =>
                {
                    packages.AddRange(updatePackage.Select(x => new UpmInstallerPackage(x)));
                    RebuildList(packages);
                });
            };

            var upgradeButton = tree.Q<Button>("upgrade");
            upgradeButton.clicked += UpmInstaller.Upgrade;

            var clearButton = tree.Q<Button>("clear");
            clearButton.clicked += () => packages.Clear();

            var importToMakerButton = tree.Q<Button>("importToMaker");
            importToMakerButton.clicked += () =>
            {
                packageListMaker.AddRange(packages);
                RebuildList(packageListMaker);
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
                wrapper.wrapPackages.AddRange(packageListMaker);
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

                packageListMaker.Clear();
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

            var packageListsField = tree.Q<ListView>("packageLists");
            var removeSelectedButton = tree.Q<Button>("removeSelected");

            // Own add/remove rows: the built-in ListView footer floats over whatever comes next inside a card.
            var packagesField = tree.Q<ListView>("packages");
            tree.Q<Button>("addPackage").clicked += () =>
            {
                packages.Add(new("", ""));
                RebuildList(packages);
            };
            tree.Q<Button>("removePackage").clicked += () =>
            {
                foreach (var index in packagesField.selectedIndices.OrderByDescending(i => i).ToList())
                    if (index >= 0 && index < packages.Count)
                        packages.RemoveAt(index);
                RebuildList(packages);
            };

            var makerField = tree.Q<ListView>("packageListMaker");
            tree.Q<Button>("addMaker").clicked += () =>
            {
                packageListMaker.Add(new("", ""));
                RebuildList(packageListMaker);
            };
            tree.Q<Button>("removeMaker").clicked += () =>
            {
                foreach (var index in makerField.selectedIndices.OrderByDescending(i => i).ToList())
                    if (index >= 0 && index < packageListMaker.Count)
                        packageListMaker.RemoveAt(index);
                RebuildList(packageListMaker);
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