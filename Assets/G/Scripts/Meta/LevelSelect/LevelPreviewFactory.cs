using System.Collections.Generic;
using UnityEngine;
using G.Core.Common;
using G.Gameplay.Nonogram.Level;

namespace G.Meta.LevelSelect
{
    public class LevelPreviewFactory : Service
    {
        private readonly Dictionary<string, Texture2D> _previews = new();

        public Texture2D GetPreview(NonogramLevelAsset level)
        {
            if (level == null)
                return null;

            if (_previews.TryGetValue(level.Id, out Texture2D cached))
                return cached;

            Texture2D preview = Build(level);
            _previews.Add(level.Id, preview);

            return preview;
        }

        protected override void OnDispose()
        {
            foreach (Texture2D preview in _previews.Values)
                Object.Destroy(preview);

            _previews.Clear();
        }

        private Texture2D Build(NonogramLevelAsset level)
        {
            Texture2D texture = new(level.Width, level.Height, TextureFormat.RGBA32, mipChain: false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };

            for (int row = 0; row < level.Height; row++)
            {
                int targetY = level.Height - 1 - row;

                for (int column = 0; column < level.Width; column++)
                    texture.SetPixel(column, targetY, level.GetColor(level.GetSolution(column, row)));
            }

            texture.Apply();
            return texture;
        }
    }
}
