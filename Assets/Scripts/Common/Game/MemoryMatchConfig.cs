using UnityEngine;

namespace Geidai.Common.Game
{
    [CreateAssetMenu(fileName = "MemoryMatchConfig", menuName = "Geidai/Memory Match Config", order = 2)]
    public class MemoryMatchConfig : ScriptableObject
    {
        [Tooltip("使用するペア数（カード枚数 = ペア数 × 2）。UI 未配置時の既定。3〜9（18枚）。")]
        [SerializeField] private int pairCount = 4;

        [Tooltip("めくったあとカード上に図鑑名を出す（展示ではオフ推奨）。")]
        [SerializeField] private bool showCardLabels;

        [Tooltip("めくったときの表絵柄（全カード共通。未設定時は Game2 既定アート）。")]
        [SerializeField] private Sprite cardFaceSprite;

        [Tooltip("伏せ面の裏絵柄（素材未到着時は空のまま＝プレースホルダ色）。")]
        [SerializeField] private Sprite cardBackSprite;

        [Tooltip("不一致のとき、2枚を伏せるまでの秒数。")]
        [SerializeField] private float mismatchHideDelaySeconds = 0.9f;

        [Tooltip("一致後に次の操作を受け付けるまでの短い待ち（秒）。")]
        [SerializeField] private float matchPauseSeconds = 0.35f;

        public int PairCount => Mathf.Clamp(pairCount, 1, 9);
        public bool ShowCardLabels => showCardLabels;
        public Sprite CardFaceSprite => cardFaceSprite;
        public Sprite CardBackSprite => cardBackSprite;
        public float MismatchHideDelaySeconds => Mathf.Max(0.2f, mismatchHideDelaySeconds);
        public float MatchPauseSeconds => Mathf.Max(0f, matchPauseSeconds);

        public void SetValues(int pairCount, float mismatchHideDelaySeconds, float matchPauseSeconds)
        {
            this.pairCount = pairCount;
            this.mismatchHideDelaySeconds = mismatchHideDelaySeconds;
            this.matchPauseSeconds = matchPauseSeconds;
        }
    }
}
