using System;
using System.Collections.Generic;
using MessagePipe;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Zenject;
using G.Core.Common;
using G.Core.Save;
using G.Gameplay.Nonogram.Config;
using G.Gameplay.Nonogram.Data;
using G.Gameplay.Nonogram.Level;

namespace G.Gameplay.Nonogram.Board
{
    public class NonogramBoardService : Service, INonogramBoardService, ISaveParticipant
    {
        private const string SAVE_KEY = "nonogram";
        private const int SAVE_ORDER = 20;

        private readonly List<NonogramClue> _clueBuffer = new();
        private readonly List<byte> _lineBuffer = new();
        private readonly List<CellAddress> _hintCandidates = new();

        private readonly NonogramRuleConfig _rule;
        private readonly LazyInject<ISaveService> _saveService;
        private readonly IPublisher<NonogramMessages.BoardLoaded> _boardLoadedPublisher;
        private readonly IPublisher<NonogramMessages.BoardCleared> _boardClearedPublisher;
        private readonly IPublisher<NonogramMessages.CellChanged> _cellChangedPublisher;
        private readonly IPublisher<NonogramMessages.LineSolved> _lineSolvedPublisher;
        private readonly IPublisher<NonogramMessages.MistakeMade> _mistakeMadePublisher;
        private readonly IPublisher<NonogramMessages.HintUsed> _hintUsedPublisher;
        private readonly IPublisher<NonogramMessages.BoardSolved> _boardSolvedPublisher;
        private readonly IPublisher<NonogramMessages.BoardFailed> _boardFailedPublisher;
        private readonly IPublisher<NonogramMessages.BoardRevived> _boardRevivedPublisher;
        private readonly IPublisher<NonogramMessages.BatchFinished> _batchFinishedPublisher;
        private readonly BatchScope _batchScope;

        private NonogramLevelAsset _level;
        private byte[] _solution;
        private NonogramCell[] _cells;
        private NonogramClue[][] _rowClues;
        private NonogramClue[][] _columnClues;
        private LineStatus[] _rows;
        private LineStatus[] _columns;
        private NonogramBoardSaveData _pendingSave;

        private int _width;
        private int _height;
        private int _targetFilled;
        private int _correctFilled;
        private int _wrongFilled;
        private int _livesLeft;
        private int _hintsLeft;
        private int _batchDepth;
        private bool _batchChanged;
        private bool _isSolved;
        private bool _isFailed;
        private bool _isMonochrome;

        public NonogramBoardService(
            NonogramRuleConfig rule,
            LazyInject<ISaveService> saveService,
            IPublisher<NonogramMessages.BoardLoaded> boardLoadedPublisher,
            IPublisher<NonogramMessages.BoardCleared> boardClearedPublisher,
            IPublisher<NonogramMessages.CellChanged> cellChangedPublisher,
            IPublisher<NonogramMessages.LineSolved> lineSolvedPublisher,
            IPublisher<NonogramMessages.MistakeMade> mistakeMadePublisher,
            IPublisher<NonogramMessages.HintUsed> hintUsedPublisher,
            IPublisher<NonogramMessages.BoardSolved> boardSolvedPublisher,
            IPublisher<NonogramMessages.BoardFailed> boardFailedPublisher,
            IPublisher<NonogramMessages.BoardRevived> boardRevivedPublisher,
            IPublisher<NonogramMessages.BatchFinished> batchFinishedPublisher)
        {
            _rule = rule;
            _saveService = saveService;
            _boardLoadedPublisher = boardLoadedPublisher;
            _boardClearedPublisher = boardClearedPublisher;
            _cellChangedPublisher = cellChangedPublisher;
            _lineSolvedPublisher = lineSolvedPublisher;
            _mistakeMadePublisher = mistakeMadePublisher;
            _hintUsedPublisher = hintUsedPublisher;
            _boardSolvedPublisher = boardSolvedPublisher;
            _boardFailedPublisher = boardFailedPublisher;
            _boardRevivedPublisher = boardRevivedPublisher;
            _batchFinishedPublisher = batchFinishedPublisher;
            _batchScope = new BatchScope(this);
        }

        public string Key => SAVE_KEY;
        public int Order => SAVE_ORDER;

