using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    internal class UnityResourceMonitor : EditorWindow
    {
        private const string Title = "Unity Resource Monitor";
        private const int RefreshIntervalMs = 100;
        private const long Megabyte = 1024 * 1024;

        private static readonly Color NormalColor = new(0.34f, 0.78f, 0.48f);
        private static readonly Color WarningColor = new(0.94f, 0.65f, 0.20f);
        private static readonly Color CriticalColor = new(0.97f, 0.36f, 0.36f);
        private static readonly Color InfoColor = new(0.72f, 0.78f, 0.86f);

        [SerializeField] private VisualTreeAsset? uxml;

        private readonly List<MetricView> _metrics = new();
        private Process? _process;
        private double _lastCheckTime;
        private double _lastCpuTime;

        private void OnDestroy() => _process?.Dispose();

        private void CreateGUI()
        {
            _process = Process.GetCurrentProcess();

            var tree = uxml!.CloneTree();
            tree.style.flexGrow = 1;
            rootVisualElement.Add(tree);

            var container = tree.Q<VisualElement>("metrics");
            _metrics.Clear();
            _metrics.Add(AddMetric(container, 0, "CPU Usage",
                "Editor process CPU usage over the last 100 ms."));
            _metrics.Add(AddMetric(container, 1, "Mono Used",
                "Managed heap in use / managed heap size."));
            _metrics.Add(AddMetric(container, 2, "Reserved Memory",
                "Memory Unity has reserved / total system memory."));
            _metrics.Add(AddMetric(container, 3, "Allocated Memory",
                "Memory Unity is using / memory it has reserved."));
            _metrics.Add(AddMetric(container, 4, "Graphics Memory",
                "Memory allocated for the graphics driver / video memory."));
            _metrics.Add(AddMetric(container, 5, "Free Reserved",
                "Reserved memory that is not in use right now."));
            _metrics.Add(AddMetric(container, 6, "Temp Allocator",
                "Size of the temporary native allocator, which render textures and short-lived buffers use."));

            rootVisualElement.schedule.Execute(UpdateMetrics).Every(RefreshIntervalMs);

            // The first CPU sample needs a baseline, or the value is measured against the whole editor lifetime.
            ResetCpuSample();
            UpdateMetrics();
        }

        private void ResetCpuSample()
        {
            if (_process == null) return;

            _process.Refresh();
            _lastCpuTime = _process.TotalProcessorTime.TotalMilliseconds;
            _lastCheckTime = EditorApplication.timeSinceStartup;
        }

        private void UpdateMetrics()
        {
            if (_process == null) return;

            _process.Refresh();

            var currentCpuTime = _process.TotalProcessorTime.TotalMilliseconds;
            var currentCheckTime = EditorApplication.timeSinceStartup;
            var cpuDelta = currentCpuTime - _lastCpuTime;
            var timeDelta = (currentCheckTime - _lastCheckTime) * 1000.0;
            var cpuUsage = timeDelta > 0 ? cpuDelta / timeDelta * 100.0 / Environment.ProcessorCount : 0;

            _lastCpuTime = currentCpuTime;
            _lastCheckTime = currentCheckTime;

            var reservedMemory = Profiler.GetTotalReservedMemoryLong() / Megabyte;
            var graphicsMemory = Profiler.GetAllocatedMemoryForGraphicsDriver() / Megabyte;
            var videoMemory = SystemInfo.graphicsMemorySize;

            foreach (var metric in _metrics)
            {
                switch (metric.Row.id)
                {
                    case 0:
                        metric.Row.Update((long)Math.Round(cpuUsage), 100, PercentText);
                        break;
                    case 1:
                        metric.Row.Update(Profiler.GetMonoUsedSizeLong() / Megabyte,
                            Profiler.GetMonoHeapSizeLong() / Megabyte, MegabyteText);
                        break;
                    case 2:
                        metric.Row.Update(reservedMemory, SystemInfo.systemMemorySize, MegabyteText);
                        break;
                    case 3:
                        metric.Row.Update(Profiler.GetTotalAllocatedMemoryLong() / Megabyte,
                            Math.Max(1, reservedMemory), MegabyteText);
                        break;
                    case 4:
                        if (videoMemory > 0)
                            metric.Row.Update(graphicsMemory, videoMemory, MegabyteText);
                        else
                            metric.Row.UpdateAbsolute(graphicsMemory, MegabyteText);
                        break;
                    case 5:
                        metric.Row.UpdateAbsolute(Profiler.GetTotalUnusedReservedMemoryLong() / Megabyte,
                            MegabyteText);
                        break;
                    case 6:
                        metric.Row.UpdateAbsolute(Profiler.GetTempAllocatorSize() / Megabyte, MegabyteText);
                        break;
                }

                metric.Refresh();
            }
        }

        private static MetricView AddMetric(VisualElement parent, int id, string title, string tooltip)
        {
            var metric = new VisualElement();
            metric.AddToClassList("yky-metric");
            metric.tooltip = tooltip;

            var head = new VisualElement();
            head.AddToClassList("yky-metric__head");

            var name = new Label(title);
            name.AddToClassList("yky-metric__name");

            var value = new Label("-");
            value.AddToClassList("yky-metric__value");

            head.Add(name);
            head.Add(value);
            metric.Add(head);

            var meter = new VisualElement();
            meter.AddToClassList("yky-meter");
            var fill = new VisualElement();
            fill.AddToClassList("yky-meter__fill");
            meter.Add(fill);
            metric.Add(meter);

            parent.Add(metric);

            return new MetricView(new UnityResourceMonitorRow(id, title), meter, fill, value);
        }

        private static string MegabyteText(long value) => $"{value} MB";

        private static string PercentText(long value) => $"{value}%";

        [MenuItem("Tools/YKYToolkit/Unity Resource Monitor", false, Util.Three)]
        private static void ShowWindow()
        {
            var window = GetWindow<UnityResourceMonitor>();
            window.titleContent = ToolkitIcons.Content(Title, "resource-monitor");
            window.minSize = new Vector2(360, 300);
        }

        private sealed class MetricView
        {
            private readonly VisualElement _meter;
            private readonly VisualElement _fill;
            private readonly Label _value;

            internal readonly UnityResourceMonitorRow Row;

            internal MetricView(UnityResourceMonitorRow row, VisualElement meter, VisualElement fill, Label value)
            {
                Row = row;
                _meter = meter;
                _fill = fill;
                _value = value;
            }

            internal void Refresh()
            {
                _value.text = Row.text;

                // Metrics without a meaningful maximum only show their value.
                if (Row.maxValue <= 0)
                {
                    _meter.style.display = DisplayStyle.None;
                    _value.style.color = InfoColor;
                    return;
                }

                _meter.style.display = DisplayStyle.Flex;

                var ratio = Mathf.Clamp01((float)Row.value / Row.maxValue);
                _fill.style.width = Length.Percent(ratio * 100f);

                var color = ratio >= 0.8f ? CriticalColor : ratio >= 0.6f ? WarningColor : NormalColor;
                _fill.style.backgroundColor = color;
                _value.style.color = color;
            }
        }
    }
}
