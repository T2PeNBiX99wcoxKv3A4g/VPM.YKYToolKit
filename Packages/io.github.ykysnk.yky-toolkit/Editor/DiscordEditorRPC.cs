using System;
using io.github.ykysnk.utils;
using io.github.ykysnk.ykyToolkit.Discord;
using JetBrains.Annotations;
using UnityEditor;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [InitializeOnLoad]
    [PublicAPI]
    public static class DiscordEditorRPC
    {
        public const string ApplicationId = "1465547633652138117";
        private const string ToolsMenuPath = "Tools/YKYToolkit/Enable Discord Rich Presence";
        private static Discord.Discord? _discord;
        private static ActivityManager? _activityManager;
        private static string? _clientId;
        private static bool _initialized;

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

        [MenuItem("Tools/YKYToolkit/Discord SDK Initialize")]
        private static void MenuInitialize() => Initialize(ApplicationId);

        [MenuItem("Tools/YKYToolkit/Discord SDK Shutdown")]
        private static void MenuShutdown() => Shutdown();

        [MenuItem("Tools/YKYToolkit/Discord SDK Restart")]
        private static void MenuRestart()
        {
            Shutdown();
            Initialize(ApplicationId);
        }

        private static void DelayedInit()
        {
            SetActivity(EditorMainWindowTitle.GetTitleOrDefault("Unity Editor"), "Unity Editor Test State", "unity-1024");
        }

        public static void Initialize(string id)
        {
            if (_initialized)
            {
                Utils.LogWarning(nameof(DiscordEditorRPC), "Already initialized.");
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

                Utils.Log(nameof(DiscordEditorRPC), "Discord SDK initialized.");
            }
            catch (Exception ex)
            {
                Utils.LogError(nameof(DiscordEditorRPC),
                    $"Failed to initialize Discord SDK.\n{ex.Message}\n{ex.StackTrace}");

                _discord = null;
                _activityManager = null;
                _initialized = false;
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
                State = state
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
            if (!_initialized || _discord == null)
                return;

            try
            {
                _discord.RunCallbacks();
            }
            catch (Exception ex)
            {
                Utils.LogWarning(nameof(DiscordEditorRPC),
                    $"Discord SDK callbacks failed.\n{ex.Message}\n{ex.StackTrace}");
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
                Utils.LogWarning(nameof(DiscordEditorRPC),
                    $"Discord SDK dispose failed.\n{ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                _discord = null;
                _activityManager = null;
                _initialized = false;

                Utils.Log(nameof(DiscordEditorRPC), "Discord SDK shutdown.");
            }
        }
    }
}