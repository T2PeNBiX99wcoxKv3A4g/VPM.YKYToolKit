using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    /// <summary>The title of this Unity process's main window, used for the Discord rich presence.</summary>
    [PublicAPI]
    public static class EditorMainWindowTitle
    {
        private static IntPtr _mainWindow;
        private static uint _processId;

        public static string GetTitleOrDefault(string defaultValue)
        {
            var title = GetTitle();
            return string.IsNullOrWhiteSpace(title) ? defaultValue : title;
        }

        /// <summary>Title of this process's own main window, not the first Unity window on the desktop.</summary>
        public static string GetTitle()
        {
#if UNITY_EDITOR_WIN
            var window = MainWindow();
            if (window == IntPtr.Zero) return "";

            var text = new StringBuilder(512);
            return GetWindowText(window, text, text.Capacity) > 0 ? text.ToString() : "";
#else
            // The macOS/Linux implementations relied on AppKit/X11 entry points that do not exist, so the caller's
            // default is used instead.
            return "";
#endif
        }

        /// <summary>
        ///     Whether this process owns the foreground window. With several Unity editors open, only the focused one
        ///     should publish a Discord activity.
        /// </summary>
        public static bool IsCurrentProcessFocused()
        {
#if UNITY_EDITOR_WIN
            var foreground = GetForegroundWindow();
            if (foreground == IntPtr.Zero) return false;

            GetWindowThreadProcessId(foreground, out var processId);
            return processId == CurrentProcessId();
#else
            // Without a native foreground check, behave as before and let the last running instance win.
            return true;
#endif
        }

#if UNITY_EDITOR_WIN
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr window, StringBuilder text, int count);

        private static IntPtr MainWindow()
        {
            if (_mainWindow != IntPtr.Zero) return _mainWindow;

            using var process = Process.GetCurrentProcess();
            _mainWindow = process.MainWindowHandle;
            return _mainWindow;
        }

        private static uint CurrentProcessId()
        {
            if (_processId != 0) return _processId;

            using var process = Process.GetCurrentProcess();
            _processId = (uint)process.Id;
            return _processId;
        }
#endif
    }
}
