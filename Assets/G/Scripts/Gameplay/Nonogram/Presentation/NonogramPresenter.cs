using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using R3;
using G.Core.Common;
using G.Gameplay.Nonogram.Board;
using G.Gameplay.Nonogram.Config;
using G.Gameplay.Nonogram.Data;

namespace G.Gameplay.Nonogram.Presentation
{
    public class NonogramPresenter : Presenter
    {
        private const byte DEFAULT_COLOR_INDEX = 1;

        private readonly CompositeDisposable _attachDisposables = new();

        private readonly INonogramBoardService _board;
        private readonly NonogramRuleConfig _rule;
        private readonly ISubscriber<NonogramMessages.BoardLoaded> _boardLoaded;
        private readonly ISubscriber<NonogramMessages.BoardCleared> _boardCleared;
        private readonly ISubscriber<NonogramMessages.CellChanged> _cellChanged;
        private readonly ISubscriber<NonogramMessages.LineSolved> _lineSolved;
        private readonly ISubscriber<NonogramMessages.MistakeMade> _mistakeMade;
        private readonly ISubscriber<NonogramMessages.BoardSolved> _boardSolved;
        private readonly ISubscriber<NonogramMessages.BoardRevived> _boardRevived;

        private NonogramBoardView _view;
        private NonogramHudView _hud;
        private IDisposable _strokeBatch;
        private PaintTool _tool = PaintTool.Fill;
        private PaintTool _strokeTool = PaintTool.Fill;
        private byte _colorIndex = DEFAULT_COLOR_INDEX;

        public NonogramPresenter(
            INonogramBoardService board,
            NonogramRuleConfig rule,
            ISubscriber<NonogramMessages.BoardLoaded> boardLoaded,
            ISubscriber<NonogramMessages.BoardCleared> boardCleared,
            ISubscriber<NonogramMessages.CellChanged> cellChanged,
            ISubscriber<NonogramMessages.LineSolved> lineSolved,
            ISubscriber<NonogramMessages.MistakeMade> mistakeMade,
            ISubscriber<NonogramMessages.BoardSolved> boardSolved,
            ISubscriber<NonogramMessages.BoardRevived> boardRevived)
        {
            _board = board;
            _rule = rule;
            _boardLoaded = boardLoaded;
            _boardCleared = boardCleared;
            _cellChanged = cellChanged;
            _lineSolved = lineSolved;
            _mistakeMade = mistakeMade;
            _boardSolved = boardSolved;
            _boardRevived = boardRevived;
        }

        public PaintTool Tool => _tool;
        public byte ColorIndex => _colorIndex;

        public void Attach(NonogramBoardView view, NonogramHudView hud)
        {
            if (view == null || hud == null)
            {
                GameDebug.LogConfigurationError("[Nonogram] Презентеру передали пустую доску или HUD.");
                return;
            }

            Detach();

            _view = view;
            _hud = hud;

            _attachDisposables.Add(_boardLoaded.Subscribe(OnBoardLoaded));
            _attachDisposables.Add(_boardCleared.Subscribe(OnBoardCleared));
            _attachDisposables.Add(_cellChanged.Subscribe(OnCellChanged));
            _attachDisposables.Add(_lineSolved.Subscribe(OnLineSolved));
            _attachDisposables.Add(_mistakeMade.Subscribe(OnMistakeMade));
            _attachDisposables.Add(_boardSolved.Subscribe(OnBoardSolved));
            _attachDisposables.Add(_boardRevived.Subscribe(OnBoardRevived));

            _attachDisposables.Add(view.Input.StrokeStarted.Subscribe(OnStrokeStarted));
            _attachDisposables.Add(view.Input.StrokeMoved.Subscribe(OnStrokeMoved));
            _attachDisposables.Add(view.Input.StrokeFinished.Subscribe(OnStrokeFinished));

            _attachDisposables.Add(hud.ToolSelected.Subscribe(SetTool));
            _attachDisposables.Add(hud.ColorSelected.Subscribe(SetColor));

            ApplyToolAndColor();

            if (_board.HasBoard == false)
                return;

            _view.Build(_board);
            _view.RefreshAll(_board);
            ApplyBoardState();
        }

