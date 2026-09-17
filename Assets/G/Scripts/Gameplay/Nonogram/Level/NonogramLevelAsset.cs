using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace G.Gameplay.Nonogram.Level
{
    [CreateAssetMenu(fileName = "NonogramLevel", menuName = "Configs/Nonogram/Level")]
    public class NonogramLevelAsset : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private Texture2D _sourceTexture;
        [SerializeField] private int _width;
        [SerializeField] private int _height;
        [SerializeField] private List<Color> _palette = new();
        [SerializeField] private List<byte> _solution = new();
        [SerializeField] private bool _playMonochrome = true;

        public string Id => _id;
        public bool PlayMonochrome => _playMonochrome;
        public int Width => _width;
        public int Height => _height;
        public int PaletteSize => _palette.Count;
        public IReadOnlyList<Color> Palette => _palette;
        public IReadOnlyList<byte> Solution => _solution;

        public Color GetColor(byte colorIndex)
        {
            if (colorIndex == 0 || colorIndex > _palette.Count)
                return Color.clear;

            return _palette[colorIndex - 1];
        }

        public byte GetSolution(int column, int row)
        {
            return _solution[row * _width + column];
        }

        public bool IsValid(out string problem)
        {
            StringBuilder problems = new();

            if (string.IsNullOrEmpty(_id))
                problems.AppendLine("пустой идентификатор");

            if (_width <= 0 || _height <= 0)
                problems.AppendLine($"недопустимый размер {_width}x{_height}");

            if (_palette.Count == 0)
                problems.AppendLine("пустая палитра");

            if (_width > 0 && _height > 0 && _solution.Count != _width * _height)
                problems.AppendLine($"решение из {_solution.Count} клеток не совпадает с размером {_width}x{_height}");

            bool hasFilled = false;

            for (int i = 0; i < _solution.Count; i++)
            {
                byte color = _solution[i];

                if (color > _palette.Count)
                {
                    problems.AppendLine($"клетка {i} ссылается на цвет {color}, которого нет в палитре");
                    break;
                }

                if (color != 0)
                    hasFilled = true;
            }

            if (hasFilled == false)
                problems.AppendLine("в решении нет ни одной закрашенной клетки");

            problem = problems.ToString().TrimEnd();
            return problem.Length == 0;
        }

#if UNITY_EDITOR
        [ContextMenu("Собрать из текстуры")]
        private void BuildFromTexture()
        {
            if (NonogramLevelImport.TryBuild(_sourceTexture, out int width, out int height,
                    out List<Color> palette, out List<byte> solution) == false)
                return;

            _width = width;
            _height = height;
            _palette = palette;
            _solution = solution;

            if (string.IsNullOrEmpty(_id))
                _id = name;

            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[Nonogram] Уровень {_id} собран: {width}x{height}, цветов {palette.Count}.", this);
        }
#endif
    }
}
