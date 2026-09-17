using System;
using System.Collections.Generic;
using UnityEngine;
using G.Gameplay.Nonogram.Data;
using G.Gameplay.Nonogram.Level;

namespace G.Gameplay.Nonogram.Board
{
    public interface INonogramBoardService
    {
        bool HasBoard { get; }
        string LevelId { get; }
        int Width { get; }
        int Height { get; }
        int PaletteSize { get; }
        int LivesLeft { get; }
        int HintsLeft { get; }
        int FilledLeft { get; }
        bool IsSolved { get; }
        bool IsFailed { get; }
        bool IsBatchRunning { get; }
        bool IsMonochrome { get; }
        MistakeMode MistakeMode { get; }

        void Load(NonogramLevelAsset level);

        void Clear();

        bool IsInside(CellAddress address);

        NonogramCell GetCell(CellAddress address);

        byte GetSolutionColor(CellAddress address);

        Color GetColor(byte colorIndex);

        Color GetRevealColor(CellAddress address);

        IReadOnlyList<NonogramClue> GetClues(LineAddress line);

        bool IsLineSolved(LineAddress line);

        PaintOutcome Paint(CellAddress address, PaintTool tool, byte colorIndex);

        bool TryHint(out CellAddress address);

        void Revive();

        IDisposable BeginBatch();
    }
}
