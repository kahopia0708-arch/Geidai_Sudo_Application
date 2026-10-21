using UnityEngine;

namespace Geidai.Common.Game
{
    [CreateAssetMenu(fileName = "MemoryMatchConfig", menuName = "Geidai/Memory Match Config", order = 2)]
    public class MemoryMatchConfig : ScriptableObject
    {
        [Tooltip("使用するペア数（カード枚数 = ペア数 × 2）。")]
        [SerializeField] private int pairCount = 4;

        [Tooltip("不一致のとき、2枚を伏せるまでの秒数。")]
        [SerializeField] private float mismatchHideDelaySeconds = 0.9f;

        [Tooltip("一致後に次の操作を受け付けるまでの短い待ち（秒）。")]
        [SerializeField] private float matchPauseSeconds = 0.35f;

        public int PairCount => Mathf.Clamp(pairCount, 1, 8);
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
