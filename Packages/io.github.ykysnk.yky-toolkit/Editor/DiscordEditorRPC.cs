using System;
using io.github.ykysnk.utils;
using io.github.ykysnk.utils.NonUdon;
using io.github.ykysnk.utils.NonUdon.Extensions;
using io.github.ykysnk.ykyToolkit.Editor.Discord;
using UnityEditor;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [InitializeOnLoad]
    public static class DiscordEditorRPC
    {
        public const string ApplicationId = "1465547633652138117";
        private const string EnableKey = "YKYToolkit/EnableDiscordRichPresence";
        private const string ToolsMenuPath = "Tools/YKYToolkit/Enable Discord Rich Presence";
        private const double RetryInterval = 3.0;
        private static Discord.Discord? _discord;
        private static ActivityManager? _activityManager;
        private static string? _clientId;
        private static bool _initialized;
        private static bool _updateSubscribed;
        private static long _startTime;
        private static double _lastRetryTime;

        static DiscordEditorRPC()
        {
            if (!EnableDiscordRichPresence) return;
            EnsureUpdateSubscribed();
            EditorApplication.quitting += Shutdown;
            AssemblyReloadEvents.beforeAssemblyReload += Shutdown;
            Initialize(ApplicationId);
        }

        internal static bool EnableDiscordRichPresence
        {
            get => EditorPrefs.GetBool(EnableKey, true);
            set
            {
                var previous = EnableDiscordRichPresence;
                EditorPrefs.SetBool(EnableKey, value);
                if (previous == value) return;

                if (value)
                {
                    EnsureUpdateSubscribed();
                    EditorApplication.delayCall += () => Initialize(ApplicationId);
                }
                else
                    Shutdown();
            }
        }

        internal static void ResetSettings() => EnableDiscordRichPresence = true;

        private static void EnsureUpdateSubscribed()
        {
            if (_updateSubscribed) return;
            _updateSubscribed = true;
            EditorApplication.update += Update;
        }

        [MenuItem(ToolsMenuPath, false, Util.Two)]
        private static void EnableDiscordRichPresenceShowMenu() => EnableDiscordRichPresence = !EnableDiscordRichPresence;

        [MenuItem(ToolsMenuPath, true, Util.Two)]
        private static bool EnableDiscordRichPresenceShowMenuValidate()
        {
            Menu.SetChecked(ToolsMenuPath, EnableDiscordRichPresence);
            return true;
        }

        [MenuItem("Tools/YKYToolkit/Discord SDK Initialize", false, Util.FourAfter)]
        private static void MenuInitialize() => Initialize(ApplicationId);

        [MenuItem("Tools/YKYToolkit/Discord SDK Shutdown", false, Util.FourAfter)]
        private static void MenuShutdown() => Shutdown();

        [MenuItem("Tools/YKYToolkit/Discord SDK Restart", false, Util.FourAfter)]
        private static void MenuRestart()
        {
            Shutdown();
            ResetRetryTime();
            Utils.Log(nameof(DiscordEditorRPC), $"Discord SDK will restart after {RetryInterval} seconds.");
        }

        public static void Initialize(string id)
        {
            if (_initialized)
                return;

            _clientId = id;

            Try.Run(() =>
            {
                _discord = new(
                    long.Parse(_clientId),
                    (ulong)CreateFlags.NoRequireDiscord
                );

                _activityManager = _discord.GetActivityManager();
                _initialized = true;
                _startTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                Utils.Log(nameof(DiscordEditorRPC), "Discord SDK initialized.");
            }).OnFailure(ex =>
            {
                _discord = null;
                _activityManager = null;
                _initialized = false;
                ResetRetryTime();

                Utils.LogError(nameof(DiscordEditorRPC),
                    $"Failed to initialize Discord SDK. {ex.Message}\n{ex.StackTrace}");
            });
        }

        public static void SetActivity(string details, string state,
            string? largeImage = null, string? smallImage = null, string? largeText = null, string? smallText = null,
            long? timestamp = null)
        {
            if (!_initialized || _discord == null || _activityManager == null)
                return;

            var activity = new Activity
            {
                Details = Clamp(details),
                State = Clamp(state),
                Timestamps = new()
                {
                    Start = timestamp ?? _startTime
                }
            };

            if (!string.IsNullOrEmpty(largeImage) || !string.IsNullOrEmpty(smallImage) ||
                !string.IsNullOrEmpty(largeText) || !string.IsNullOrEmpty(smallText))
                activity.Assets = new()
                {
                    LargeImage = largeImage ?? "",
                    LargeText = largeText ?? "",
                    SmallImage = smallImage ?? "",
                    SmallText = smallText ?? ""
                };

            _activityManager.UpdateActivity(activity, result =>
            {
                if (result != Result.Ok)
                    Utils.LogWarning(nameof(DiscordEditorRPC), $"UpdateActivity failed: {result}");
            });
        }

        private static string Clamp(string value) => value.Length <= 128 ? value : value[..128];

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

            Try.Run(() => _discord.RunCallbacks()).OnFailure(ex =>
            {
                Utils.LogWarning(nameof(DiscordEditorRPC),
                    $"Discord SDK callbacks failed. Attempting to reconnect. {ex.Message}\n{ex.StackTrace}");
                Shutdown();
                ResetRetryTime();
            });
        }

        public static void Shutdown()
        {
            if (!_initialized)
                return;

            Try.Run(() => _discord?.Dispose()).OnFailure(ex =>
                Utils.LogWarning(nameof(DiscordEditorRPC), $"Discord SDK dispose failed. {ex.Message}\n{ex.StackTrace}"));

            _discord = null;
            _activityManager = null;
            _initialized = false;
            ResetRetryTime();

            Utils.Log(nameof(DiscordEditorRPC), "Discord SDK shutdown.");
        }

        private static void ResetRetryTime() => _lastRetryTime = EditorApplication.timeSinceStartup;
    }
}