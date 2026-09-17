using System.Collections.Generic;
using G.Gameplay.Nonogram.Data;

namespace G.Gameplay.Nonogram.Level
{
    public static class NonogramClueBuilder
    {
        public static void BuildLine(IReadOnlyList<byte> colors, List<NonogramClue> result)
        {
            result.Clear();

            byte currentColor = 0;
            int currentLength = 0;

            for (int i = 0; i < colors.Count; i++)
            {
                byte color = colors[i];

                if (color == currentColor)
                {
                    if (color != 0)
                        currentLength++;

                    continue;
                }

                if (currentColor != 0)
                    result.Add(new NonogramClue(currentLength, currentColor));

                currentColor = color;
                currentLength = color == 0 ? 0 : 1;
            }

            if (currentColor != 0)
                result.Add(new NonogramClue(currentLength, currentColor));
        }

        public static NonogramClue[][] BuildRows(IReadOnlyList<byte> solution, int width, int height)
        {
            NonogramClue[][] clues = new NonogramClue[height][];
            List<byte> line = new(width);
            List<NonogramClue> buffer = new();

            for (int row = 0; row < height; row++)
            {
                line.Clear();

                for (int column = 0; column < width; column++)
                    line.Add(solution[row * width + column]);

                BuildLine(line, buffer);
                clues[row] = buffer.ToArray();
            }

            return clues;
        }

        public static NonogramClue[][] BuildColumns(IReadOnlyList<byte> solution, int width, int height)
        {
            NonogramClue[][] clues = new NonogramClue[width][];
            List<byte> line = new(height);
            List<NonogramClue> buffer = new();

            for (int column = 0; column < width; column++)
            {
                line.Clear();

                for (int row = 0; row < height; row++)
                    line.Add(solution[row * width + column]);

                BuildLine(line, buffer);
                clues[column] = buffer.ToArray();
            }

            return clues;
        }

        public static bool AreEqual(IReadOnlyList<NonogramClue> left, IReadOnlyList<NonogramClue> right)
        {
            if (left.Count != right.Count)
                return false;

            for (int i = 0; i < left.Count; i++)
            {
                if (left[i].Equals(right[i]) == false)
                    return false;
            }

            return true;
        }
    }
}
