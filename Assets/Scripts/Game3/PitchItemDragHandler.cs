using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Geidai.Game3
{
    [RequireComponent(typeof(RectTransform))]
    public class PitchItemDragHandler :
        MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [Header("Animation")]
        [SerializeField] private float shiftDuration = 0.12f;
        [SerializeField] private float dropDuration = 0.10f;

        private RectTransform _rect;
        private RectTransform _container;
        private Canvas _canvas;
        private HorizontalLayoutGroup _layoutGroup;

        // ドラッグ開始時の各スロット位置
        private readonly List<Vector2> _slotPositions = new();

        // ドラッグ中の自分以外のPitchItem
        private readonly List<RectTransform> _otherItems = new();

        // 各Itemを動かしているCoroutine
        private readonly Dictionary<RectTransform, Coroutine> _moveCoroutines = new();

        private int _targetIndex;
        private float _dragY;
        private bool _dragging;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _container = transform.parent as RectTransform;
            _canvas = GetComponentInParent<Canvas>();
            _layoutGroup = GetComponentInParent<HorizontalLayoutGroup>();
        }

        // --------------------------------------------------
        // ドラッグ開始
        // --------------------------------------------------
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_container == null || _layoutGroup == null)
                return;
            PitchItemView itemView = GetComponent<PitchItemView>();
            if (itemView != null)
                itemView.NotifyDragStarted();
            // Layout Groupによる配置を確定させる
            LayoutRebuilder.ForceRebuildLayoutImmediate(_container);

            _slotPositions.Clear();
            _otherItems.Clear();

            var allItems = new List<RectTransform>();

            // 現在の見た目順にPitchItemを取得
            for (int i = 0; i < _container.childCount; i++)
            {
                Transform child = _container.GetChild(i);

                if (child.GetComponent<PitchItemView>() == null)
                    continue;

                if (child is RectTransform rect)
                    allItems.Add(rect);
            }

            // 現在の位置を「スロット」として保存
            foreach (RectTransform item in allItems)
            {
                _slotPositions.Add(item.anchoredPosition);
            }

            _targetIndex = allItems.IndexOf(_rect);

            foreach (RectTransform item in allItems)
            {
                if (item != _rect)
                    _otherItems.Add(item);
            }

            _dragY = _rect.anchoredPosition.y;
            _dragging = true;

            // ドラッグ中だけ自動レイアウトを停止
            _layoutGroup.enabled = false;
        }

        // --------------------------------------------------
        // ドラッグ中
        // --------------------------------------------------
        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging || _canvas == null)
                return;

            // 選択したカードだけ横方向に動かす
            _rect.anchoredPosition += new Vector2(
                eventData.delta.x / _canvas.scaleFactor,
                0f
            );

            // Y位置は固定
            _rect.anchoredPosition = new Vector2(
                _rect.anchoredPosition.x,
                _dragY
            );

            int newIndex = FindNearestSlotIndex();

            if (newIndex != _targetIndex)
            {
                _targetIndex = newIndex;

                // 他のカードを新しい位置へスッと移動
                AnimateOtherItems();
            }
        }

        // --------------------------------------------------
        // 現在一番近いスロットを探す
        // --------------------------------------------------
        private int FindNearestSlotIndex()
        {
            int nearestIndex = 0;
            float nearestDistance = float.MaxValue;

            for (int i = 0; i < _slotPositions.Count; i++)
            {
                float distance = Mathf.Abs(
                    _rect.anchoredPosition.x -
                    _slotPositions[i].x
                );

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }

        // --------------------------------------------------
        // ドラッグしていないカードを移動
        // --------------------------------------------------
        private void AnimateOtherItems()
        {
            for (int i = 0; i < _otherItems.Count; i++)
            {
                RectTransform item = _otherItems[i];

                // ドラッグ中のカードが入る場所を1個空ける
                int slotIndex =
                    i < _targetIndex
                        ? i
                        : i + 1;

                Vector2 targetPosition =
                    _slotPositions[slotIndex];

                StartMoveAnimation(
                    item,
                    targetPosition,
                    shiftDuration
                );
            }
        }

        // --------------------------------------------------
        // 移動Animation開始
        // --------------------------------------------------
        private void StartMoveAnimation(
            RectTransform item,
            Vector2 target,
            float duration)
        {
            if (_moveCoroutines.TryGetValue(item, out Coroutine current))
            {
                if (current != null)
                    StopCoroutine(current);
            }

            Coroutine routine =
                StartCoroutine(
                    MoveTo(item, target, duration)
                );

            _moveCoroutines[item] = routine;
        }

        // --------------------------------------------------
        // スッと移動するAnimation
        // --------------------------------------------------
        private IEnumerator MoveTo(
            RectTransform item,
            Vector2 target,
            float duration)
        {
            Vector2 start = item.anchoredPosition;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(
                    elapsed / duration
                );

                // Ease Out Cubic
                // 最初は速く、最後は柔らかく止まる
                float eased =
                    1f - Mathf.Pow(1f - t, 3f);

                item.anchoredPosition =
                    Vector2.LerpUnclamped(
                        start,
                        target,
                        eased
                    );

                yield return null;
            }

            item.anchoredPosition = target;

            _moveCoroutines.Remove(item);
        }

        // --------------------------------------------------
        // ドラッグ終了
        // --------------------------------------------------
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;

            _dragging = false;

            StartCoroutine(FinishDrop());
        }

        // --------------------------------------------------
        // 最終位置へ収める
        // --------------------------------------------------
        private IEnumerator FinishDrop()
        {
            // 掴んでいたカードも最後のスロットへスッと移動
            Vector2 start = _rect.anchoredPosition;
            Vector2 target = _slotPositions[_targetIndex];

            float elapsed = 0f;

            while (elapsed < dropDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(
                    elapsed / dropDuration
                );

                float eased =
                    1f - Mathf.Pow(1f - t, 3f);

                _rect.anchoredPosition =
                    Vector2.LerpUnclamped(
                        start,
                        target,
                        eased
                    );

                yield return null;
            }

            _rect.anchoredPosition = target;

            // 他の移動Coroutineが残っていれば終了
            foreach (Coroutine routine in _moveCoroutines.Values)
            {
                if (routine != null)
                    StopCoroutine(routine);
            }

            _moveCoroutines.Clear();

            // 最終的なHierarchy順を作る
            var finalOrder =
                new List<RectTransform>(_otherItems);

            finalOrder.Insert(
                _targetIndex,
                _rect
            );

            for (int i = 0; i < finalOrder.Count; i++)
            {
                finalOrder[i].SetSiblingIndex(i);
            }

            // 再びHorizontal Layout Groupに任せる
            if (_layoutGroup != null)
            {
                _layoutGroup.enabled = true;

                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    _container
                );
            }
            //並び替え完了後に正誤判定
            PitchItemView itemView = GetComponent<PitchItemView>();

            if (itemView != null)
            {
                itemView.NotifyOrderChanged();
                itemView.NotifyDragFinished();
            }
        }
    }
}