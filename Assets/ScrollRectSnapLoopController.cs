using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollRectSnapLoop : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Header("References")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;

    [Header("Items")]
    [SerializeField] private RectTransform[] items;

    [Header("Snap Settings")]
    [SerializeField] private float snapSpeed = 12f;
    [SerializeField] private float swipeThreshold = 30f;

    [Header("Layout Settings")]
    [SerializeField] private float itemWidth = 850f;
    [SerializeField] private float spacing = 20f;
    [SerializeField] private float paddingLeft = 20f;

    private int currentIndex = 0;
    private Vector2 targetPosition;
    private bool isSnapping = false;
    private Vector2 dragStartPos;

    private void Reset()
    {
        scrollRect = GetComponent<ScrollRect>();
    }

    private void Awake()
    {
        if (scrollRect == null)
        {
            scrollRect = GetComponent<ScrollRect>();
        }

        if (content == null && scrollRect != null)
        {
            content = scrollRect.content;
        }

        if (scrollRect != null)
        {
            scrollRect.inertia = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
        }

        SnapToIndex(currentIndex, false);
    }

    private void Update()
    {
        if (content == null || items == null || items.Length == 0)
            return;

        if (isSnapping)
        {
            content.anchoredPosition = Vector2.Lerp(
                content.anchoredPosition,
                targetPosition,
                Time.deltaTime * snapSpeed
            );

            if (Vector2.Distance(content.anchoredPosition, targetPosition) < 0.1f)
            {
                content.anchoredPosition = targetPosition;
                isSnapping = false;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragStartPos = eventData.position;
        isSnapping = false;

        if (scrollRect != null)
        {
            scrollRect.StopMovement();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (items == null || items.Length == 0)
            return;

        float deltaX = eventData.position.x - dragStartPos.x;

        if (Mathf.Abs(deltaX) < swipeThreshold)
        {
            SnapToCurrent();
            return;
        }

        if (deltaX < 0f)
        {
            // 左スワイプ -> 次へ
            currentIndex = (currentIndex + 1) % items.Length;
        }
        else
        {
            // 右スワイプ -> 前へ
            currentIndex = (currentIndex - 1 + items.Length) % items.Length;
        }

        SnapToCurrent();
    }

    public void SnapToCurrent()
    {
        SnapToIndex(currentIndex, true);
    }

    private void SnapToIndex(int index, bool animate)
    {
        if (content == null || items == null || items.Length == 0)
            return;

        currentIndex = (index % items.Length + items.Length) % items.Length;
        targetPosition = GetTargetPosition(currentIndex);

        if (animate)
        {
            isSnapping = true;
        }
        else
        {
            content.anchoredPosition = targetPosition;
            isSnapping = false;
        }
    }

    private Vector2 GetTargetPosition(int index)
    {
        float step = itemWidth + spacing;
        float x = -(paddingLeft + step * index);
        return new Vector2(x, content.anchoredPosition.y);
    }
}