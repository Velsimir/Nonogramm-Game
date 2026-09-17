using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using G.Core.Common;
using G.Gameplay.Nonogram.Board;
using G.Gameplay.Nonogram.Data;

namespace G.Gameplay.Nonogram.Presentation
{
    public class NonogramBoardView : View
    {
        private static readonly Vector2 TopLeft = new(0f, 1f);
        private static readonly Vector2 Center = new(0.5f, 0.5f);

        [SerializeField] private NonogramSkin _skin;
        [SerializeField] private RectTransform _viewport;
        [SerializeField] private RectTransform _content;
        [SerializeField] private RectTransform _gridRoot;
        [SerializeField] private RectTransform _rowCluesRoot;
        [SerializeField] private RectTransform _columnCluesRoot;
        [SerializeField] private NonogramCellView _cellPrefab;
        [SerializeField] private NonogramClueView _cluePrefab;
        [SerializeField] private NonogramBoardInput _input;

        private readonly List<NonogramCellView> _cellPool = new();
        private readonly List<NonogramClueView> _cluePool = new();

        private NonogramCellView[] _cellViews = Array.Empty<NonogramCellView>();
        private List<NonogramClueView>[] _rowClueViews = Array.Empty<List<NonogramClueView>>();
        private List<NonogramClueView>[] _columnClueViews = Array.Empty<List<NonogramClueView>>();
        private float[] _columnOffsets = Array.Empty<float>();
        private float[] _rowOffsets = Array.Empty<float>();

        private INonogramBoardService _builtBoard;
        private int _width;
        private int _height;
        private int _usedCells;
        private int _usedClues;
        private float _cellSize;
        private bool _isBuilding;
        private bool _isRevealed;

        public NonogramBoardInput Input => _input;

        public void Build(INonogramBoardService board)
        {
            BuildInternal(board, resetReveal: true);
        }

        public async UniTask PlayRevealAsync(INonogramBoardService board, CancellationToken cancellationToken)
        {
            if (IsBuiltFor(board) == false || board.IsMonochrome == false)
                return;

            _isRevealed = true;

            using CancellationTokenSource linked =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, DestroyToken);

            List<UniTask> reveals = new();

            for (int row = 0; row < _height; row++)
            {
                for (int column = 0; column < _width; column++)
                {
                    CellAddress address = new(column, row);

                    if (board.GetCell(address).IsFilled == false)
                        continue;

                    NonogramCellView view = _cellViews[row * _width + column];

                    if (view == null)
                        continue;

                    reveals.Add(view.RevealAsync(board.GetRevealColor(address), _skin.RevealCellDuration,
                        (column + row) * _skin.RevealWaveDelay, linked.Token));
                }
            }

            await UniTask.WhenAll(reveals);
        }

        private void BuildInternal(INonogramBoardService board, bool resetReveal)
        {
            if (resetReveal)
                _isRevealed = false;

            if (board == null || board.HasBoard == false)
            {
                Clear();
                return;
            }

            if (IsConfigured() == false)
                return;

            if (_isBuilding)
                return;

            _isBuilding = true;
            _builtBoard = board;
            _width = board.Width;
            _height = board.Height;
            _usedCells = 0;
            _usedClues = 0;

            int maxRowClues = GetMaxClues(board, LineAxis.Row, _height);
            int maxColumnClues = GetMaxClues(board, LineAxis.Column, _width);

            _cellSize = ResolveCellSize(maxRowClues, maxColumnClues);
            _columnOffsets = BuildOffsets(_width, _cellSize);
            _rowOffsets = BuildOffsets(_height, _cellSize);

            float bandWidth = maxRowClues * _cellSize;
            float bandHeight = maxColumnClues * _cellSize;
            float gridWidth = _columnOffsets[_width - 1] + _cellSize;
            float gridHeight = _rowOffsets[_height - 1] + _cellSize;

            ApplyRoots(bandWidth, bandHeight, gridWidth, gridHeight);
            BuildCells(board);
            BuildClues(board, maxRowClues, maxColumnClues, bandWidth, bandHeight);
            HideUnused();

            _input.SetLayout(_columnOffsets, _rowOffsets, _cellSize);
            _isBuilding = false;
        }

        public void RefreshCell(INonogramBoardService board, CellAddress address)
        {
            if (IsBuiltFor(board) == false || board.IsInside(address) == false)
                return;

            NonogramCellView view = _cellViews[address.Row * _width + address.Column];

            if (view == null)
                return;

            NonogramCell cell = board.GetCell(address);
            view.Apply(_skin, cell, GetFillColor(board, address, cell));
        }

