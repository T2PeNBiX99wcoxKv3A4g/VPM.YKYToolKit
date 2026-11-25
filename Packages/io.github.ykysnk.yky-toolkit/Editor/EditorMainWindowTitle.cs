using System;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    /// <summary>The title of the Unity main window, used for the Discord rich presence.</summary>
    [PublicAPI]
    public static class EditorMainWindowTitle
    {
        public static string GetTitleOrDefault(string defaultValue)
        {
            var title = GetTitle();
            return string.IsNullOrWhiteSpace(title) ? defaultValue : title;
        }

        public static string GetTitle()
        {
#if UNITY_EDITOR_WIN
            return GetTitleWindows();
#else
            // The macOS/Linux implementations relied on AppKit/X11 entry points that do not exist (NSWindowTitle), so
            // they only ever threw and returned an empty string. The caller's default is used instead.
            return "";
#endif
        }

#if UNITY_EDITOR_WIN
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private static string GetTitleWindows()
        {
            var hwnd = FindWindow("UnityContainerWndClass", null);
            if (hwnd == IntPtr.Zero)
                hwnd = FindWindow("UnityWndClass", null);

            if (hwnd == IntPtr.Zero)
                return "";

            var sb = new StringBuilder(256);
            GetWindowText(hwnd, sb, sb.Capacity);
            return sb.ToString();
        }
#endif
    }
}