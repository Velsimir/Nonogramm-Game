using System.Collections.Generic;

namespace G.Gameplay.Nonogram.Board
{
    public class NonogramBoardSaveData
    {
        public string LevelId;
        public int LivesLeft;
        public int HintsLeft;
        public List<byte> States = new();
        public List<byte> Colors = new();
    }
}
