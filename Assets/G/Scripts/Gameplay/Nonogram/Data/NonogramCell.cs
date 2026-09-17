namespace G.Gameplay.Nonogram.Data
{
    public readonly struct NonogramCell
    {
        public static readonly NonogramCell Empty = new(CellState.Empty, 0);

        public readonly CellState State;
        public readonly byte ColorIndex;

        public NonogramCell(CellState state, byte colorIndex)
        {
            State = state;
            ColorIndex = colorIndex;
        }

        public bool IsEmpty => State == CellState.Empty;
        public bool IsFilled => State == CellState.Filled;
        public bool IsMarked => State == CellState.Marked;

        public static NonogramCell Filled(byte colorIndex)
        {
            return new NonogramCell(CellState.Filled, colorIndex);
        }

        public static NonogramCell Marked()
        {
            return new NonogramCell(CellState.Marked, 0);
        }
    }
}
