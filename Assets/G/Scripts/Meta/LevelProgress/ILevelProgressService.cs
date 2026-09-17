using G.Gameplay.Nonogram.Level;

namespace G.Meta.LevelProgress
{
    public interface ILevelProgressService
    {
        int CompletedCount { get; }

        bool IsCompleted(string levelId);

        bool IsUnlocked(string levelId);

        void MarkCompleted(string levelId);

        bool TryGetNextLevel(out NonogramLevelAsset level);
    }
}
