using Cysharp.Threading.Tasks;
using G.Core.Common;
using G.Meta.LevelFlow;

namespace G.Meta.LevelSelect
{
    public class MenuSceneEntry : Service
    {
        private readonly ILevelFlowService _flow;

        public MenuSceneEntry(ILevelFlowService flow)
        {
            _flow = flow;
        }

        protected override void OnInitialize()
        {
            CompleteTransitionAsync().Forget();
        }

        private async UniTaskVoid CompleteTransitionAsync()
        {
            await UniTask.NextFrame(DisposeToken);
            await _flow.CompleteTransitionAsync(DisposeToken);
        }
    }
}