        public void RefreshLine(INonogramBoardService board, LineAddress line)
        {
            if (IsBuiltFor(board) == false)
                return;

            List<NonogramClueView>[] source = line.Axis == LineAxis.Row ? _rowClueViews : _columnClueViews;

            if (line.Index < 0 || line.Index >= source.Length)
                return;

            IReadOnlyList<NonogramClue> clues = board.GetClues(line);
            List<NonogramClueView> views = source[line.Index];
            bool isSolved = board.IsLineSolved(line);
            bool isMonochrome = board.PaletteSize == 1;

            for (int i = 0; i < views.Count && i < clues.Count; i++)
                views[i].Apply(_skin, clues[i], board.GetColor(clues[i].ColorIndex), isMonochrome, isSolved);
        }

        public void RefreshAll(INonogramBoardService board)
        {
            if (IsBuiltFor(board) == false)
                return;

            for (int row = 0; row < _height; row++)
            {
                for (int column = 0; column < _width; column++)
                    RefreshCell(board, new CellAddress(column, row));
            }

            for (int row = 0; row < _height; row++)
                RefreshLine(board, LineAddress.Row(row));

            for (int column = 0; column < _width; column++)
                RefreshLine(board, LineAddress.Column(column));
        }

        public void Clear()
        {
            _builtBoard = null;
            _width = 0;
            _height = 0;
            _usedCells = 0;
            _usedClues = 0;
            _cellSize = 0f;
            _isRevealed = false;
            _cellViews = Array.Empty<NonogramCellView>();
            _rowClueViews = Array.Empty<List<NonogramClueView>>();
            _columnClueViews = Array.Empty<List<NonogramClueView>>();
            _columnOffsets = Array.Empty<float>();
            _rowOffsets = Array.Empty<float>();

            HideUnused();

            if (_input != null)
                _input.SetLayout(_columnOffsets, _rowOffsets, 0f);
        }

        private void OnRectTransformDimensionsChange()
        {
            if (_builtBoard == null || _isBuilding)
                return;

            INonogramBoardService board = _builtBoard;

            BuildInternal(board, resetReveal: false);
            RefreshAll(board);
        }

        private Color GetFillColor(INonogramBoardService board, CellAddress address, NonogramCell cell)
        {
            if (board.IsMonochrome == false)
                return board.GetColor(cell.ColorIndex);

            return _isRevealed ? board.GetRevealColor(address) : _skin.MonochromeInkColor;
        }

        private bool IsBuiltFor(INonogramBoardService board)
        {
            return board != null && ReferenceEquals(board, _builtBoard) && _width > 0 && _height > 0;
        }

        private bool IsConfigured()
        {
            if (_skin != null && _viewport != null && _content != null && _gridRoot != null
                && _rowCluesRoot != null && _columnCluesRoot != null
                && _cellPrefab != null && _cluePrefab != null && _input != null)
                return true;

            GameDebug.LogConfigurationError($"[Nonogram] На {name} не заполнены ссылки NonogramBoardView.");
            return false;
        }

        private int GetMaxClues(INonogramBoardService board, LineAxis axis, int count)
        {
            int max = 1;

            for (int index = 0; index < count; index++)
            {
                int length = board.GetClues(new LineAddress(axis, index)).Count;

                if (length > max)
                    max = length;
            }

            return max;
        }

        private float ResolveCellSize(int maxRowClues, int maxColumnClues)
        {
            Vector2 viewport = _viewport.rect.size;
            int horizontalGaps = (_width - 1) / _skin.BlockSize;
            int verticalGaps = (_height - 1) / _skin.BlockSize;

            float horizontal = (viewport.x - (_width - 1) * _skin.CellSpacing - horizontalGaps * _skin.BlockSpacing)
                / (maxRowClues + _width);
            float vertical = (viewport.y - (_height - 1) * _skin.CellSpacing - verticalGaps * _skin.BlockSpacing)
                / (maxColumnClues + _height);

            return Mathf.Clamp(Mathf.Min(horizontal, vertical), _skin.MinCellSize, _skin.MaxCellSize);
        }

        private float[] BuildOffsets(int count, float cellSize)
        {
            float[] offsets = new float[count];

            for (int i = 0; i < count; i++)
                offsets[i] = i * (cellSize + _skin.CellSpacing) + i / _skin.BlockSize * _skin.BlockSpacing;

            return offsets;
        }

