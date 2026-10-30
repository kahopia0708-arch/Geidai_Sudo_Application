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
        private Vector3 _homeWorldPosition;
        private bool _homePositionSaved;
        private Vector2 _homeAnchorMin;
        private Vector2 _homeAnchorMax;
        private Vector2 _homePivot;
        private Vector2 _homeSizeDelta;
        private Vector3 _homeLocalScale;
        private Vector2 _homeAnchoredPosition;

        // 現在置かれている雲
        private CloudSlotView _currentSlot;

        private bool _placedSuccessfully;

        //雲の上の置き場所
        [Header("Cloud Placement")]
        [SerializeField] private float placedYOffset = 65f;

        //雲着地
        [Header("Cloud Landing")]
        [SerializeField] private float landingDuration = 0.28f;
        [SerializeField] private float dropZoneAbove = 1.0f;

        private Coroutine _landingCoroutine;
        private CloudSlotView _landingSlot;
        private Vector3 _landingTarget;

        [Header("Wrong Answer Animation")]
        [SerializeField] private float fallDuration = 0.7f;
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
            _homeAnchoredPosition = _rect.anchoredPosition;
        }
        public void CaptureHomePosition()
        {
            if (transform.parent != _homeParent ||
                !gameObject.activeInHierarchy)
                return;

            _homeWorldPosition = _rect.position;
            _homePositionSaved = true;
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

            // CloudSlotの子にはしない。
            // Canvas上のままCloudSlotの中心へ移動する。
            RectTransform slotRect = slot.GetComponent<RectTransform>();

            if (slotRect == null)
                return false;

            // 雲への配置を予約
            slot.Accept(_itemView);
            _currentSlot = slot;
            _landingSlot = slot;

            // 着地位置を記録するだけ。まだ移動しない
            _landingTarget = slotRect.TransformPoint(
                slotRect.rect.center + Vector2.up * placedYOffset
            );

            _placedSuccessfully = true;
            return true;
        }

        // --------------------------------------------------
        // 少し重なっているCloudSlotを探す
        // --------------------------------------------------
        private CloudSlotView FindOverlappingSlot()
        {
            CloudSlotView[] slots = FindObjectsByType<CloudSlotView>();

            Rect itemRect = GetWorldRect(_rect);
            Vector2 itemCenter = itemRect.center;

            CloudSlotView bestSlot = null;
            float bestDistance = float.MaxValue;

            foreach (CloudSlotView slot in slots)
            {
                if (!slot.gameObject.activeInHierarchy)
                    continue;

                RectTransform slotRect =
                    slot.GetComponent<RectTransform>();

                if (slotRect == null)
                    continue;

                Rect cloudRect = GetWorldRect(slotRect);

                // 左右の判定
                float horizontalMargin = itemRect.width * 0.2f;

                if (itemCenter.x < cloudRect.xMin - horizontalMargin ||
                    itemCenter.x > cloudRect.xMax + horizontalMargin)
                    continue;

                // 雲の上方向へ判定範囲を拡張
                float minY = cloudRect.yMin;
                float maxY = cloudRect.yMax
                    + itemRect.height * dropZoneAbove;

                if (itemCenter.y < minY || itemCenter.y > maxY)
                    continue;

                // 最も近い雲を選択
                float distance =
                    Mathf.Abs(itemCenter.x - cloudRect.center.x) +
                    Mathf.Abs(
                        itemCenter.y -
                        (cloudRect.center.y + placedYOffset)
                    ) * 0.25f;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestSlot = slot;
                }
            }

            return bestSlot;
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

            // OnDropで配置されていない場合、
            // 雲の上側も含めた判定を行う
            if (!_placedSuccessfully)
            {
                CloudSlotView overlappingSlot = FindOverlappingSlot();

                if (overlappingSlot != null)
                {
                    TryPlaceInSlot(overlappingSlot);
                }
            }

            if (_placedSuccessfully)
            {
                // 離した位置から雲に向かって落下
                _landingCoroutine = StartCoroutine(AnimateLanding());
            }
            else
            {
                // 雲が見つからなければ元の位置に戻す
                SendHome();

                if (_itemView != null)
                    _itemView.NotifyOrderChanged();
            }

            if (_itemView != null)
                _itemView.NotifyDragFinished();
        }

        private IEnumerator AnimateLanding()
        {
            _canvasGroup.blocksRaycasts = false;

            Vector3 start = _rect.position;
            Vector3 target = _landingTarget;

            float elapsed = 0f;
            float duration = Mathf.Max(0.05f, landingDuration);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(elapsed / duration);

                // 横方向は滑らかに位置合わせ
                float smooth = t * t * (3f - 2f * t);

                // 上から落ちる場合は重力のように加速
                float vertical = start.y > target.y
                    ? t * t
                    : smooth;

                _rect.position = new Vector3(
                    Mathf.Lerp(start.x, target.x, smooth),
                    Mathf.Lerp(start.y, target.y, vertical),
                    Mathf.Lerp(start.z, target.z, smooth)
                );

                yield return null;
            }

            _rect.position = target;

            // 着地後に雲の背面へ移動
            transform.SetParent(_landingSlot.transform.parent, true);
            transform.SetAsFirstSibling();

            _canvasGroup.blocksRaycasts = true;

            // 着地の衝撃で雲をぽよんと動かす
            _landingSlot.PlayBounce();

            _landingCoroutine = null;

            // 着地してから正誤判定
            if (_itemView != null)
                _itemView.NotifyOrderChanged();
        }
        // --------------------------------------------------
        // 下のカード置き場へ戻す
        // --------------------------------------------------
        public void SendHome()
        {
            if (_landingCoroutine != null)
            {
                StopCoroutine(_landingCoroutine);
                _landingCoroutine = null;
                _canvasGroup.blocksRaycasts = true;
            }
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
            _rect.anchoredPosition = _homeAnchoredPosition;

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

            if (_landingCoroutine != null)
            {
                StopCoroutine(_landingCoroutine);
                _landingCoroutine = null;
            }

            // 雲の占有を解除
            if (_currentSlot != null)
            {
                _currentSlot.Release(_itemView);
                _currentSlot = null;
            }

            // 初期位置が未記録なら即座に戻す
            if (_homeParent == null || _dragRoot == null)
            {
                SendHome();
                _canvasGroup.blocksRaycasts = true;
                _returningHome = false;
                yield break;
            }

            // 前面のDragLayerに移動
            MoveToDragRootPreservingVisual();

            Vector3 startPosition = _rect.position;
            Vector3 targetPosition = _homePositionSaved
                ? _homeWorldPosition
                : _homeParent.TransformPoint(_homeParent.rect.center);

            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, fallDuration);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(elapsed / duration);

                // ゆっくり動き始め、着地時に減速
                float eased = t * t * (3f - 2f * t);

                _rect.position = Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    eased
                );

                yield return null;
            }

            // 元の配置へ戻す
            SendHome();

            _canvasGroup.blocksRaycasts = true;
            _returningHome = false;
        }

    }
}
