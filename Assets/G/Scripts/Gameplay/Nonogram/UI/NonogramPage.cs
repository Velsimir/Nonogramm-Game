using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using Zenject;
using G.Core.Common;
using G.Core.UI;
using G.Gameplay.Nonogram.Board;
using G.Gameplay.Nonogram.Level;
using G.Gameplay.Nonogram.Presentation;
using G.Meta.LevelFlow;

namespace G.Gameplay.Nonogram.UI
{
    public class NonogramPage : PayloadPage<NonogramLevelAsset>
    {
        [SerializeField] private NonogramBoardView _boardView;
        [SerializeField] private NonogramHudView _hudView;

        private INonogramBoardService _board;
        private NonogramPresenter _presenter;
        private ILevelFlowService _flow;

        [Inject]
        private void Construct(INonogramBoardService board, NonogramPresenter presenter, ILevelFlowService flow)
        {
            _board = board;
            _presenter = presenter;
            _flow = flow;
        }

        protected override void OnShow(NonogramLevelAsset payload)
        {
            if (_boardView == null || _hudView == null)
            {
                GameDebug.LogConfigurationError("[Nonogram] На NonogramPage не назначены доска или HUD.");
                return;
            }

            _presenter.Attach(_boardView, _hudView);
            HiddenDisposables.Add(_hudView.BackClicked.Subscribe(_ => OnBackClicked()));

            _board.Load(payload);
        }

        protected override void OnHidden()
        {
            _presenter.Detach();
        }

        private void OnBackClicked()
        {
            _flow.ReturnToMenuAsync().Forget();
        }
    }
}
