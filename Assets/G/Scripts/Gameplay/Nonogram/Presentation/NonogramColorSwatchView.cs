using R3;
using UnityEngine;
using UnityEngine.UI;
using G.Core.Common;

namespace G.Gameplay.Nonogram.Presentation
{
    public class NonogramColorSwatchView : View
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _color;
        [SerializeField] private GameObject _selection;

        private readonly Subject<Unit> _clicked = new();

        private byte _colorIndex;

        public Observable<Unit> Clicked => _clicked;
        public byte ColorIndex => _colorIndex;

        private void Awake()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        public void Apply(byte colorIndex, Color color)
        {
            _colorIndex = colorIndex;
            _color.color = color;
        }

        public void SetSelected(bool isSelected)
        {
            _selection.SetActive(isSelected);
        }

        protected override void OnDestroyed()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
            _clicked.Dispose();
        }

        private void OnButtonClicked()
        {
            _clicked.OnNext(Unit.Default);
        }
    }
}
