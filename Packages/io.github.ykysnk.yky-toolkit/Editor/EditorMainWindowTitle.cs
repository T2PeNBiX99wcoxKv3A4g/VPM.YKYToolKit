using System;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;

namespace io.github.ykysnk.ykyToolkit.Editor
{
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
            return GetTitle_Windows();
#elif UNITY_EDITOR_OSX
            return GetTitle_macOS();
#elif UNITY_EDITOR_LINUX
            return GetTitle_Linux();
#else
            return "";
#endif
        }

        // ───────────────────────────────────────────────────────────────
        // Windows (Win32)
        // ───────────────────────────────────────────────────────────────
#if UNITY_EDITOR_WIN
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private static string GetTitle_Windows()
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

        // ───────────────────────────────────────────────────────────────
        // macOS (Cocoa)
        // ───────────────────────────────────────────────────────────────
#if UNITY_EDITOR_OSX
        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        private static extern IntPtr NSApplicationSharedApplication();
    
        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        private static extern IntPtr NSAppMainWindow(IntPtr nsApp);
    
        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        private static extern IntPtr NSWindowTitle(IntPtr nsWindow);
    
        private static string GetTitle_macOS()
        {
            try
            {
                IntPtr app = NSApplicationSharedApplication();
                IntPtr window = NSAppMainWindow(app);
                IntPtr titlePtr = NSWindowTitle(window);
    
                return Marshal.PtrToStringAuto(titlePtr) ?? "";
            }
            catch
            {
                return "";
            }
        }
#endif

        // ───────────────────────────────────────────────────────────────
        // Linux (X11)
        // ───────────────────────────────────────────────────────────────
#if UNITY_EDITOR_LINUX
        [DllImport("libX11")]
        private static extern IntPtr XOpenDisplay(IntPtr display);
    
        [DllImport("libX11")]
        private static extern int XFetchName(IntPtr display, IntPtr window, out IntPtr windowName);
    
        [DllImport("libX11")]
        private static extern int XCloseDisplay(IntPtr display);
    
        private static string GetTitle_Linux()
        {
            try
            {
                IntPtr display = XOpenDisplay(IntPtr.Zero);
                if (display == IntPtr.Zero)
                    return "";
    
                IntPtr rootWindow = IntPtr.Zero;
                IntPtr namePtr;
    
                if (XFetchName(display, rootWindow, out namePtr) != 0)
                {
                    string title = Marshal.PtrToStringAnsi(namePtr);
                    XCloseDisplay(display);
                    return title ?? "";
                }
    
                XCloseDisplay(display);
                return "";
            }
            catch
            {
                return "";
            }
        }
#endif
    }
}