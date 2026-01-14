using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using io.github.ykysnk.utils.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal class UnityResourceMonitor : EditorWindow
    {
        private const string Title = "Unity Resource Monitor";

        [SerializeField] private VisualTreeAsset? uxml;
        [SerializeField] private List<UnityResourceMonitorRow> rows = new();

        private double _lastCheckTime;
        private double _lastCpuTime;
        private Process? _process;

        private void CreateGUI()
        {
            _process = Process.GetCurrentProcess();

            var serializedObject = new SerializedObject(this);
            var tree = uxml?.CloneTree();
            tree.Bind(serializedObject);
            rootVisualElement.Add(tree);
            rootVisualElement.AddManipulator(
                new ContextualMenuManipulator(evt => evt.menu.AppendAction("Reload", _ => Reload())));
            rootVisualElement.schedule.Execute(UpdateUI).Every(100);
            UpdateUI();
            return;

            void UpdateUI()
            {
                _process.Refresh();

                var currentCpuTime = _process.TotalProcessorTime.TotalMilliseconds;
                var currentCheckTime = EditorApplication.timeSinceStartup;

                var cpuDelta = currentCpuTime - _lastCpuTime;
                var timeDelta = (currentCheckTime - _lastCheckTime) * 1000.0;

                var cpuCount = Environment.ProcessorCount;
                var cpuUsage = cpuDelta / timeDelta * 100.0 / cpuCount;

                _lastCpuTime = _process.TotalProcessorTime.TotalMilliseconds;
                _lastCheckTime = EditorApplication.timeSinceStartup;

#if UNITY_EDITOR_WIN
                var userHandlesValue = GetGuiResources(_process.Handle, 0);
                var gdiHandlesValue = GetGuiResources(_process.Handle, 1);
#else
                var userHandlesValue = -1;
                var gdiHandlesValue = -1;
#endif

                foreach (var row in rows)
                    switch (row.id)
                    {
                        case 0:
                            row.Update(userHandlesValue, 10000, value => value > 0 ? $"{value}" : "N/A");
                            break;
                        case 1:
                            row.Update(gdiHandlesValue, 10000, value => value > 0 ? $"{value}" : "N/A");
                            break;
                        case 2:
                            row.Update(Profiler.GetMonoUsedSizeLong() / 1024 / 1024,
                                Profiler.GetMonoHeapSizeLong() / 1024 / 1024, value => $"{value} MB");
                            break;
                        case 3:
                            row.Update(Profiler.GetTotalReservedMemoryLong() / 1024 / 1024, SystemInfo.systemMemorySize,
                                value => $"{value} MB");
                            break;
                        case 4:
                            row.Update(Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024, SystemInfo.systemMemorySize,
                                value => $"{value} MB");
                            break;
                        case 5:
                            row.Update((long)cpuUsage, 100, value => $"{value}%");
                            break;
                    }
            }
        }

        private void Reload()
        {
            rows.Clear();
            rows.Add(new(0, "USER Handles"));
            rows.Add(new(1, "GDI Handles"));
            rows.Add(new(2, "Mono Used"));
            rows.Add(new(3, "GC Allocated"));
            rows.Add(new(4, "Total Allocated"));
            rows.Add(new(5, "CPU Usage"));
        }

#if UNITY_EDITOR_WIN
        [DllImport("user32.dll")]
        private static extern int GetGuiResources(IntPtr hProcess, int uiFlags);
#endif

        [MenuItem("Tools/YKYToolkit/Unity Resource Monitor")]
        private static void ShowWindow()
        {
            var window = GetWindow<UnityResourceMonitor>();

            window.titleContent = EditorGUIUtils.IconContent(Title, "Profiler.Memory");
            if (window.rows.Count < 1)
                window.Reload();
        }
    }
}