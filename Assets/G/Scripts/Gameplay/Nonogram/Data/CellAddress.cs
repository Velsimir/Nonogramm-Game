using System;

namespace G.Gameplay.Nonogram.Data
{
    public readonly struct CellAddress : IEquatable<CellAddress>
    {
        public readonly int Column;
        public readonly int Row;

        public CellAddress(int column, int row)
        {
            Column = column;
            Row = row;
        }

        public bool Equals(CellAddress other)
        {
            return Column == other.Column && Row == other.Row;
        }

        public override bool Equals(object obj)
        {
            return obj is CellAddress other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Column, Row);
        }

        public override string ToString()
        {
            return $"[{Column}, {Row}]";
        }
    }
}
