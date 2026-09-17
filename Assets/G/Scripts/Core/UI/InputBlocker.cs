using UnityEngine;

namespace G.Core.UI
{
    public class InputBlocker : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        private int _lockCount;

        public bool IsBlocked => _lockCount > 0;

        public void Lock()
        {
            _lockCount++;
            Apply();
        }

        public void Unlock()
        {
            _lockCount = Mathf.Max(0, _lockCount - 1);
            Apply();
        }

        private void Apply()
        {
            _canvasGroup.blocksRaycasts = IsBlocked;
        }
    }
}
