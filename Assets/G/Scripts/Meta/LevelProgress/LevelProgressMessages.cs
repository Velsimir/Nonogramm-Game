using MessagePipe;
using Zenject;

namespace G.Meta.LevelProgress
{
    public static class LevelProgressMessages
    {
        public record LevelCompleted(string LevelId);

        public record LevelUnlocked(string LevelId);

        public static void Install(DiContainer container, MessagePipeOptions options)
        {
            container.BindMessageBroker<LevelCompleted>(options);
            container.BindMessageBroker<LevelUnlocked>(options);
        }
    }
}
