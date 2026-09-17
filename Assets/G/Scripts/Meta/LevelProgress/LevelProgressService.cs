using System.Collections.Generic;
using MessagePipe;
using Newtonsoft.Json.Linq;
using Zenject;
using G.Core.Common;
using G.Core.Save;
using G.Gameplay.Nonogram.Board;
using G.Gameplay.Nonogram.Level;

namespace G.Meta.LevelProgress
{
    public class LevelProgressService : Service, ILevelProgressService, ISaveParticipant
    {
        private const string SAVE_KEY = "levelProgress";
        private const int SAVE_ORDER = 10;

        private readonly HashSet<string> _completed = new();

        private readonly NonogramLevelsConfig _levels;
        private readonly LazyInject<ISaveService> _saveService;
        private readonly ISubscriber<NonogramMessages.BoardSolved> _boardSolvedSubscriber;
        private readonly IPublisher<LevelProgressMessages.LevelCompleted> _levelCompletedPublisher;
        private readonly IPublisher<LevelProgressMessages.LevelUnlocked> _levelUnlockedPublisher;

        public LevelProgressService(
            NonogramLevelsConfig levels,
            LazyInject<ISaveService> saveService,
            ISubscriber<NonogramMessages.BoardSolved> boardSolvedSubscriber,
            IPublisher<LevelProgressMessages.LevelCompleted> levelCompletedPublisher,
            IPublisher<LevelProgressMessages.LevelUnlocked> levelUnlockedPublisher)
        {
            _levels = levels;
            _saveService = saveService;
            _boardSolvedSubscriber = boardSolvedSubscriber;
            _levelCompletedPublisher = levelCompletedPublisher;
            _levelUnlockedPublisher = levelUnlockedPublisher;
        }

        public string Key => SAVE_KEY;
        public int Order => SAVE_ORDER;

        public int CompletedCount => _completed.Count;

        protected override void OnInitialize()
        {
            Disposables.Add(_boardSolvedSubscriber.Subscribe(message => MarkCompleted(message.LevelId)));
        }

        public bool IsCompleted(string levelId)
        {
            return string.IsNullOrEmpty(levelId) == false && _completed.Contains(levelId);
        }

        public bool IsUnlocked(string levelId)
        {
            int index = IndexOf(levelId);

            if (index < 0)
            {
                GameDebug.LogError($"[LevelProgress] Уровень {levelId} отсутствует в NonogramLevelsConfig.");
                return false;
            }

            if (index == 0)
                return true;

            return IsCompleted(_levels.Levels[index - 1].Id);
        }

        public void MarkCompleted(string levelId)
        {
            if (string.IsNullOrEmpty(levelId))
                return;

            if (_completed.Add(levelId) == false)
                return;

            _levelCompletedPublisher.Publish(new LevelProgressMessages.LevelCompleted(levelId));
            PublishUnlockedNext(levelId);

            _saveService.Value.MarkDirty(SaveReason.Important);
        }

        public bool TryGetNextLevel(out NonogramLevelAsset level)
        {
            level = null;

            foreach (NonogramLevelAsset candidate in _levels.Levels)
            {
                if (candidate == null || IsCompleted(candidate.Id))
                    continue;

                level = candidate;
                return true;
            }

            return false;
        }

        public object Capture()
        {
            LevelProgressSaveData data = new();
            data.Completed.AddRange(_completed);

            return data;
        }

        public void Restore(JToken section)
        {
            LevelProgressSaveData data = SaveSection.Read<LevelProgressSaveData>(section, SAVE_KEY);

            _completed.Clear();

            foreach (string levelId in data.Completed)
            {
                if (string.IsNullOrEmpty(levelId) == false)
                    _completed.Add(levelId);
            }
        }

        private void PublishUnlockedNext(string levelId)
        {
            int index = IndexOf(levelId);

            if (index < 0 || index + 1 >= _levels.Levels.Count)
                return;

            NonogramLevelAsset next = _levels.Levels[index + 1];

            if (next == null)
                return;

            _levelUnlockedPublisher.Publish(new LevelProgressMessages.LevelUnlocked(next.Id));
        }

        private int IndexOf(string levelId)
        {
            for (int i = 0; i < _levels.Levels.Count; i++)
            {
                NonogramLevelAsset level = _levels.Levels[i];

                if (level != null && level.Id == levelId)
                    return i;
            }

            return -1;
        }
    }
}
