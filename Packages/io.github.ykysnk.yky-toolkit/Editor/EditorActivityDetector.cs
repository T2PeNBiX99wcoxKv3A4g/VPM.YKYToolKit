using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [InitializeOnLoad]
    public static class EditorActivityDetector
    {
        public enum EditorState
        {
            Idle,
            EditingScene,
            EditingScript,
            EditingPrefab,
            EditingMaterial,
            EditingShader,
            Playing,
            Paused,
            Compiling,
            Building,
            Importing
        }

        private const double PollInterval = 0.25;

        private static bool _initialized;
        private static bool _focused;
        private static double _nextPoll;
        private static long _playStartTime;
        private static string _lastDetails = string.Empty;
        private static string _lastState = string.Empty;

        static EditorActivityDetector()
        {
            EditorApplication.update += Update;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.delayCall += DelayedInit;
        }

        public static EditorState CurrentState { get; private set; } = EditorState.Idle;
        public static string CurrentDetails { get; private set; } = "Browsing project";

        private static void DelayedInit()
        {
            _initialized = true;
            _focused = EditorMainWindowTitle.IsCurrentProcessFocused();
            if (_focused) UpdateDiscordState(true);
        }

        private static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
                _playStartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            else if (change == PlayModeStateChange.EnteredEditMode)
                _playStartTime = 0;
        }

        private static void Update()
        {
            if (!DiscordEditorRPC.EnableDiscordRichPresence) return;

            var now = EditorApplication.timeSinceStartup;
            if (now < _nextPoll) return;
            _nextPoll = now + PollInterval;

            // With several Unity editors open, only the one owning the foreground window publishes an activity.
            var focused = EditorMainWindowTitle.IsCurrentProcessFocused();
            if (focused != _focused)
            {
                _focused = focused;
                if (focused) UpdateDiscordState(true);
            }

            if (!focused) return;

            if (EditorApplication.isCompiling)
            {
                SetState(EditorState.Compiling, "Reloading domain");
                return;
            }

            if (BuildPipeline.isBuildingPlayer)
            {
                SetState(EditorState.Building, "Building project");
                return;
            }

            if (EditorApplication.isUpdating)
            {
                SetState(EditorState.Importing, "Importing assets");
                return;
            }

            if (EditorApplication.isPlaying)
            {
                SetState(EditorApplication.isPaused ? EditorState.Paused : EditorState.Playing,
                    EditorApplication.isPaused ? "Paused" : "Playing");
                return;
            }

            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                SetState(EditorState.EditingPrefab, "Editing a prefab");
                return;
            }

            // The selected object type is enough; the name of the file is deliberately not published.
            if (Selection.activeObject is MonoScript)
            {
                SetState(EditorState.EditingScript, "Editing a script");
                return;
            }

            if (Selection.activeObject is Material)
            {
                SetState(EditorState.EditingMaterial, "Editing a material");
                return;
            }

            if (Selection.activeObject is Shader)
            {
                SetState(EditorState.EditingShader, "Editing a shader");
                return;
            }

            if (SceneView.lastActiveSceneView != null && EditorWindow.focusedWindow is SceneView)
            {
                SetState(EditorState.EditingScene, "Editing a scene");
                return;
            }

            SetState(EditorState.Idle, "Browsing project");
        }

        private static void SetState(EditorState state, string details)
        {
            // The state is recomputed on every poll; the details and the Discord update only happen on a real change.
            if (CurrentState == state) return;

            CurrentState = state;
            CurrentDetails = details;
            UpdateDiscordState();
        }

        private static void UpdateDiscordState(bool force = false)
        {
            if (!_initialized || !DiscordEditorRPC.EnableDiscordRichPresence) return;

            var details = EditorPresenceInfo.ProjectName;
            var state = $"{CurrentDetails} · {EditorPresenceInfo.Context}";

            if (!force && details == _lastDetails && state == _lastState) return;
            _lastDetails = details;
            _lastState = state;

            // While playing, the elapsed timer shows how long the play session has been running.
            var timestamp = EditorApplication.isPlaying && _playStartTime > 0 ? _playStartTime : (long?)null;
            DiscordEditorRPC.SetActivity(details, state, "unity-1024",
                largeText: EditorPresenceInfo.LargeText, timestamp: timestamp);
        }
    }
}
