using System.Collections.Generic;
using io.github.ykysnk.utils;
using io.github.ykysnk.utils.NonUdon;
using io.github.ykysnk.ykyToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace Test
{
    public static class TestClass
    {
        [MenuItem("Test/Create Empty Folders")]
        private static void Test()
        {
            for (var x = 0; x < 10; x++)
            {
                AssetDatabase.CreateFolder("Assets", $"{x}");

                for (var y = 0; y < 10; y++)
                {
                    AssetDatabase.CreateFolder($"Assets/{x}", $"{y}");

                    for (var z = 0; z < 10; z++)
                        AssetDatabase.CreateFolder($"Assets/{x}/{y}", $"{y}");
                }
            }
        }

        [MenuItem("Test/Discord RPC")]
        private static void TestDiscordRPC()
        {
            DiscordEditorRPC.Initialize(DiscordEditorRPC.ApplicationId);
        }

        [MenuItem("Test/Discord RPC Set Activity")]
        private static void TestDiscordSetActivity()
        {
            DiscordEditorRPC.SetActivity("Unity Editor Test", "Unity Editor Test State", "unity-1024");
        }

        [MenuItem("Test/Discord RPC Clear Activity")]
        private static void TestDiscordClearActivity()
        {
            DiscordEditorRPC.ClearActivity();
        }

        [MenuItem("Test/Discord RPC Shutdown")]
        private static void TestDiscordShutdown()
        {
            DiscordEditorRPC.Shutdown();
        }

        [MenuItem("Test/Test")]
        private static void Test2()
        {
            Utils.Log(nameof(TestClass), $"Test: {EditorMainWindowTitle.GetTitle()}");
        }

        [MenuItem("Test/Test2")]
        private static void Test3()
        {
            var list = new List<ImportWatcherFileColor>
            {
                new(".png", Color.cyan),
                new(".jpg", Color.magenta)
            };
            Utils.Log(nameof(Test3), $"Test: {JsonUtils.TryToJson(Wrapper.Create(list), out var json, out _)} {json}");
            Utils.Log(nameof(Test3),
                $"Test: {JsonUtils.TryFromJson<ListWrapper<ImportWatcherFileColor>>(json!, out var result, out _)} {result}");
        }
    }
}