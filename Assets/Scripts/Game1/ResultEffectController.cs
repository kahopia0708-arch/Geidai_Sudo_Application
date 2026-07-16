using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Game1
{
    /// <summary>
    /// 正解演出（カエル成長）・不正解のやり直し・結果サマリの提示（U6 / P5 / US-GAME1-05）。
    /// ロジックは持たず「見せ方」に徹する。不正解はペナルティ無し（やり直しを促す）。
    /// アニメ/差分・文言は Sさん が調整可能（US-TECH-07）。
    /// </summary>
    public class ResultEffectController : MonoBehaviour
    {
        [Header("正解演出（カエル成長）")]
        [Tooltip("成長段階のスプライト（correctCount に応じて進む）。")]
        [SerializeField] private Image frogImage;
        [SerializeField] private Sprite[] growthStages;

        [Header("フィードバック表示")]
        [SerializeField] private GameObject correctFeedback;
        [SerializeField] private GameObject retryFeedback;

        [Header("結果サマリ")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private Text resultText;

        private int _growthIndex;

        private void OnEnable()
        {
            HideTransient();
        }

        /// <summary>正解時：カエルを1段階成長させ、正解フィードバックを表示する。</summary>
        public void PlayCorrect()
        {
            HideTransient();
            if (correctFeedback != null) correctFeedback.SetActive(true);

            if (frogImage != null && growthStages != null && growthStages.Length > 0)
            {
                _growthIndex = Mathf.Min(_growthIndex + 1, growthStages.Length - 1);
                frogImage.sprite = growthStages[_growthIndex];
            }
        }

        /// <summary>不正解時：やり直しを促す（ペナルティ無し・進行は変えない）。</summary>
        public void PlayRetry()
        {
            HideTransient();
            if (retryFeedback != null) retryFeedback.SetActive(true);
        }

        /// <summary>結果サマリを表示する。</summary>
        public void ShowResult(int correctCount, int total)
        {
            HideTransient();
            if (resultPanel != null) resultPanel.SetActive(true);
            if (resultText != null) resultText.text = $"{total} もん ちゅう {correctCount} もん せいかい！";
        }

        private void HideTransient()
        {
            if (correctFeedback != null) correctFeedback.SetActive(false);
            if (retryFeedback != null) retryFeedback.SetActive(false);
        }
    }
}