        public bool HasBoard => _level != null;
        public string LevelId => _level != null ? _level.Id : string.Empty;
        public int Width => _width;
        public int Height => _height;
        public int PaletteSize => GetPaletteSize();
        public int LivesLeft => _livesLeft;
        public int HintsLeft => _hintsLeft;
        public int FilledLeft => _targetFilled - _correctFilled;
        public bool IsSolved => _isSolved;
        public bool IsFailed => _isFailed;
        public bool IsBatchRunning => _batchDepth > 0;
        public bool IsMonochrome => _isMonochrome;
        public MistakeMode MistakeMode => _rule.MistakeMode;

        public void Load(NonogramLevelAsset level)
        {
            if (level == null)
            {
                GameDebug.LogConfigurationError("[Nonogram] Попытка загрузить пустой уровень.");
                return;
            }

            if (level.IsValid(out string problem) == false)
            {
                GameDebug.LogConfigurationError($"[Nonogram] Уровень {level.name} нельзя загрузить: {problem}");
                return;
            }

            _level = level;
            _isMonochrome = level.PlayMonochrome;
            _width = level.Width;
            _height = level.Height;
            _solution = BuildWorkingSolution(level);
            _cells = new NonogramCell[_width * _height];
            _rowClues = NonogramClueBuilder.BuildRows(_solution, _width, _height);
            _columnClues = NonogramClueBuilder.BuildColumns(_solution, _width, _height);
            _rows = new LineStatus[_height];
            _columns = new LineStatus[_width];
            _livesLeft = _rule.Lives;
            _hintsLeft = _rule.Hints;
            _isSolved = false;
            _isFailed = false;

            BuildLineTargets();
            ApplyPendingSave();
            RecalculateStatuses();

            _boardLoadedPublisher.Publish(new NonogramMessages.BoardLoaded(LevelId));
        }

        public void Clear()
        {
            if (HasBoard == false)
                return;

            _level = null;
            _solution = null;
            _cells = null;
            _rowClues = null;
            _columnClues = null;
            _rows = null;
            _columns = null;
            _width = 0;
            _height = 0;
            _targetFilled = 0;
            _correctFilled = 0;
            _wrongFilled = 0;
            _isSolved = false;
            _isFailed = false;

            _boardClearedPublisher.Publish(new NonogramMessages.BoardCleared());
        }

        public bool IsInside(CellAddress address)
        {
            return address.Column >= 0 && address.Column < _width
                && address.Row >= 0 && address.Row < _height;
        }

        public NonogramCell GetCell(CellAddress address)
        {
            if (HasBoard == false || IsInside(address) == false)
                return NonogramCell.Empty;

            return _cells[ToIndex(address)];
        }

        public byte GetSolutionColor(CellAddress address)
        {
            if (HasBoard == false || IsInside(address) == false)
                return 0;

            return _solution[ToIndex(address)];
        }

        public Color GetRevealColor(CellAddress address)
        {
            if (_level == null || IsInside(address) == false)
                return Color.clear;

            return _level.GetColor(_level.GetSolution(address.Column, address.Row));
        }

        public Color GetColor(byte colorIndex)
        {
            return _level != null ? _level.GetColor(colorIndex) : Color.clear;
        }

        public IReadOnlyList<NonogramClue> GetClues(LineAddress line)
        {
            if (HasBoard == false)
                return Array.Empty<NonogramClue>();

            NonogramClue[][] source = line.Axis == LineAxis.Row ? _rowClues : _columnClues;

            if (line.Index < 0 || line.Index >= source.Length)
                return Array.Empty<NonogramClue>();

            return source[line.Index];
        }

        public bool IsLineSolved(LineAddress line)
        {
            if (HasBoard == false || IsLineInside(line) == false)
                return false;

            return GetLineStatus(line).Solved;
        }

        public PaintOutcome Paint(CellAddress address, PaintTool tool, byte colorIndex)
        {
            if (HasBoard == false)
            {
                GameDebug.LogWarning("[Nonogram] Закраска без загруженного уровня.");
                return PaintOutcome.Rejected;
            }

            if (IsInside(address) == false || _isSolved || _isFailed)
                return PaintOutcome.Rejected;

            using (BeginBatch())
            {
                PaintOutcome outcome = tool switch
                {
                    PaintTool.Fill => Fill(address, colorIndex),

                    PaintTool.Mark => Mark(address),

                    PaintTool.Erase => Erase(address),

                    _ => PaintOutcome.Rejected,
                };

                if (outcome == PaintOutcome.Rejected)
                    return outcome;

                EvaluateLines(address);
                EvaluateBoard();

                return outcome;
            }
        }

