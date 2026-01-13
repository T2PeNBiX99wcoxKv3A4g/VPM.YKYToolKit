using UnityEditor;

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
    }
}