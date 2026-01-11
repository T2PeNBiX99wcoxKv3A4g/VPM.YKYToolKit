using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    public class UnityResourceMonitor : EditorWindow
    {
        private const string Title = "Unity Resource Monitor";

        [SerializeField] private VisualTreeAsset? uxml;
        [SerializeField] private List<UnityResourceMonitorRow> rows = new();

        public void CreateGUI()
        {
            var serializedObject = new SerializedObject(this);
            var tree = uxml?.CloneTree();
            tree.Bind(serializedObject);
            rootVisualElement.Add(tree);
            rootVisualElement.schedule.Execute(UpdateUI).Every(100);
            UpdateUI();
            return;

            void UpdateUI()
            {
                var process = Process.GetCurrentProcess();
                process.Refresh();

#if UNITY_EDITOR_WIN
                var userHandlesValue = GetGuiResources(process.Handle, 0);
                var gdiHandlesValue = GetGuiResources(process.Handle, 1);
#else
                var userHandlesValue = -1;
                var gdiHandlesValue = -1;
#endif

                foreach (var row in rows)
                    switch (row.title)
                    {
                        case "USER Handles":
                            row.Update(userHandlesValue, value => value > 0 ? $"{value}" : "N/A");
                            break;
                        case "GDI Handles":
                            row.Update(gdiHandlesValue, value => value > 0 ? $"{value}" : "N/A");
                            break;
                        case "Mono Used":
                            row.Update(Profiler.GetMonoUsedSizeLong() / 1024 / 1024, value => $"{value} MB");
                            break;
                        case "GC Alloc":
                            row.Update(Profiler.GetTotalReservedMemoryLong() / 1024 / 1024, value => $"{value} MB");
                            break;
                        case "Total Allocated":
                            row.Update(Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024, value => $"{value} MB");
                            break;
                    }
            }
        }

#if UNITY_EDITOR_WIN
        [DllImport("user32.dll")]
        private static extern int GetGuiResources(IntPtr hProcess, int uiFlags);
#endif

        [MenuItem("Tools/YKYToolkit/Unity Resource Monitor")]
        public static void ShowWindow()
        {
            var window = GetWindow<UnityResourceMonitor>(Title);
            window.rows.Clear();
            window.rows.Add(new("USER Handles", 10000));
            window.rows.Add(new("GDI Handles", 10000));
            window.rows.Add(new("Mono Used", Profiler.GetMonoHeapSizeLong() / 1024 / 1024));
            window.rows.Add(new("GC Alloc", SystemInfo.systemMemorySize));
            window.rows.Add(new("Total Allocated", SystemInfo.systemMemorySize));
        }
    }
}