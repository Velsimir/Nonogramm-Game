using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using G.Core.Common;
using G.Gameplay.Nonogram.Level;

namespace G.Meta.LevelSelect
{
    public class LevelTileView : View
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _numberLabel;
        [SerializeField] private TMP_Text _sizeLabel;
        [SerializeField] private RawImage _preview;
        [SerializeField] private GameObject _lockedIcon;
        [SerializeField] private GameObject _unknownIcon;
        [SerializeField] private GameObject _completedIcon;

        private readonly Subject<Unit> _clicked = new();

        public Observable<Unit> Clicked => _clicked;

        private void Awake()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        public void Apply(int number, NonogramLevelAsset level, bool isUnlocked, bool isCompleted,
            Texture2D preview)
        {
            _numberLabel.text = number.ToString();
            _sizeLabel.text = $"{level.Width}x{level.Height}";
            _button.interactable = isUnlocked;

            _lockedIcon.SetActive(isUnlocked == false);
            _unknownIcon.SetActive(isUnlocked && isCompleted == false);
            _completedIcon.SetActive(isCompleted);

            _preview.enabled = isCompleted;
            _preview.texture = isCompleted ? preview : null;
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
