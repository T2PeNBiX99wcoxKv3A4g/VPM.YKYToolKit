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
            AddMetric(container, "CPU Usage", "Editor process CPU usage over the last 100 ms.",
                row => row.Update(SampleCpuPercent(), 100, PercentText));
            AddMetric(container, "Mono Used", "Managed heap in use / managed heap size.",
                row => row.Update(Profiler.GetMonoUsedSizeLong() / Megabyte,
                    Profiler.GetMonoHeapSizeLong() / Megabyte, MegabyteText));
            AddMetric(container, "Reserved Memory", "Memory Unity has reserved / total system memory.",
                row => row.Update(Profiler.GetTotalReservedMemoryLong() / Megabyte,
                    SystemInfo.systemMemorySize, MegabyteText));
            AddMetric(container, "Allocated Memory", "Memory Unity is using / memory it has reserved.",
                row => row.Update(Profiler.GetTotalAllocatedMemoryLong() / Megabyte,
                    Math.Max(1, Profiler.GetTotalReservedMemoryLong() / Megabyte), MegabyteText));
            AddMetric(container, "Graphics Memory",
                "Memory allocated for the graphics driver / video memory.", row =>
                {
                    var graphicsMemory = Profiler.GetAllocatedMemoryForGraphicsDriver() / Megabyte;
                    var videoMemory = SystemInfo.graphicsMemorySize;
                    if (videoMemory > 0)
                        row.Update(graphicsMemory, videoMemory, MegabyteText);
                    else
                        row.UpdateAbsolute(graphicsMemory, MegabyteText);
                });
            AddMetric(container, "Free Reserved", "Reserved memory that is not in use right now.",
                row => row.UpdateAbsolute(Profiler.GetTotalUnusedReservedMemoryLong() / Megabyte, MegabyteText));
            AddMetric(container, "Temp Allocator",
                "Size of the temporary native allocator, which render textures and short-lived buffers use.",
                row => row.UpdateAbsolute(Profiler.GetTempAllocatorSize() / Megabyte, MegabyteText));

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

        private long SampleCpuPercent()
        {
            if (_process == null) return 0;

            _process.Refresh();

            var currentCpuTime = _process.TotalProcessorTime.TotalMilliseconds;
            var currentCheckTime = EditorApplication.timeSinceStartup;
            var cpuDelta = currentCpuTime - _lastCpuTime;
            var timeDelta = (currentCheckTime - _lastCheckTime) * 1000.0;
            var usage = timeDelta > 0 ? cpuDelta / timeDelta * 100.0 / Environment.ProcessorCount : 0;

            _lastCpuTime = currentCpuTime;
            _lastCheckTime = currentCheckTime;

            return (long)Math.Round(usage);
        }

        private void UpdateMetrics()
        {
            foreach (var metric in _metrics)
            {
                metric.Sample();
                metric.Refresh();
            }
        }

        private void AddMetric(VisualElement parent, string title, string tooltip,
            Action<UnityResourceMonitorRow> sample)
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

            _metrics.Add(new MetricView(new UnityResourceMonitorRow(title), meter, fill, value, sample));
        }

        private static string MegabyteText(long value) => $"{value} MB";

        private static string PercentText(long value) => $"{value}%";

        [MenuItem("Tools/YKYToolkit/Unity Resource Monitor", false, Util.ThreeAfter)]
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
            private readonly Action<UnityResourceMonitorRow> _sample;

            private readonly UnityResourceMonitorRow _row;

            internal MetricView(UnityResourceMonitorRow row, VisualElement meter, VisualElement fill, Label value,
                Action<UnityResourceMonitorRow> sample)
            {
                _row = row;
                _meter = meter;
                _fill = fill;
                _value = value;
                _sample = sample;
            }

            internal void Sample() => _sample(_row);

            internal void Refresh()
            {
                _value.text = _row.text;

                // Metrics without a meaningful maximum only show their value.
                if (_row.maxValue <= 0)
                {
                    _meter.style.display = DisplayStyle.None;
                    _value.style.color = InfoColor;
                    return;
                }

                _meter.style.display = DisplayStyle.Flex;

                var ratio = Mathf.Clamp01((float)_row.value / _row.maxValue);
                _fill.style.width = Length.Percent(ratio * 100f);

                var color = ratio >= 0.8f ? CriticalColor : ratio >= 0.6f ? WarningColor : NormalColor;
                _fill.style.backgroundColor = color;
                _value.style.color = color;
            }
        }
    }
}