        public void Detach()
        {
            FinishStroke();

            _attachDisposables.Clear();
            _view = null;
            _hud = null;
        }

        public void SetTool(PaintTool tool)
        {
            _tool = tool;

            if (_hud != null)
                _hud.SetTool(_tool);
        }

        public void SetColor(byte colorIndex)
        {
            if (colorIndex == 0)
            {
                GameDebug.LogWarning("[Nonogram] Индекс 0 — пустая клетка, выбрать его как цвет кисти нельзя.");
                return;
            }

            _colorIndex = colorIndex;

            if (_hud != null)
                _hud.SetColor(_colorIndex);
        }

        protected override void OnDispose()
        {
            FinishStroke();
            _attachDisposables.Dispose();
        }

        private void OnBoardLoaded(NonogramMessages.BoardLoaded message)
        {
            _colorIndex = DEFAULT_COLOR_INDEX;

            if (_view == null)
                return;

            _view.Build(_board);
            _view.RefreshAll(_board);
            ApplyBoardState();
        }

        private void OnBoardCleared(NonogramMessages.BoardCleared message)
        {
            FinishStroke();

            if (_view == null)
                return;

            _view.Clear();
        }

        private void OnCellChanged(NonogramMessages.CellChanged message)
        {
            if (_view == null)
                return;

            _view.RefreshCell(_board, message.Address);
        }

        private void OnLineSolved(NonogramMessages.LineSolved message)
        {
            if (_view == null)
                return;

            _view.RefreshLine(_board, message.Line);
        }

        private void OnMistakeMade(NonogramMessages.MistakeMade message)
        {
            if (_hud == null)
                return;

            _hud.SetLives(message.LivesLeft, _rule.Lives);
        }

        private void OnBoardRevived(NonogramMessages.BoardRevived message)
        {
            if (_hud == null)
                return;

            _hud.SetLives(_board.LivesLeft, _rule.Lives);
        }

        private void OnBoardSolved(NonogramMessages.BoardSolved message)
        {
            if (_view == null)
                return;

            _view.PlayRevealAsync(_board, DisposeToken).Forget();
        }

        private void ApplyBoardState()
        {
            if (_hud == null)
                return;

            _hud.SetLives(_board.LivesLeft, _rule.Lives);
            _hud.BuildPalette(_board);
            _hud.SetColor(_colorIndex);
        }

        private void ApplyToolAndColor()
        {
            if (_hud == null)
                return;

            _hud.SetTool(_tool);
            _hud.SetColor(_colorIndex);
        }

        private void OnStrokeStarted(CellAddress address)
        {
            if (_board.HasBoard == false)
                return;

            FinishStroke();

            _strokeTool = ResolveStrokeTool(address);
            _strokeBatch = _board.BeginBatch();

            _board.Paint(address, _strokeTool, _colorIndex);
        }

        private void OnStrokeMoved(CellAddress address)
        {
            if (_strokeBatch == null)
                return;

            _board.Paint(address, _strokeTool, _colorIndex);
        }

        private void OnStrokeFinished(Unit unit)
        {
            FinishStroke();
        }

        private PaintTool ResolveStrokeTool(CellAddress address)
        {
            NonogramCell cell = _board.GetCell(address);

            if (_tool == PaintTool.Fill && cell.IsFilled && cell.ColorIndex == _colorIndex)
                return PaintTool.Erase;

            if (_tool == PaintTool.Mark && cell.IsMarked)
                return PaintTool.Erase;

            return _tool;
        }

        private void FinishStroke()
        {
            if (_strokeBatch == null)
                return;

            _strokeBatch.Dispose();
            _strokeBatch = null;
        }
    }
}
