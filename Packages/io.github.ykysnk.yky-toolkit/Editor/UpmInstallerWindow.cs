using System.Collections.Generic;
using System.Linq;
using io.github.ykysnk.utils.Editor;
using io.github.ykysnk.utils.Editor.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal class UpmInstallerWindow : EditorWindow
    {
        private const string Title = "UPM Installer";

        private static readonly List<string> ToolPackages = new()
        {
            "com.unity.memoryprofiler",
            "com.unity.build-report-inspector"
        };

        [SerializeField] private VisualTreeAsset? uxml;
        [SerializeField] private List<UpmInstallerPackage> packages = new();
        [SerializeField] private List<UpmInstallerPackage> packageListMaker = new();
        [SerializeField] private bool isPackageListExpanded;

        private void CreateGUI()
        {
            var serializedObject = new SerializedObject(this);
            var tree = uxml!.CloneTree();
            InternalLocalizationExtensions.Helper.UILocalize(tree);
            tree.Bind(serializedObject);
            rootVisualElement.Add(tree);

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

            var quickInstallButton = tree.Q<Button>("quickInstall");
            quickInstallButton.clicked += () =>
            {
                packages.AddRange(ToolPackages.Select(x => new UpmInstallerPackage(x)));
                RebuildList(packages);
            };
        }

        private static void RebuildList(List<UpmInstallerPackage> packages)
        {
            packages.RemoveAll(x => string.IsNullOrEmpty(x.FullName));
            var newList = packages.Distinct().ToList();
            packages.Clear();
            packages.AddRange(newList);
        }

        [MenuItem("Tools/YKYToolkit/UPM Installer")]
        private static void ShowWindow()
        {
            var window = GetWindow<UpmInstallerWindow>();
            window.titleContent = EditorGUIUtils.IconContent(Title, "package manager");
        }
    }
}