        public bool TryHint(out CellAddress address)
        {
            address = default;

            if (HasBoard == false || _isSolved || _isFailed || _hintsLeft <= 0)
                return false;

            CollectHintCandidates();

            if (_hintCandidates.Count == 0)
                return false;

            address = _hintCandidates[UnityEngine.Random.Range(0, _hintCandidates.Count)];

            using (BeginBatch())
            {
                WriteCell(address, NonogramCell.Filled(GetSolutionColor(address)));
                _hintsLeft--;
                _hintUsedPublisher.Publish(new NonogramMessages.HintUsed(address, _hintsLeft));

                EvaluateLines(address);
                EvaluateBoard();
            }

            return true;
        }

        public void Revive()
        {
            if (HasBoard == false || _isFailed == false)
                return;

            _isFailed = false;
            _livesLeft = _rule.Lives;

            _boardRevivedPublisher.Publish(new NonogramMessages.BoardRevived(LevelId));
            _saveService.Value.MarkDirty(SaveReason.Important);
        }

        public IDisposable BeginBatch()
        {
            _batchDepth++;
            return _batchScope;
        }

        public object Capture()
        {
            if (HasBoard == false)
                return _pendingSave;

            NonogramBoardSaveData data = new()
            {
                LevelId = LevelId,
                LivesLeft = _livesLeft,
                HintsLeft = _hintsLeft,
            };

            data.States.Capacity = _cells.Length;
            data.Colors.Capacity = _cells.Length;

            foreach (NonogramCell cell in _cells)
            {
                data.States.Add((byte)cell.State);
                data.Colors.Add(cell.ColorIndex);
            }

            return data;
        }

        public void Restore(JToken section)
        {
            _pendingSave = SaveSection.Read<NonogramBoardSaveData>(section, SAVE_KEY);

            if (HasBoard == false)
                return;

            ApplyPendingSave();
            RecalculateStatuses();
        }

        private PaintOutcome Fill(CellAddress address, byte colorIndex)
        {
            if (colorIndex == 0 || colorIndex > PaletteSize)
                return PaintOutcome.Rejected;

            NonogramCell cell = GetCell(address);

            if (_rule.MistakeMode == MistakeMode.Free)
            {
                if (cell.IsFilled && cell.ColorIndex == colorIndex)
                    return PaintOutcome.Rejected;

                WriteCell(address, NonogramCell.Filled(colorIndex));
                return PaintOutcome.Filled;
            }

            if (cell.IsEmpty == false)
                return PaintOutcome.Rejected;

            byte expected = GetSolutionColor(address);

            if (expected == colorIndex)
            {
                WriteCell(address, NonogramCell.Filled(colorIndex));
                return PaintOutcome.Filled;
            }

            RegisterMistake(address, expected);
            return PaintOutcome.Mistake;
        }

        private PaintOutcome Mark(CellAddress address)
        {
            if (_rule.AllowMarks == false)
                return PaintOutcome.Rejected;

            if (GetCell(address).IsEmpty == false)
                return PaintOutcome.Rejected;

            WriteCell(address, NonogramCell.Marked());
            return PaintOutcome.Marked;
        }

        private PaintOutcome Erase(CellAddress address)
        {
            NonogramCell cell = GetCell(address);

            if (cell.IsEmpty)
                return PaintOutcome.Rejected;

            if (_rule.MistakeMode == MistakeMode.Validated && cell.IsFilled)
                return PaintOutcome.Rejected;

            WriteCell(address, NonogramCell.Empty);
            return PaintOutcome.Erased;
        }

