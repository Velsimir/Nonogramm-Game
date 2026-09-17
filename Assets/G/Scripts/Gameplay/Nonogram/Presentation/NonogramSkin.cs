using UnityEngine;

namespace G.Gameplay.Nonogram.Presentation
{
    [CreateAssetMenu(fileName = "NonogramSkin", menuName = "Configs/Nonogram/Skin")]
    public class NonogramSkin : ScriptableObject
    {
        [Header("Спрайты")]
        [SerializeField] private Sprite _cellBackground;
        [SerializeField] private Sprite _cellFilled;
        [SerializeField] private Sprite _cellMarked;
        [SerializeField] private Sprite _clueChip;

        [Header("Цвета")]
        [SerializeField] private Color _cellBackgroundColor = new(0.93f, 0.93f, 0.95f, 1f);
        [SerializeField] private Color _cellMarkedColor = new(0.55f, 0.58f, 0.65f, 1f);
        [SerializeField] private Color _clueChipColor = new(0.93f, 0.93f, 0.95f, 1f);
        [SerializeField] private Color _clueTextColor = new(0.15f, 0.16f, 0.2f, 1f);
        [SerializeField] private Color _clueSolvedChipColor = new(0.32f, 0.34f, 0.42f, 1f);
        [SerializeField] private Color _clueSolvedTextColor = new(0.62f, 0.65f, 0.72f, 1f);
        [SerializeField] private Color _monochromeInkColor = new(0.18f, 0.19f, 0.24f, 1f);

        [Header("Раскраска после победы")]
        [SerializeField, Min(0.05f)] private float _revealCellDuration = 0.45f;
        [SerializeField, Min(0f)] private float _revealWaveDelay = 0.03f;

        [Header("Размеры")]
        [SerializeField, Min(8f)] private float _minCellSize = 22f;
        [SerializeField, Min(8f)] private float _maxCellSize = 110f;
        [SerializeField, Min(0f)] private float _cellSpacing = 2f;
        [SerializeField, Min(1)] private int _blockSize = 5;
        [SerializeField, Min(0f)] private float _blockSpacing = 6f;
        [SerializeField, Range(0.2f, 1f)] private float _clueFontRatio = 0.62f;
        [SerializeField, Min(0f)] private float _clueChipPadding = 4f;

        public Sprite CellBackground => _cellBackground;
        public Sprite CellFilled => _cellFilled;
        public Sprite CellMarked => _cellMarked;
        public Sprite ClueChip => _clueChip;

        public Color CellBackgroundColor => _cellBackgroundColor;
        public Color CellMarkedColor => _cellMarkedColor;
        public Color ClueChipColor => _clueChipColor;
        public Color ClueTextColor => _clueTextColor;
        public Color ClueSolvedChipColor => _clueSolvedChipColor;
        public Color ClueSolvedTextColor => _clueSolvedTextColor;
        public Color MonochromeInkColor => _monochromeInkColor;

        public float RevealCellDuration => _revealCellDuration;
        public float RevealWaveDelay => _revealWaveDelay;

        public float MinCellSize => _minCellSize;
        public float MaxCellSize => _maxCellSize;
        public float CellSpacing => _cellSpacing;
        public int BlockSize => _blockSize;
        public float BlockSpacing => _blockSpacing;
        public float ClueFontRatio => _clueFontRatio;
        public float ClueChipPadding => _clueChipPadding;
    }
}
