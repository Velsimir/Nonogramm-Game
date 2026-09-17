using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using G.Gameplay.Nonogram.Data;

namespace G.Gameplay.Nonogram.Presentation
{
    public class NonogramCellView : MonoBehaviour
    {
        [SerializeField] private RectTransform _rect;
        [SerializeField] private Image _background;
        [SerializeField] private Image _content;

        private Tween _revealTween;

        public void SetGeometry(Vector2 position, float size)
        {
            _rect.anchoredPosition = position;
            _rect.sizeDelta = new Vector2(size, size);
        }

        public async UniTask RevealAsync(Color target, float duration, float delay,
            CancellationToken cancellationToken)
        {
            if (_content.enabled == false)
                return;

            KillReveal();

            _revealTween = _content.DOColor(target, duration)
                .SetDelay(delay)
                .SetEase(Ease.OutQuad);

            await _revealTween.ToUniTask(cancellationToken: cancellationToken).SuppressCancellationThrow();

            _revealTween = null;
        }

        private void OnDestroy()
        {
            KillReveal();
        }

        private void KillReveal()
        {
            if (_revealTween == null)
                return;

            _revealTween.Kill();
            _revealTween = null;
        }

        public void Apply(NonogramSkin skin, NonogramCell cell, Color fillColor)
        {
            _background.sprite = skin.CellBackground;
            _background.color = skin.CellBackgroundColor;

            switch (cell.State)
            {
                case CellState.Filled:
                    _content.enabled = true;
                    _content.sprite = skin.CellFilled;
                    _content.color = fillColor;
                    break;

                case CellState.Marked:
                    _content.enabled = true;
                    _content.sprite = skin.CellMarked;
                    _content.color = skin.CellMarkedColor;
                    break;

                default:
                    _content.enabled = false;
                    break;
            }
        }
    }
}