        private void RegisterMistake(CellAddress address, byte expected)
        {
            NonogramCell corrected = expected == 0
                ? NonogramCell.Marked()
                : NonogramCell.Filled(expected);

            WriteCell(address, corrected);

            _livesLeft--;
            _mistakeMadePublisher.Publish(new NonogramMessages.MistakeMade(address, _livesLeft));

            if (_livesLeft > 0)
                return;

            _isFailed = true;
            _boardFailedPublisher.Publish(new NonogramMessages.BoardFailed(LevelId));
        }

        private void WriteCell(CellAddress address, NonogramCell cell)
        {
            int index = ToIndex(address);

            ApplyCounters(address, _cells[index], -1);
            _cells[index] = cell;
            ApplyCounters(address, cell, 1);

            _batchChanged = true;
            _cellChangedPublisher.Publish(new NonogramMessages.CellChanged(address));
        }

        private void ApplyCounters(CellAddress address, NonogramCell cell, int sign)
        {
            if (cell.IsFilled == false)
                return;

            byte expected = GetSolutionColor(address);

            if (expected != 0 && cell.ColorIndex == expected)
            {
                _correctFilled += sign;
                _rows[address.Row].Correct += sign;
                _columns[address.Column].Correct += sign;
                return;
            }

            _wrongFilled += sign;
            _rows[address.Row].Wrong += sign;
            _columns[address.Column].Wrong += sign;
        }

        private void EvaluateLines(CellAddress address)
        {
            EvaluateLine(LineAddress.Row(address.Row));
            EvaluateLine(LineAddress.Column(address.Column));
        }

        private void EvaluateLine(LineAddress line)
        {
            ref LineStatus status = ref GetLineStatus(line);

            if (status.AutoMarked == false && status.Correct == status.Target && status.Wrong == 0)
            {
                status.AutoMarked = true;

                if (_rule.AutoMarkCompletedLines && _rule.AllowMarks)
                    AutoMarkLine(line);
            }

            bool solved = EvaluateLineSolved(line, status);

            if (solved == status.Solved)
                return;

            status.Solved = solved;

            if (solved)
                _lineSolvedPublisher.Publish(new NonogramMessages.LineSolved(line));
        }

        private bool EvaluateLineSolved(LineAddress line, in LineStatus status)
        {
            if (_rule.MistakeMode == MistakeMode.Validated)
                return status.Correct == status.Target;

            FillLineColors(line, _lineBuffer);
            NonogramClueBuilder.BuildLine(_lineBuffer, _clueBuffer);

            return NonogramClueBuilder.AreEqual(GetClues(line), _clueBuffer);
        }

        private void AutoMarkLine(LineAddress line)
        {
            int length = GetLineLength(line);

            for (int position = 0; position < length; position++)
            {
                CellAddress address = GetLineCell(line, position);

                if (GetCell(address).IsEmpty == false)
                    continue;

                WriteCell(address, NonogramCell.Marked());
            }
        }

        private void EvaluateBoard()
        {
            if (_isSolved || _isFailed)
                return;

            if (_correctFilled != _targetFilled || _wrongFilled != 0)
                return;

            _isSolved = true;
            _boardSolvedPublisher.Publish(new NonogramMessages.BoardSolved(LevelId));
        }

        private void CollectHintCandidates()
        {
            _hintCandidates.Clear();

            for (int row = 0; row < _height; row++)
            {
                for (int column = 0; column < _width; column++)
                {
                    CellAddress address = new(column, row);
                    byte expected = GetSolutionColor(address);

                    if (expected == 0)
                        continue;

                    NonogramCell cell = GetCell(address);

                    if (cell.IsFilled && cell.ColorIndex == expected)
                        continue;

                    _hintCandidates.Add(address);
                }
            }
        }

        private void BuildLineTargets()
        {
            _targetFilled = 0;

            for (int row = 0; row < _height; row++)
            {
                for (int column = 0; column < _width; column++)
                {
                    if (_solution[row * _width + column] == 0)
                        continue;

                    _targetFilled++;
                    _rows[row].Target++;
                    _columns[column].Target++;
                }
            }
        }

        private byte[] BuildWorkingSolution(NonogramLevelAsset level)
        {
            byte[] solution = new byte[level.Width * level.Height];

            for (int i = 0; i < solution.Length; i++)
            {
                byte color = level.Solution[i];

                if (color == 0)
                    continue;

                solution[i] = level.PlayMonochrome ? (byte)1 : color;
            }

            return solution;
        }

