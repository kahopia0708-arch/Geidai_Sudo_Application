using System.Collections.Generic;
using UnityEngine;
using Geidai.Common.Game;

namespace Geidai.Game3
{
    /// <summary>
    /// ③音ならべの出題設定。
    /// 出題数・選択肢数・難易度・カタログ音をInspectorから調整する。
    /// </summary>
    [CreateAssetMenu(
        fileName = "Game3Config",
        menuName = "Geidai/Game3 Config",
        order = 3)]
    public class Game3Config : ScriptableObject
    {
        [Tooltip("1ゲームの出題数（>=1）。")]
        [SerializeField] private int questionCount = 5;

        [Tooltip("1問で並べる音の数（2〜4）。")]
        [SerializeField] private int choiceCount = 3;

        [Tooltip("難易度段階。現段階では音同士の最小ピッチ間隔[セント]として使用。")]
        [SerializeField]
        private List<DifficultyLevel> difficulties = new List<DifficultyLevel>
        {
            new DifficultyLevel("かんたん", 300),
            new DifficultyLevel("ふつう", 200),
            new DifficultyLevel("むずかしい", 100),
            new DifficultyLevel("とても難しい", 50),
        };

        [Tooltip("音ならべで優先して使用するカタログ音。")]
        [SerializeField] private List<AudioClip> catalogClips = new List<AudioClip>();

        public int QuestionCount => Mathf.Max(1, questionCount);

        public int ChoiceCount => Mathf.Clamp(choiceCount, 2, 4);

        public IReadOnlyList<DifficultyLevel> Difficulties => difficulties;

        public IReadOnlyList<AudioClip> CatalogClips => catalogClips;

        public DifficultyLevel GetDifficulty(int index)
        {
            if (difficulties == null || difficulties.Count == 0)
                return new DifficultyLevel("ふつう", 200);

            int clamped = Mathf.Clamp(index, 0, difficulties.Count - 1);
            var difficulty = difficulties[clamped];

            if (difficulty.centsStep < 1)
                difficulty.centsStep = 1;

            return difficulty;
        }
    }
}
