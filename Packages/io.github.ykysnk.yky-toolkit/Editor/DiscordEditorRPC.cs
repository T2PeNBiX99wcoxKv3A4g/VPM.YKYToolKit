using System;
using io.github.ykysnk.utils;
using io.github.ykysnk.ykyToolkit.Discord;
using JetBrains.Annotations;
using UnityEditor;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [InitializeOnLoad]
    public static class DiscordEditorRPC
    {
        public const string ApplicationId = "1465547633652138117";
        private const string ToolsMenuPath = "Tools/YKYToolkit/Enable Discord Rich Presence";
        private const double RetryInterval = 10.0;
        private static Discord.Discord? _discord;
        private static ActivityManager? _activityManager;
        private static string? _clientId;
        private static bool _initialized;
        private static long _startTime;
        private static double _lastRetryTime;

        static DiscordEditorRPC()
        {
            if (!EnableDiscordRichPresence) return;
            EditorApplication.update += Update;
            EditorApplication.quitting += Shutdown;
            AssemblyReloadEvents.beforeAssemblyReload += Shutdown;
            Initialize(ApplicationId);
        }

        public static bool EnableDiscordRichPresence
        {
            [PublicAPI]
            set
            {
                if (EditorPrefs.GetBool("YKYToolkit/LastEnableDiscordRichPresence", true) != value)
                {
                    if (value)
                        EditorApplication.delayCall += () => Initialize(ApplicationId);
                    else
                        Shutdown();
                }

                EditorPrefs.SetBool("YKYToolkit/LastEnableDiscordRichPresence",
                    EditorPrefs.GetBool("YKYToolkit/EnableDiscordRichPresence", true));
                EditorPrefs.SetBool("YKYToolkit/EnableDiscordRichPresence", value);
            }
            get => EditorPrefs.GetBool("YKYToolkit/EnableDiscordRichPresence", true);
        }

        [MenuItem(ToolsMenuPath, false, Util.Twe)]
        private static void EnableDiscordRichPresenceShowMenu() => EnableDiscordRichPresence = !EnableDiscordRichPresence;

        [MenuItem(ToolsMenuPath, true, Util.Twe)]
        private static bool EnableDiscordRichPresenceShowMenuValidate()
        {
            Menu.SetChecked(ToolsMenuPath, EnableDiscordRichPresence);
            return true;
        }

        [MenuItem("Tools/YKYToolkit/Discord SDK Initialize", false, Util.Four)]
        private static void MenuInitialize() => Initialize(ApplicationId);

        [MenuItem("Tools/YKYToolkit/Discord SDK Shutdown", false, Util.Four)]
        private static void MenuShutdown() => Shutdown();

        [MenuItem("Tools/YKYToolkit/Discord SDK Restart", false, Util.Four)]
        private static void MenuRestart()
        {
            Shutdown();
            ResetRetryTime();
            Utils.Log(nameof(DiscordEditorRPC), "Discord SDK will restart after 10 seconds.");
        }

        public static void Initialize(string id)
        {
            if (_initialized)
            {
                return;
            }

            _clientId = id;

            try
            {
                _discord = new(
                    long.Parse(_clientId),
                    (ulong)CreateFlags.NoRequireDiscord
                );

                _activityManager = _discord.GetActivityManager();
                _initialized = true;
                _startTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                Utils.Log(nameof(DiscordEditorRPC), "Discord SDK initialized.");
            }
            catch (Exception ex)
            {
                _discord = null;
                _activityManager = null;
                _initialized = false;
                ResetRetryTime();

                Utils.LogError(nameof(DiscordEditorRPC),
                    $"Failed to initialize Discord SDK. {ex.Message}\n{ex.StackTrace}");
            }
        }

        public static void SetActivity(string details, string state,
            string? largeImage = null, string? smallImage = null)
        {
            if (!_initialized || _discord == null || _activityManager == null)
                return;

            var activity = new Activity
            {
                Details = details,
                State = state,
                Timestamps = new()
                {
                    Start = _startTime
                }
            };

            if (!string.IsNullOrEmpty(largeImage) || !string.IsNullOrEmpty(smallImage))
                activity.Assets = new()
                {
                    LargeImage = largeImage ?? "",
                    SmallImage = smallImage ?? ""
                };

            _activityManager.UpdateActivity(activity, result =>
            {
                if (result != Result.Ok)
                    Utils.LogWarning(nameof(DiscordEditorRPC), $"UpdateActivity failed: {result}");
            });
        }

        public static void ClearActivity()
        {
            if (!_initialized || _discord == null || _activityManager == null)
                return;

            _activityManager.UpdateActivity(new(), _ =>
            {
                _activityManager.ClearActivity(result2 =>
                {
                    if (result2 != Result.Ok)
                        Utils.LogWarning(nameof(DiscordEditorRPC), $"ClearActivity failed: {result2}");
                });
            });
        }

        private static void Update()
        {
            if (!EnableDiscordRichPresence) return;

            if (!_initialized)
            {
                if (EditorApplication.timeSinceStartup - _lastRetryTime > RetryInterval)
                    Initialize(ApplicationId);

                return;
            }

            if (_discord == null) return;

            try
            {
                _discord.RunCallbacks();
            }
            catch (Exception ex)
            {
                Utils.LogWarning(nameof(DiscordEditorRPC),
                    $"Discord SDK callbacks failed. Attempting to reconnect. {ex.Message}\n{ex.StackTrace}");
                Shutdown();
                ResetRetryTime();
            }
        }

        public static void Shutdown()
        {
            if (!_initialized)
                return;

            try
            {
                _discord?.Dispose();
            }
            catch (Exception ex)
            {
                Utils.LogWarning(nameof(DiscordEditorRPC), $"Discord SDK dispose failed. {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                _discord = null;
                _activityManager = null;
                _initialized = false;
                ResetRetryTime();

                Utils.Log(nameof(DiscordEditorRPC), "Discord SDK shutdown.");
            }
        }

        private static void ResetRetryTime() => _lastRetryTime = EditorApplication.timeSinceStartup;
    }
}