using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.UI;
using G.Core.Common;
using G.Gameplay.Nonogram.Board;
using G.Gameplay.Nonogram.Data;

namespace G.Gameplay.Nonogram.Presentation
{
    public class NonogramHudView : View
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private RectTransform _heartsRoot;
        [SerializeField] private Image _heartPrefab;
        [SerializeField] private GameObject _paletteRoot;
        [SerializeField] private RectTransform _paletteContent;
        [SerializeField] private NonogramColorSwatchView _swatchPrefab;
        [SerializeField] private Button _fillToolButton;
        [SerializeField] private Button _markToolButton;

        [Header("Цвета")]
        [SerializeField] private Color _heartAliveColor = new(0.9f, 0.27f, 0.35f, 1f);
        [SerializeField] private Color _heartLostColor = new(0.3f, 0.31f, 0.36f, 1f);
        [SerializeField] private Color _toolSelectedColor = new(0.35f, 0.62f, 0.95f, 1f);
        [SerializeField] private Color _toolIdleColor = new(0.22f, 0.24f, 0.3f, 1f);

        private readonly List<Image> _hearts = new();
        private readonly List<NonogramColorSwatchView> _swatches = new();

        private readonly Subject<Unit> _backClicked = new();
        private readonly Subject<PaintTool> _toolSelected = new();
        private readonly Subject<byte> _colorSelected = new();

        public Observable<Unit> BackClicked => _backClicked;
        public Observable<PaintTool> ToolSelected => _toolSelected;
        public Observable<byte> ColorSelected => _colorSelected;

        private void Awake()
        {
            _backButton.onClick.AddListener(OnBackClicked);
            _fillToolButton.onClick.AddListener(OnFillClicked);
            _markToolButton.onClick.AddListener(OnMarkClicked);
        }

        public void SetLives(int left, int total)
        {
            EnsureHearts(total);

            for (int i = 0; i < _hearts.Count; i++)
            {
                bool isUsed = i < total;

                _hearts[i].gameObject.SetActive(isUsed);

                if (isUsed)
                    _hearts[i].color = i < left ? _heartAliveColor : _heartLostColor;
            }
        }

        public void BuildPalette(INonogramBoardService board)
        {
            int count = board.IsMonochrome ? 0 : board.PaletteSize;

            _paletteRoot.SetActive(count > 1);

            EnsureSwatches(count);

            for (int i = 0; i < _swatches.Count; i++)
            {
                bool isUsed = i < count;

                _swatches[i].gameObject.SetActive(isUsed);

                if (isUsed)
                    _swatches[i].Apply((byte)(i + 1), board.GetColor((byte)(i + 1)));
            }
        }

        public void SetTool(PaintTool tool)
        {
            _fillToolButton.targetGraphic.color = tool == PaintTool.Fill ? _toolSelectedColor : _toolIdleColor;
            _markToolButton.targetGraphic.color = tool == PaintTool.Mark ? _toolSelectedColor : _toolIdleColor;
        }

        public void SetColor(byte colorIndex)
        {
            foreach (NonogramColorSwatchView swatch in _swatches)
                swatch.SetSelected(swatch.ColorIndex == colorIndex);
        }

        protected override void OnDestroyed()
        {
            _backButton.onClick.RemoveListener(OnBackClicked);
            _fillToolButton.onClick.RemoveListener(OnFillClicked);
            _markToolButton.onClick.RemoveListener(OnMarkClicked);

            _backClicked.Dispose();
            _toolSelected.Dispose();
            _colorSelected.Dispose();
        }

        private void EnsureHearts(int count)
        {
            while (_hearts.Count < count)
                _hearts.Add(Instantiate(_heartPrefab, _heartsRoot));
        }

        private void EnsureSwatches(int count)
        {
            while (_swatches.Count < count)
            {
                NonogramColorSwatchView swatch = Instantiate(_swatchPrefab, _paletteContent);

                Disposables.Add(swatch.Clicked.Subscribe(_ => _colorSelected.OnNext(swatch.ColorIndex)));
                _swatches.Add(swatch);
            }
        }

        private void OnBackClicked()
        {
            _backClicked.OnNext(Unit.Default);
        }

        private void OnFillClicked()
        {
            _toolSelected.OnNext(PaintTool.Fill);
        }

        private void OnMarkClicked()
        {
            _toolSelected.OnNext(PaintTool.Mark);
        }
    }
}
