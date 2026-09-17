using System;

namespace G.Gameplay.Nonogram.Data
{
    public readonly struct LineAddress : IEquatable<LineAddress>
    {
        public readonly LineAxis Axis;
        public readonly int Index;

        public LineAddress(LineAxis axis, int index)
        {
            Axis = axis;
            Index = index;
        }

        public static LineAddress Row(int index)
        {
            return new LineAddress(LineAxis.Row, index);
        }

        public static LineAddress Column(int index)
        {
            return new LineAddress(LineAxis.Column, index);
        }

        public bool Equals(LineAddress other)
        {
            return Axis == other.Axis && Index == other.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is LineAddress other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Axis, Index);
        }

        public override string ToString()
        {
            return $"{Axis} {Index}";
        }
    }
}