        private int GetPaletteSize()
        {
            if (_level == null)
                return 0;

            return _isMonochrome ? 1 : _level.PaletteSize;
        }

        private void ApplyPendingSave()
        {
            if (_pendingSave == null || _pendingSave.LevelId != LevelId)
                return;

            if (_pendingSave.States.Count != _cells.Length || _pendingSave.Colors.Count != _cells.Length)
            {
                GameDebug.LogWarning(
                    $"[Nonogram] Сейв уровня {LevelId} не совпадает с размером доски, доска начата заново.");
                _pendingSave = null;
                return;
            }

            for (int i = 0; i < _cells.Length; i++)
                _cells[i] = new NonogramCell((CellState)_pendingSave.States[i], _pendingSave.Colors[i]);

            _livesLeft = _pendingSave.LivesLeft;
            _hintsLeft = _pendingSave.HintsLeft;
        }

        private void RecalculateStatuses()
        {
            _correctFilled = 0;
            _wrongFilled = 0;

            for (int i = 0; i < _rows.Length; i++)
                ResetLineCounters(ref _rows[i]);

            for (int i = 0; i < _columns.Length; i++)
                ResetLineCounters(ref _columns[i]);

            for (int row = 0; row < _height; row++)
            {
                for (int column = 0; column < _width; column++)
                {
                    CellAddress address = new(column, row);
                    ApplyCounters(address, _cells[ToIndex(address)], 1);
                }
            }

            for (int row = 0; row < _height; row++)
                RefreshLineStatus(LineAddress.Row(row));

            for (int column = 0; column < _width; column++)
                RefreshLineStatus(LineAddress.Column(column));

            _isSolved = _correctFilled == _targetFilled && _wrongFilled == 0;
            _isFailed = _isSolved == false && _livesLeft <= 0;
        }

        private void ResetLineCounters(ref LineStatus status)
        {
            status.Correct = 0;
            status.Wrong = 0;
            status.Solved = false;
            status.AutoMarked = false;
        }

        private void RefreshLineStatus(LineAddress line)
        {
            ref LineStatus status = ref GetLineStatus(line);

            status.AutoMarked = status.Correct == status.Target && status.Wrong == 0;
            status.Solved = EvaluateLineSolved(line, status);
        }

        private void FillLineColors(LineAddress line, List<byte> buffer)
        {
            buffer.Clear();

            int length = GetLineLength(line);

            for (int position = 0; position < length; position++)
            {
                NonogramCell cell = GetCell(GetLineCell(line, position));
                buffer.Add(cell.IsFilled ? cell.ColorIndex : (byte)0);
            }
        }

        private CellAddress GetLineCell(LineAddress line, int position)
        {
            return line.Axis == LineAxis.Row
                ? new CellAddress(position, line.Index)
                : new CellAddress(line.Index, position);
        }

        private int GetLineLength(LineAddress line)
        {
            return line.Axis == LineAxis.Row ? _width : _height;
        }

        private bool IsLineInside(LineAddress line)
        {
            int count = line.Axis == LineAxis.Row ? _height : _width;
            return line.Index >= 0 && line.Index < count;
        }

        private ref LineStatus GetLineStatus(LineAddress line)
        {
            if (line.Axis == LineAxis.Row)
                return ref _rows[line.Index];

            return ref _columns[line.Index];
        }

        private int ToIndex(CellAddress address)
        {
            return address.Row * _width + address.Column;
        }

        private void EndBatch()
        {
            _batchDepth--;

            if (_batchDepth > 0)
                return;

            _batchDepth = 0;

            if (_batchChanged == false)
                return;

            _batchChanged = false;
            _batchFinishedPublisher.Publish(new NonogramMessages.BatchFinished());
            _saveService.Value.MarkDirty(SaveReason.Important);
        }

        private struct LineStatus
        {
            public int Target;
            public int Correct;
            public int Wrong;
            public bool Solved;
            public bool AutoMarked;
        }

        private class BatchScope : IDisposable
        {
            private readonly NonogramBoardService _service;

            public BatchScope(NonogramBoardService service)
            {
                _service = service;
            }

            public void Dispose()
            {
                _service.EndBatch();
            }
        }
    }
}
