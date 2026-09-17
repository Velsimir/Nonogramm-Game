using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using R3;
using UnityEngine;
using Zenject;
using G.Core.Common;
using G.Core.UI;
using G.Gameplay.Nonogram.Level;
using G.Meta.LevelFlow;
using G.Meta.LevelProgress;

namespace G.Meta.LevelSelect
{
    public class LevelSelectPage : Page
    {
        [SerializeField] private RectTransform _content;
        [SerializeField] private LevelTileView _tilePrefab;

        private readonly List<LevelTileView> _tiles = new();

        private NonogramLevelsConfig _levels;
        private ILevelProgressService _progress;
        private ILevelFlowService _flow;
        private LevelPreviewFactory _previews;
        private ISubscriber<LevelProgressMessages.LevelCompleted> _levelCompletedSubscriber;
        private ISubscriber<LevelProgressMessages.LevelUnlocked> _levelUnlockedSubscriber;

        [Inject]
        private void Construct(
            NonogramLevelsConfig levels,
            ILevelProgressService progress,
            ILevelFlowService flow,
            LevelPreviewFactory previews,
            ISubscriber<LevelProgressMessages.LevelCompleted> levelCompletedSubscriber,
            ISubscriber<LevelProgressMessages.LevelUnlocked> levelUnlockedSubscriber)
        {
            _levels = levels;
            _progress = progress;
            _flow = flow;
            _previews = previews;
            _levelCompletedSubscriber = levelCompletedSubscriber;
            _levelUnlockedSubscriber = levelUnlockedSubscriber;
        }

        protected override void OnShow()
        {
            if (_content == null || _tilePrefab == null)
            {
                GameDebug.LogConfigurationError("[LevelSelect] На LevelSelectPage не назначен контент или префаб плитки.");
                return;
            }

            EnsureTiles(_levels.Levels.Count);
            SubscribeTiles();
            Rebuild();

            HiddenDisposables.Add(_levelCompletedSubscriber.Subscribe(_ => Rebuild()));
            HiddenDisposables.Add(_levelUnlockedSubscriber.Subscribe(_ => Rebuild()));
        }

        private void Rebuild()
        {
            for (int i = 0; i < _tiles.Count; i++)
            {
                LevelTileView tile = _tiles[i];

                if (i >= _levels.Levels.Count)
                {
                    tile.gameObject.SetActive(false);
                    continue;
                }

                NonogramLevelAsset level = _levels.Levels[i];

                if (level == null)
                {
                    tile.gameObject.SetActive(false);
                    continue;
                }

                bool isCompleted = _progress.IsCompleted(level.Id);
                bool isUnlocked = _progress.IsUnlocked(level.Id);

                tile.gameObject.SetActive(true);
                tile.Apply(i + 1, level, isUnlocked, isCompleted,
                    isCompleted ? _previews.GetPreview(level) : null);
            }
        }

        private void EnsureTiles(int count)
        {
            while (_tiles.Count < count)
                _tiles.Add(Instantiate(_tilePrefab, _content));
        }

        private void SubscribeTiles()
        {
            for (int i = 0; i < _tiles.Count; i++)
            {
                int index = i;
                HiddenDisposables.Add(_tiles[i].Clicked.Subscribe(_ => OnTileClicked(index)));
            }
        }

        private void OnTileClicked(int index)
        {
            if (_levels.TryGetByIndex(index, out NonogramLevelAsset level) == false)
                return;

            if (_progress.IsUnlocked(level.Id) == false)
                return;

            _flow.StartLevelAsync(level).Forget();
        }
    }
}
