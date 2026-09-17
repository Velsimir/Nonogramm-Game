using MessagePipe;
using Zenject;
using G.Gameplay.Nonogram.Data;

namespace G.Gameplay.Nonogram.Board
{
    public static class NonogramMessages
    {
        public record BoardLoaded(string LevelId);

        public record BoardCleared();

        public record CellChanged(CellAddress Address);

        public record LineSolved(LineAddress Line);

        public record MistakeMade(CellAddress Address, int LivesLeft);

        public record HintUsed(CellAddress Address, int HintsLeft);

        public record BoardSolved(string LevelId);

        public record BoardFailed(string LevelId);

        public record BoardRevived(string LevelId);

        public record BatchFinished();

        public static void Install(DiContainer container, MessagePipeOptions options)
        {
            container.BindMessageBroker<BoardLoaded>(options);
            container.BindMessageBroker<BoardCleared>(options);
            container.BindMessageBroker<CellChanged>(options);
            container.BindMessageBroker<LineSolved>(options);
            container.BindMessageBroker<MistakeMade>(options);
            container.BindMessageBroker<HintUsed>(options);
            container.BindMessageBroker<BoardSolved>(options);
            container.BindMessageBroker<BoardFailed>(options);
            container.BindMessageBroker<BoardRevived>(options);
            container.BindMessageBroker<BatchFinished>(options);
        }
    }
}
