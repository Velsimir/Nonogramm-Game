using System;

namespace G.Gameplay.Nonogram.Data
{
    public readonly struct NonogramClue : IEquatable<NonogramClue>
    {
        public readonly int Length;
        public readonly byte ColorIndex;

        public NonogramClue(int length, byte colorIndex)
        {
            Length = length;
            ColorIndex = colorIndex;
        }

        public bool Equals(NonogramClue other)
        {
            return Length == other.Length && ColorIndex == other.ColorIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is NonogramClue other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Length, ColorIndex);
        }

        public override string ToString()
        {
            return $"{Length}#{ColorIndex}";
        }
    }
}
