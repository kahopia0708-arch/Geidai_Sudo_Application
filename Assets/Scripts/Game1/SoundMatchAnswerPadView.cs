using Geidai.Common.UI;
using UnityEngine;

namespace Geidai.Game1
{
    /// <summary>
    /// 弟子側（答えを置く場所）。楽器を重ねると吸着して答え合わせが始まる。
    /// </summary>
    public class SoundMatchAnswerPadView : MonoBehaviour
    {
        [SerializeField] private RectTransform dropArea;
        [SerializeField] private RectTransform snapAnchor;

        [Tooltip("ドロップ領域をスクリーン上で拡張する余白（px）。")]
        [SerializeField] private float dropPaddingPixels = 48f;

        [SerializeField] private float minOverlapAreaPixels = 800f;

        public RectTransform SnapAnchor =>
            snapAnchor != null ? snapAnchor : dropArea != null ? dropArea : transform as RectTransform;

        public void Configure(RectTransform drop, RectTransform snap)
        {
            dropArea = drop;
            snapAnchor = snap;
        }

        public bool AcceptsDrop(RectTransform choiceRect, Vector2 screenPos, Camera eventCamera)
        {
            if (dropArea == null) dropArea = transform as RectTransform;
            if (dropArea == null || choiceRect == null) return false;

            if (RectTransformHitTest.Overlaps(
                    dropArea,
                    choiceRect,
                    eventCamera,
                    minOverlapAreaPixels))
            {
                return true;
            }

            if (RectTransformHitTest.ContainsScreenPoint(
                    dropArea,
                    RectTransformHitTest.GetScreenCenter(choiceRect, eventCamera),
                    eventCamera,
                    dropPaddingPixels))
            {
                return true;
            }

            return RectTransformHitTest.ContainsScreenPoint(
                dropArea,
                screenPos,
                eventCamera,
                dropPaddingPixels);
        }
    }
}
