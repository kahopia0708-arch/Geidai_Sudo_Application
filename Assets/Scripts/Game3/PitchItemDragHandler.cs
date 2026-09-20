using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

namespace Geidai.Game3
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class PitchItemDragHandler :
        MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        private RectTransform _rect;
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private PitchItemView _itemView;

        [Header("Drag UI")]
        // ドラッグ中・雲配置中の親]
        [SerializeField] private RectTransform _dragRoot;

        // 最初の下段の置き場所
        private RectTransform _homeParent;
        private int _homeSiblingIndex;

        private Vector2 _homeAnchorMin;
        private Vector2 _homeAnchorMax;
        private Vector2 _homePivot;
        private Vector2 _homeSizeDelta;
        private Vector3 _homeLocalScale;

        // 現在置かれている雲
        private CloudSlotView _currentSlot;

        private bool _placedSuccessfully;

        [Header("Wrong Answer Animation")]
        [SerializeField] private float fallDuration = 0.25f;
        [SerializeField] private float fallDistance = 120f;

        private bool _returningHome;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _itemView = GetComponent<PitchItemView>();
            // 最初の「下のカード置き場」を保存
            _homeParent = transform.parent as RectTransform;
            _homeSiblingIndex = transform.GetSiblingIndex();

            _homeAnchorMin = _rect.anchorMin;
            _homeAnchorMax = _rect.anchorMax;
            _homePivot = _rect.pivot;
            _homeSizeDelta = _rect.sizeDelta;
            _homeLocalScale = _rect.localScale;
        }

        // --------------------------------------------------
        // ドラッグ開始
        // --------------------------------------------------
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_dragRoot == null)
                return;

            // ドラッグ開始時にも音を鳴らす
            if (_itemView != null)
            {
                _itemView.NotifyDragStarted();
            }

            _placedSuccessfully = false;

            // すでに雲に置かれていたなら、
            // その雲を空けてからドラッグ開始
            if (_currentSlot != null)
            {
                _currentSlot.Release(_itemView);
                _currentSlot = null;
            }

            MoveToDragRootPreservingVisual();

            // ドラッグ中は自分自身がRaycastを邪魔しない
            _canvasGroup.blocksRaycasts = false;
        }

        // --------------------------------------------------
        // 見た目の大きさを維持したままCanvas直下へ移す
        // --------------------------------------------------
        private void MoveToDragRootPreservingVisual()
        {
            Vector3[] corners = new Vector3[4];
            _rect.GetWorldCorners(corners);

            Vector3 worldCenter =
                (corners[0] + corners[2]) * 0.5f;

            Vector3 localBottomLeft =
                _dragRoot.InverseTransformPoint(corners[0]);

            Vector3 localTopRight =
                _dragRoot.InverseTransformPoint(corners[2]);

            Vector2 visualSize = new Vector2(
                Mathf.Abs(localTopRight.x - localBottomLeft.x),
                Mathf.Abs(localTopRight.y - localBottomLeft.y)
            );

            transform.SetParent(_dragRoot, false);
            transform.SetAsLastSibling();

            _rect.anchorMin = new Vector2(0.5f, 0.5f);
            _rect.anchorMax = new Vector2(0.5f, 0.5f);
            _rect.pivot = new Vector2(0.5f, 0.5f);

            _rect.sizeDelta = visualSize;
            _rect.localScale = Vector3.one;
            _rect.position = worldCenter;
        }

        // --------------------------------------------------
        // ドラッグ中
        // --------------------------------------------------
        public void OnDrag(PointerEventData eventData)
        {
            if (_dragRoot == null)
                return;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                _dragRoot,
                eventData.position,
                eventData.pressEventCamera,
                out Vector3 worldPoint))
            {
                _rect.position = worldPoint;
            }
        }

        // --------------------------------------------------
        // CloudSlotへ配置
        // --------------------------------------------------
        public bool TryPlaceInSlot(CloudSlotView slot)
        {
            if (slot == null || _itemView == null)
                return false;

            // その雲にすでに別カードがあったら、
            // 既存カードを下へ戻す
            PitchItemView existingItem = slot.Occupant;

            if (existingItem != null &&
                existingItem != _itemView)
            {
                PitchItemDragHandler existingDrag =
                    existingItem.GetComponent<PitchItemDragHandler>();

                if (existingDrag != null)
                {
                    existingDrag.SendHome();
                }
                else
                {
                    slot.Release(existingItem);
                }
            }

            // 念のため以前の雲を空ける
            if (_currentSlot != null &&
                _currentSlot != slot)
            {
                _currentSlot.Release(_itemView);
            }

            slot.Accept(_itemView);
            _currentSlot = slot;

            // CloudSlotの子にはしない。
            // Canvas上のままCloudSlotの中心へ移動する。
            RectTransform slotRect =
                slot.GetComponent<RectTransform>();

            if (slotRect == null)
                return false;

            Vector3 slotCenter =
                slotRect.TransformPoint(slotRect.rect.center);

            _rect.position = slotCenter;

            transform.SetAsLastSibling();

            _placedSuccessfully = true;

            return true;
        }

        // --------------------------------------------------
        // 少し重なっているCloudSlotを探す
        // --------------------------------------------------
        private CloudSlotView FindOverlappingSlot()
        {
            CloudSlotView[] slots =
                FindObjectsByType<CloudSlotView>();

            CloudSlotView bestSlot = null;
            float bestOverlap = 0f;

            Rect itemRect = GetWorldRect(_rect);

            foreach (CloudSlotView slot in slots)
            {
                if (!slot.gameObject.activeInHierarchy)
                    continue;

                RectTransform slotRect =
                    slot.GetComponent<RectTransform>();

                if (slotRect == null)
                    continue;

                Rect cloudRect =
                    GetWorldRect(slotRect);

                float overlap =
                    CalculateOverlapArea(
                        itemRect,
                        cloudRect
                    );

                if (overlap > bestOverlap)
                {
                    bestOverlap = overlap;
                    bestSlot = slot;
                }
            }

            float itemArea =
                itemRect.width * itemRect.height;

            if (itemArea <= 0f)
                return null;

            float overlapRatio =
                bestOverlap / itemArea;

            // 15%以上重なっていれば吸着
            return overlapRatio >= 0.15f
                ? bestSlot
                : null;
        }

        private static Rect GetWorldRect(
            RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            return Rect.MinMaxRect(
                corners[0].x,
                corners[0].y,
                corners[2].x,
                corners[2].y
            );
        }

        private static float CalculateOverlapArea(
            Rect a,
            Rect b)
        {
            float width = Mathf.Max(
                0f,
                Mathf.Min(a.xMax, b.xMax) -
                Mathf.Max(a.xMin, b.xMin)
            );

            float height = Mathf.Max(
                0f,
                Mathf.Min(a.yMax, b.yMax) -
                Mathf.Max(a.yMin, b.yMin)
            );

            return width * height;
        }

        // --------------------------------------------------
        // ドラッグ終了
        // --------------------------------------------------
        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = true;

            // OnDropが来なくても、
            // 雲と15%以上重なっていれば吸着
            if (!_placedSuccessfully)
            {
                CloudSlotView overlappingSlot =
                    FindOverlappingSlot();

                if (overlappingSlot != null)
                {
                    TryPlaceInSlot(overlappingSlot);
                }
            }

            // どの雲にも置かなかった
            // → 下の元の位置へ戻す
            if (!_placedSuccessfully)
            {
                SendHome();
            }

            if (_itemView != null)
            {
                _itemView.NotifyDragFinished();
                _itemView.NotifyOrderChanged();
            }
        }

        // --------------------------------------------------
        // 下のカード置き場へ戻す
        // --------------------------------------------------
        public void SendHome()
        {
            if (_currentSlot != null)
            {
                _currentSlot.Release(_itemView);
                _currentSlot = null;
            }

            if (_homeParent == null)
                return;

            transform.SetParent(_homeParent, false);

            _rect.anchorMin = _homeAnchorMin;
            _rect.anchorMax = _homeAnchorMax;
            _rect.pivot = _homePivot;
            _rect.sizeDelta = _homeSizeDelta;
            _rect.localScale = _homeLocalScale;

            transform.SetSiblingIndex(_homeSiblingIndex);

            LayoutRebuilder.ForceRebuildLayoutImmediate(
                _homeParent
            );

            _placedSuccessfully = false;
        }

        public void FallAndSendHome()
        {
            if (_returningHome)
                return;

            StartCoroutine(FallAndSendHomeCoroutine());
        }

        private IEnumerator FallAndSendHomeCoroutine()
        {
            _returningHome = true;
            _canvasGroup.blocksRaycasts = false;

            // 今入っている雲を空ける
            if (_currentSlot != null)
            {
                _currentSlot.Release(_itemView);
                _currentSlot = null;
            }

            // 現在位置
            Vector3 startPosition = _rect.position;

            // 元のPitchItemContainerの「高さ」だけ取得
            Vector3 homeCenter =
                _homeParent.TransformPoint(
                    _homeParent.rect.center
                );

            // Xは現在位置のまま
            // Yだけ下段の高さへ
            Vector3 targetPosition = new Vector3(
                startPosition.x,
                homeCenter.y,
                startPosition.z
            );

            float elapsed = 0f;

            while (elapsed < fallDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(
                    elapsed / fallDuration
                );

                // 重力っぽく加速
                float eased = t * t;

                _rect.position =
                    Vector3.LerpUnclamped(
                        startPosition,
                        targetPosition,
                        eased
                    );

                yield return null;
            }

            _rect.position = targetPosition;

            // ★ここでは SendHome() しない
            // 落ちた場所にそのまま残す

            _placedSuccessfully = false;

            _canvasGroup.blocksRaycasts = true;
            _returningHome = false;
        }
    }
}