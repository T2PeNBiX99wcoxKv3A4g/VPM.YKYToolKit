using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            Building
        }

        private static bool _initialized;

        static EditorActivityDetector()
        {
            EditorApplication.update += Update;
            EditorApplication.delayCall += DelayedInit;
        }

        public static EditorState CurrentState { get; private set; } = EditorState.Idle;
        public static string CurrentDetails { get; private set; } = "Idle in Unity Editor";

        private static void DelayedInit()
        {
            _initialized = true;
            UpdateDiscordState();
        }

        private static void Update()
        {
            if (EditorApplication.isCompiling)
            {
                SetState(EditorState.Compiling, () => "Reloading Domain…");
                return;
            }

            if (BuildPipeline.isBuildingPlayer)
            {
                SetState(EditorState.Building, () => "Building Project…");
                return;
            }

            if (EditorApplication.isPlaying)
            {
                SetState(EditorApplication.isPaused ? EditorState.Paused : EditorState.Playing,
                    () => $"Scene: {GetActiveSceneName()}");
                return;
            }

            if (IsEditingScript())
            {
                SetState(EditorState.EditingScript, () => $"File: {GetSelectedObjectName()}");
                return;
            }

            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                var prefab = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;
                SetState(EditorState.EditingPrefab, () => $"Prefab: {prefab.name}.prefab");
                return;
            }

            if (IsEditingMaterial())
            {
                SetState(EditorState.EditingMaterial, () => $"Material: {GetSelectedObjectName()}");
                return;
            }

            if (IsEditingShader())
            {
                SetState(EditorState.EditingShader, () => $"Shader: {GetSelectedObjectName()}");
                return;
            }

            if (SceneView.lastActiveSceneView != null &&
                EditorWindow.focusedWindow is SceneView)
            {
                SetState(EditorState.EditingScene, () => $"Scene: {GetActiveSceneName()}");
                return;
            }

            SetState(EditorState.Idle, () => "Browsing Project");
        }

        private static void SetState(EditorState state, Func<string> details)
        {
            // The state is recomputed every editor update; the details are only built when the state actually changes,
            // so the per frame string interpolation (and the Discord update) disappears.
            if (CurrentState == state) return;

            CurrentState = state;
            CurrentDetails = details();
            UpdateDiscordState();
        }

        private static void UpdateDiscordState()
        {
            if (!_initialized || !DiscordEditorRPC.EnableDiscordRichPresence) return;
            DiscordEditorRPC.SetActivity(EditorMainWindowTitle.GetTitleOrDefault("Unity Editor"), CurrentDetails,
                "unity-1024");
        }

        private static string GetActiveSceneName()
        {
            var scene = SceneManager.GetActiveScene();
            return scene.IsValid() ? scene.name : "Untitled";
        }

        private static string GetSelectedObjectName()
        {
            var obj = Selection.activeObject;
            return obj != null ? obj.name : "Unknown";
        }

        private static bool IsEditingScript()
        {
            var obj = Selection.activeObject;
            return obj is MonoScript;
        }

        private static bool IsEditingMaterial()
        {
            var obj = Selection.activeObject;
            return obj is Material;
        }

        private static bool IsEditingShader()
        {
            var obj = Selection.activeObject;
            return obj is Shader;
        }
    }
}