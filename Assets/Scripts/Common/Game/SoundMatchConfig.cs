using System.Collections.Generic;
using UnityEngine;

namespace Geidai.Common.Game
{
    /// <summary>
    /// ①音合わせの出題パラメータ（U6 / FR-18）。出題数・選択肢数・難易度（セント段階）・fallback を
    /// Sさん がインスペクタで調整可能な ScriptableObject（データ駆動・再ビルド不要）。
    /// 異常値はアクセサでクランプする（BR-GAME1-32）。既定アセットは MCP 生成。
    /// </summary>
    [CreateAssetMenu(fileName = "SoundMatchConfig", menuName = "Geidai/Sound Match Config", order = 1)]
    public class SoundMatchConfig : ScriptableObject
    {
        [Tooltip("1 ゲームの出題数（>=1）。")]
        [SerializeField] private int questionCount = 5;

        [Tooltip("1 問の選択肢数（>=2）。")]
        [SerializeField] private int choiceCount = 3;

        [Tooltip("難易度段階（label＋選択肢間の最小ピッチ間隔[セント]）。")]
        [SerializeField]
        private List<DifficultyLevel> difficulties = new List<DifficultyLevel>
        {
            new DifficultyLevel("かんたん", 200),
            new DifficultyLevel("ふつう", 100),
            new DifficultyLevel("むずかしい", 50),
            new DifficultyLevel("とても難しい", 20),
        };

        [Tooltip("保存音が 0 件のときに使う出題素材（任意）。無ければフォールバック表示。")]
        [SerializeField] private AudioClip fallbackClip;

        /// <summary>出題数（>=1 にクランプ）。</summary>
        public int QuestionCount => Mathf.Max(1, questionCount);

        /// <summary>選択肢数（>=2 にクランプ）。</summary>
        public int ChoiceCount => Mathf.Max(2, choiceCount);

        /// <summary>難易度段階（読み取り専用）。</summary>
        public IReadOnlyList<DifficultyLevel> Difficulties => difficulties;

        /// <summary>保存音 0 件時のフォールバック素材（無ければ null）。</summary>
        public AudioClip FallbackClip => fallbackClip;

        /// <summary>
        /// 指定 index の難易度を返す（範囲外/未設定は既定 "ふつう"=100 セント）。centsStep は >=1 にクランプ。
        /// </summary>
        public DifficultyLevel GetDifficulty(int index)
        {
            if (difficulties == null || difficulties.Count == 0)
                return new DifficultyLevel("ふつう", 100);

            int clamped = Mathf.Clamp(index, 0, difficulties.Count - 1);
            var d = difficulties[clamped];
            if (d.centsStep < 1) d.centsStep = 1;
            return d;
        }

        /// <summary>差し替え/生成用（既定アセットのプログラム生成・テスト）。</summary>
        public void SetValues(int questionCount, int choiceCount, List<DifficultyLevel> difficulties, AudioClip fallbackClip)
        {
            this.questionCount = questionCount;
            this.choiceCount = choiceCount;
            this.difficulties = difficulties ?? new List<DifficultyLevel>();
            this.fallbackClip = fallbackClip;
        }
    }
}
