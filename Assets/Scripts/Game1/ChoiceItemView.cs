using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Geidai.Common.Game;

namespace Geidai.Game1
{
    /// <summary>
    /// 選択肢（おたまじゃくし）1件（U6 / P5 / US-GAME1-04）。
    /// タップで確認再生、カエルへドラッグ＆ドロップで解答。領域外ドロップは元位置へ戻す（やり直し無ペナルティ）。
    /// 見た目・演出は Sさん が調整可能（US-TECH-07）。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ChoiceItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Tooltip("タップで確認再生するボタン（未設定なら再生ボタンなし）。")]
        [SerializeField] private Button previewButton;

        [Tooltip("ドラッグ中に他要素のレイキャストを透過させる（未設定なら無視）。")]
        [SerializeField] private CanvasGroup canvasGroup;

        private SoundMatchGameController _controller;
        private ChoiceSpec _spec;
        private int _index = -1;

        private RectTransform _rt;
        private RectTransform _parentRt;
        private Canvas _canvas;
        private Vector2 _originalPos;

        public int Index => _index;
        public int Cents => _spec.cents;

        private void Awake()
        {
            _rt = transform as RectTransform;
        }

        public void Setup(SoundMatchGameController controller, ChoiceSpec spec, int index)
        {
            _controller = controller;
            _spec = spec;
            _index = index;

            if (_rt == null) _rt = transform as RectTransform;
            _parentRt = _rt != null ? _rt.parent as RectTransform : null;
            if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
            if (_rt != null) _originalPos = _rt.anchoredPosition;

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
            if (_controller != null) _controller.PreviewChoice(_spec.cents);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_rt != null) _originalPos = _rt.anchoredPosition;
            if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
            if (_rt != null) _rt.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_rt == null || _parentRt == null) return;
            Camera cam = _canvas != null ? _canvas.worldCamera : null;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRt, eventData.position, cam, out Vector2 local))
                _rt.anchoredPosition = local;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
            if (_controller != null) _controller.OnChoiceDropped(this, eventData.position);
            else ResetPosition();
        }

        /// <summary>元の位置へ戻す（領域外ドロップ・不正解のやり直し）。</summary>
        public void ResetPosition()
        {
            if (_rt != null) _rt.anchoredPosition = _originalPos;
        }
    }
}
