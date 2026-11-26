using System;
using System.IO;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    /// <summary>
    ///     Temporary: writes down what the history windows see while a docked window is recreated, so a window
    ///     that comes back empty can be explained without guessing.
    /// </summary>
    internal static class HistoryDebug
    {
        private static readonly string FilePath =
            Path.Combine(Path.GetTempPath(), "yky-history-debug.log");

        internal static void Log(string source, string message)
        {
            try
            {
                File.AppendAllText(FilePath, $"{DateTime.Now:HH:mm:ss.fff} [{source}] {message}{Environment.NewLine}");
            }
            catch
            {
                // Diagnostics must never break the window they describe.
            }
        }
    }
}
