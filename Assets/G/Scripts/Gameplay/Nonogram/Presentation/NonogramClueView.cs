using TMPro;
using UnityEngine;
using UnityEngine.UI;
using G.Gameplay.Nonogram.Data;

namespace G.Gameplay.Nonogram.Presentation
{
    public class NonogramClueView : MonoBehaviour
    {
        private const float DARK_TEXT_LUMINANCE = 0.6f;
        private const float SOLVED_BLEND = 0.7f;

        [SerializeField] private RectTransform _rect;
        [SerializeField] private Image _chip;
        [SerializeField] private TMP_Text _label;

        public void SetGeometry(Vector2 position, float size, float fontSize, float padding)
        {
            _rect.anchoredPosition = new Vector2(position.x + padding * 0.5f, position.y - padding * 0.5f);
            _rect.sizeDelta = new Vector2(size - padding, size - padding);
            _label.fontSize = fontSize;
        }

        public void Apply(NonogramSkin skin, NonogramClue clue, Color clueColor, bool isMonochrome, bool isSolved)
        {
            _label.text = clue.Length.ToString();

            _chip.enabled = true;
            _chip.sprite = skin.ClueChip;

            if (isMonochrome)
            {
                _chip.color = isSolved ? skin.ClueSolvedChipColor : skin.ClueChipColor;
                _label.color = isSolved ? skin.ClueSolvedTextColor : skin.ClueTextColor;
                return;
            }

            Color chipColor = isSolved
                ? Color.Lerp(clueColor, skin.ClueSolvedChipColor, SOLVED_BLEND)
                : clueColor;

            _chip.color = chipColor;
            _label.color = GetContrastColor(chipColor);
        }

        private Color GetContrastColor(Color background)
        {
            float luminance = background.r * 0.299f + background.g * 0.587f + background.b * 0.114f;

            return luminance > DARK_TEXT_LUMINANCE ? Color.black : Color.white;
        }
    }
}
