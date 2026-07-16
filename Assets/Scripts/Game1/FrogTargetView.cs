using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Game1
{
    /// <summary>
    /// お手本（カエル）＋ドロップ領域（U6 / P5 / US-GAME1-04）。
    /// タップでお手本ピッチを確認再生、選択肢のドロップ先（当たり判定）を提供する。
    /// 見た目・演出は Sさん が調整可能（US-TECH-07）。
    /// </summary>
    public class FrogTargetView : MonoBehaviour
    {
        [Tooltip("お手本を確認再生するボタン（未設定なら再生ボタンなし）。")]
        [SerializeField] private Button previewButton;

        [Tooltip("ドロップ判定に使う領域（未設定なら自身の RectTransform）。")]
        [SerializeField] private RectTransform dropArea;

        private SoundMatchGameController _controller;

        public void Setup(SoundMatchGameController controller)
        {
            _controller = controller;
            if (dropArea == null) dropArea = transform as RectTransform;

            if (previewButton != null)
            {
                previewButton.onClick.RemoveListener(OnPreview);
                previewButton.onClick.AddListener(OnPreview);
            }
        }

        private void OnDestroy()
        {
            if (previewButton != null) previewButton.onClick.RemoveListener(OnPreview);
        }

        private void OnPreview()
        {
            if (_controller != null) _controller.PreviewTarget();
        }

        /// <summary>指定スクリーン座標がドロップ領域内かを返す。</summary>
        public bool ContainsScreenPoint(Vector2 screenPos, Camera cam)
        {
            if (dropArea == null) dropArea = transform as RectTransform;
            if (dropArea == null) return false;
            return RectTransformUtility.RectangleContainsScreenPoint(dropArea, screenPos, cam);
        }
    }
}
