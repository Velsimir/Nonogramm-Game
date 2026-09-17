#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace G.Gameplay.Nonogram.Level
{
    public static class NonogramLevelImport
    {
        private const float COLOR_TOLERANCE = 0.08f;
        private const byte ALPHA_THRESHOLD = 128;
        private const int MAX_SIDE = 40;
        private const int MAX_PALETTE = 16;

        public static bool TryBuild(Texture2D texture, out int width, out int height,
            out List<Color> palette, out List<byte> solution)
        {
            width = 0;
            height = 0;
            palette = new List<Color>();
            solution = new List<byte>();

            if (texture == null)
            {
                Debug.LogError("[Nonogram] Не назначена исходная текстура уровня.");
                return false;
            }

            if (texture.isReadable == false)
            {
                Debug.LogError($"[Nonogram] У текстуры {texture.name} выключен Read/Write Enabled, " +
                    "включи его в настройках импорта.", texture);
                return false;
            }

            if (texture.width > MAX_SIDE || texture.height > MAX_SIDE)
            {
                Debug.LogError($"[Nonogram] Текстура {texture.name} размером {texture.width}x{texture.height} " +
                    $"больше допустимых {MAX_SIDE}x{MAX_SIDE}.", texture);
                return false;
            }

            width = texture.width;
            height = texture.height;

            Color32[] pixels = texture.GetPixels32();
            solution.Capacity = width * height;

            for (int row = 0; row < height; row++)
            {
                int sourceRow = height - 1 - row;

                for (int column = 0; column < width; column++)
                    solution.Add(ResolveColorIndex(pixels[sourceRow * width + column], palette));
            }

            if (palette.Count == 0)
            {
                Debug.LogError($"[Nonogram] В текстуре {texture.name} нет ни одного непрозрачного пикселя.", texture);
                return false;
            }

            if (palette.Count > MAX_PALETTE)
            {
                Debug.LogError($"[Nonogram] В текстуре {texture.name} {palette.Count} различимых цветов, " +
                    $"допустимо не больше {MAX_PALETTE}. Сведи палитру в графическом редакторе.", texture);
                return false;
            }

            return true;
        }

        private static byte ResolveColorIndex(Color32 pixel, List<Color> palette)
        {
            if (pixel.a < ALPHA_THRESHOLD)
                return 0;

            Color color = new Color32(pixel.r, pixel.g, pixel.b, byte.MaxValue);

            for (int i = 0; i < palette.Count; i++)
            {
                if (IsSameColor(palette[i], color))
                    return (byte)(i + 1);
            }

            palette.Add(color);
            return (byte)palette.Count;
        }

        private static bool IsSameColor(Color left, Color right)
        {
            return Mathf.Abs(left.r - right.r) <= COLOR_TOLERANCE
                && Mathf.Abs(left.g - right.g) <= COLOR_TOLERANCE
                && Mathf.Abs(left.b - right.b) <= COLOR_TOLERANCE;
        }
    }
}
#endif