        private void ApplyRoots(float bandWidth, float bandHeight, float gridWidth, float gridHeight)
        {
            float contentWidth = bandWidth + gridWidth;
            float contentHeight = bandHeight + gridHeight;

            _content.anchorMin = Center;
            _content.anchorMax = Center;
            _content.pivot = TopLeft;
            _content.sizeDelta = new Vector2(contentWidth, contentHeight);
            _content.anchoredPosition = new Vector2(-contentWidth * 0.5f, contentHeight * 0.5f);

            ApplyRoot(_gridRoot, new Vector2(bandWidth, -bandHeight), new Vector2(gridWidth, gridHeight));
            ApplyRoot(_rowCluesRoot, new Vector2(0f, -bandHeight), new Vector2(bandWidth, gridHeight));
            ApplyRoot(_columnCluesRoot, new Vector2(bandWidth, 0f), new Vector2(gridWidth, bandHeight));
        }

        private void ApplyRoot(RectTransform root, Vector2 position, Vector2 size)
        {
            root.anchorMin = TopLeft;
            root.anchorMax = TopLeft;
            root.pivot = TopLeft;
            root.sizeDelta = size;
            root.anchoredPosition = position;
        }

        private void BuildCells(INonogramBoardService board)
        {
            _cellViews = new NonogramCellView[_width * _height];

            for (int row = 0; row < _height; row++)
            {
                for (int column = 0; column < _width; column++)
                {
                    NonogramCellView view = TakeCell();
                    view.SetGeometry(new Vector2(_columnOffsets[column], -_rowOffsets[row]), _cellSize);

                    CellAddress address = new(column, row);
                    NonogramCell cell = board.GetCell(address);
                    view.Apply(_skin, cell, GetFillColor(board, address, cell));

                    _cellViews[row * _width + column] = view;
                }
            }
        }

        private void BuildClues(INonogramBoardService board, int maxRowClues, int maxColumnClues,
            float bandWidth, float bandHeight)
        {
            float fontSize = _cellSize * _skin.ClueFontRatio;
            bool isMonochrome = board.PaletteSize == 1;

            _rowClueViews = new List<NonogramClueView>[_height];
            _columnClueViews = new List<NonogramClueView>[_width];

            for (int row = 0; row < _height; row++)
            {
                LineAddress line = LineAddress.Row(row);
                IReadOnlyList<NonogramClue> clues = board.GetClues(line);
                List<NonogramClueView> views = new(maxRowClues);

                for (int i = 0; i < clues.Count; i++)
                {
                    NonogramClueView view = TakeClue(_rowCluesRoot);
                    float x = bandWidth - (clues.Count - i) * _cellSize;

                    view.SetGeometry(new Vector2(x, -_rowOffsets[row]), _cellSize, fontSize, _skin.ClueChipPadding);
                    view.Apply(_skin, clues[i], board.GetColor(clues[i].ColorIndex), isMonochrome,
                        board.IsLineSolved(line));

                    views.Add(view);
                }

                _rowClueViews[row] = views;
            }

            for (int column = 0; column < _width; column++)
            {
                LineAddress line = LineAddress.Column(column);
                IReadOnlyList<NonogramClue> clues = board.GetClues(line);
                List<NonogramClueView> views = new(maxColumnClues);

                for (int i = 0; i < clues.Count; i++)
                {
                    NonogramClueView view = TakeClue(_columnCluesRoot);
                    float y = -(bandHeight - (clues.Count - i) * _cellSize);

                    view.SetGeometry(new Vector2(_columnOffsets[column], y), _cellSize, fontSize, _skin.ClueChipPadding);
                    view.Apply(_skin, clues[i], board.GetColor(clues[i].ColorIndex), isMonochrome,
                        board.IsLineSolved(line));

                    views.Add(view);
                }

                _columnClueViews[column] = views;
            }
        }

        private NonogramCellView TakeCell()
        {
            if (_usedCells == _cellPool.Count)
                _cellPool.Add(Instantiate(_cellPrefab, _gridRoot));

            NonogramCellView view = _cellPool[_usedCells];
            _usedCells++;

            view.gameObject.SetActive(true);
            return view;
        }

        private NonogramClueView TakeClue(RectTransform root)
        {
            if (_usedClues == _cluePool.Count)
                _cluePool.Add(Instantiate(_cluePrefab, root));

            NonogramClueView view = _cluePool[_usedClues];
            _usedClues++;

            if (view.transform.parent != root)
                view.transform.SetParent(root, false);

            view.gameObject.SetActive(true);
            return view;
        }

        private void HideUnused()
        {
            for (int i = _usedCells; i < _cellPool.Count; i++)
                _cellPool[i].gameObject.SetActive(false);

            for (int i = _usedClues; i < _cluePool.Count; i++)
                _cluePool[i].gameObject.SetActive(false);
        }
    }
}
