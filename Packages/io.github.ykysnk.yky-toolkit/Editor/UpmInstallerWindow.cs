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
    // TODO: change packages string type to custom class
    internal class UpmInstallerWindow : EditorWindow
    {
        private const string Title = "UPM Installer";

        private static readonly List<string> ToolPackages = new()
        {
            "com.unity.memoryprofiler",
            "com.unity.build-report-inspector"
        };

        [SerializeField] private VisualTreeAsset? uxml;
        [SerializeField] private List<string> packages = new();
        [SerializeField] private bool isPackageListExpanded;

        private void CreateGUI()
        {
            var serializedObject = new SerializedObject(this);
            var tree = uxml?.CloneTree();
            tree.Bind(serializedObject);
            rootVisualElement.Add(tree);

            var installButton = tree.Q<Button>("install");
            installButton.clicked += () =>
            {
                RebuildList();
                if (!packages.Any()) return;
                UpmInstaller.Install(packages.ToArray());
            };

            var removeButton = tree.Q<Button>("remove");
            removeButton.clicked += () =>
            {
                RebuildList();
                if (!packages.Any()) return;
                UpmInstaller.Remove(packages.ToArray());
            };

            var updateButton = tree.Q<Button>("update");
            updateButton.clicked += () =>
            {
                UpmInstaller.UpdateAsync().WaitEditor(updatePackage =>
                {
                    packages.AddRange(updatePackage);
                    RebuildList();
                });
            };

            var upgradeButton = tree.Q<Button>("upgrade");
            upgradeButton.clicked += UpmInstaller.Upgrade;

            var clearButton = tree.Q<Button>("clear");
            clearButton.clicked += () => packages.Clear();

            var quickInstallButton = tree.Q<Button>("quickInstall");
            quickInstallButton.clicked += () =>
            {
                packages.AddRange(ToolPackages);
                RebuildList();
            };
        }

        private void RebuildList()
        {
            packages.RemoveAll(string.IsNullOrEmpty);
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