using UnityEngine;
using G.Gameplay.Nonogram.Data;

namespace G.Gameplay.Nonogram.Config
{
    [CreateAssetMenu(fileName = "NonogramRuleConfig", menuName = "Configs/Nonogram/Rule Config")]
    public class NonogramRuleConfig : ScriptableObject
    {
        [SerializeField] private MistakeMode _mistakeMode = MistakeMode.Validated;
        [SerializeField, Min(1)] private int _lives = 3;
        [SerializeField, Min(0)] private int _hints = 3;
        [SerializeField] private bool _allowMarks = true;
        [SerializeField] private bool _autoMarkCompletedLines = true;

        public MistakeMode MistakeMode => _mistakeMode;
        public int Lives => _lives;
        public int Hints => _hints;
        public bool AllowMarks => _allowMarks;
        public bool AutoMarkCompletedLines => _autoMarkCompletedLines;
    }
}
