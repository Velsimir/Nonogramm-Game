using System.Collections.Generic;
using G.Core.Common;

namespace G.Core.Scenes
{
    public static class SceneNameMap
    {
        private static readonly Dictionary<ScenesName, string> Names = new()
        {
            { ScenesName.Boot, "Boot" },
            { ScenesName.Game, "Game" },
            { ScenesName.Menu, "Menu" },
        };

        public static IReadOnlyDictionary<ScenesName, string> All => Names;

        public static string Get(ScenesName scene)
        {
            if (Names.TryGetValue(scene, out string name))
                return name;

            GameDebug.LogConfigurationError($"[SceneNameMap] Для {scene} не задано имя сцены.");
            return scene.ToString();
        }
    }
}
