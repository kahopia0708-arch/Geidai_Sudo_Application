using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Geidai.Common.Game;
using Geidai.Common.UI;

namespace Geidai.Game1
{
    /// <summary>
    /// おたまじゃくし。タップで口を開けて音確認、右睡蓮へドラッグで答え合わせ。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ChoiceItemView : MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IPointerClickHandler
    {
        private const float DragSlopPixels = 14f;

        [Header("ドラッグ演出（つまんで動かす・尻尾を振る）")]
        [SerializeField] private Vector2 dragPivot = new Vector2(0.36f, 0.5f);
        [SerializeField] private float dragLiftPixels = 18f;
        [SerializeField] private float dragScaleMultiplier = 1.08f;
        [SerializeField] private float tailWagSpeed = 13f;
        [SerializeField] private float tailWagDegrees = 12f;
        [SerializeField] private float dragVelocityTilt = 0.1f;

        [SerializeField] private Button previewButton;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image choiceImage;

        private SoundMatchGameController _controller;
        private ChoiceSpec _spec;
        private int _index = -1;
        private Sprite _tadpoleClosed;
        private Sprite _tadpoleOpen;
        private Sprite _frogClosed;
        private bool _inputLocked;
        private bool _dragExceededSlop;
        private Vector2 _pointerDownScreen;

        private RectTransform _rt;
        private RectTransform _parentRt;
        private Canvas _canvas;
        private Vector2 _originalPos;
        private Vector2 _dragOffset;
        private bool _isFrogForm;
        private int _returnHomeGeneration;
        private bool _dragVisualActive;
        private Vector2 _basePivot;
        private Vector3 _baseScale;
        private float _baseZRotation;
        private Vector2 _lastDragLocal;
        private float _dragWagPhase;
        private float _dragVelocityX;

        public int Index => _index;
        public int Cents => _spec.cents;
        public Image ChoiceImage => choiceImage;

        private void Awake()
        {
            _rt = transform as RectTransform;
            CaptureBaseTransform();
        }

        private void Update()
        {
            if (!_dragVisualActive || _rt == null || _isFrogForm) return;
            _dragWagPhase += Time.unscaledDeltaTime * tailWagSpeed;
            float wag = Mathf.Sin(_dragWagPhase) * tailWagDegrees;
            wag += Mathf.Clamp(_dragVelocityX, -120f, 120f) * dragVelocityTilt;
            _rt.localEulerAngles = new Vector3(0f, 0f, _baseZRotation + wag);
            _dragVelocityX = Mathf.Lerp(_dragVelocityX, 0f, Time.unscaledDeltaTime * 8f);
        }

        private void CaptureBaseTransform()
        {
            if (_rt == null) return;
            _basePivot = _rt.pivot;
            _baseScale = _rt.localScale;
            _baseZRotation = _rt.localEulerAngles.z;
        }

        private void OnEnable()
        {
            InstrumentFloorShadow.Ensure(_rt != null ? _rt : transform as RectTransform);
        }

        public void Setup(
            SoundMatchGameController controller,
            ChoiceSpec spec,
            int index,
            Sprite tadpoleSprite,
            Sprite frogSprite,
            Sprite tadpoleOpenSprite)
        {
            _controller = controller;
            _spec = spec;
            _index = index;
            _tadpoleClosed = tadpoleSprite;
            _frogClosed = frogSprite;
            _tadpoleOpen = tadpoleOpenSprite;
            _isFrogForm = false;

            if (_rt == null) _rt = transform as RectTransform;
            if (choiceImage == null) choiceImage = GetComponent<Image>();
            Game1CharacterImage.Apply(choiceImage);
            SetAsTadpole(_tadpoleClosed);
            _parentRt = _rt != null ? _rt.parent as RectTransform : null;
            if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
            EndDragVisualImmediate();
            if (previewButton != null)
                previewButton.gameObject.SetActive(false);
        }

        /// <summary>池の初期配置へ戻し、<see cref="_originalPos"/> を更新する。</summary>
        public void ApplyPoolHomeLayout(int slotIndex)
        {
            if (_rt == null) return;
            CancelReturnHomeAnimation();
            EndDragVisualImmediate();

            float x = slotIndex >= 0 && slotIndex < Game1StoryboardLayout.ChoicePoolX.Length
                ? Game1StoryboardLayout.ChoicePoolX[slotIndex]
                : 0f;
            _rt.anchorMin = new Vector2(0.5f, Game1StoryboardLayout.ChoicePoolAnchorY);
            _rt.anchorMax = new Vector2(0.5f, Game1StoryboardLayout.ChoicePoolAnchorY);
            _rt.sizeDelta = Game1StoryboardLayout.ChoiceSize;
            _rt.anchoredPosition = new Vector2(x, 0f);
            _rt.localRotation = Quaternion.identity;
            _originalPos = _rt.anchoredPosition;
            CaptureBaseTransform();
        }

        public void SetAsTadpole(Sprite tadpoleSprite)
        {
            if (tadpoleSprite != null) _tadpoleClosed = tadpoleSprite;
            _isFrogForm = false;
            Game1MouthVisual.SetClosed(choiceImage, _tadpoleClosed);
        }

        public void EvolveToFrog()
        {
            _isFrogForm = true;
            Game1MouthVisual.SetClosed(choiceImage, _frogClosed != null ? _frogClosed : _tadpoleClosed);
        }

        public void SetInputLocked(bool locked)
        {
            _inputLocked = locked;
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = !locked;
                canvasGroup.interactable = !locked;
            }
        }

        public void OpenMouth()
        {
            if (_isFrogForm) return;
            Game1MouthVisual.SetOpen(choiceImage, _tadpoleOpen, _tadpoleClosed);
        }

        public void CloseMouth()
        {
            if (_isFrogForm)
                Game1MouthVisual.SetClosed(choiceImage, _frogClosed != null ? _frogClosed : _tadpoleClosed);
            else
                Game1MouthVisual.SetClosed(choiceImage, _tadpoleClosed);
        }

        public void CancelReturnHomeAnimation()
        {
            _returnHomeGeneration++;
        }

        public void PlayCelebratePulse()
        {
            StartCoroutine(CelebratePulseRoutine());
        }

        /// <summary>正解演出：拡大→戻し。コルーチン完了まで成長カエルを見せる。</summary>
        public IEnumerator RunCelebratePulse()
        {
            yield return CelebratePulseRoutine();
        }

        public IEnumerator AnimateReturnHome(float durationSeconds)
        {
            if (_rt == null) yield break;
            EndDragVisualImmediate();
            int generation = ++_returnHomeGeneration;
            float duration = Mathf.Max(0.12f, durationSeconds);
            Vector2 start = _rt.anchoredPosition;
            Vector2 end = _originalPos;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (generation != _returnHomeGeneration) yield break;
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t);
                _rt.anchoredPosition = Vector2.Lerp(start, end, t);
                yield return null;
            }

            if (generation != _returnHomeGeneration) yield break;
            _rt.anchoredPosition = end;
        }

        private IEnumerator CelebratePulseRoutine()
        {
            if (_rt == null) yield break;
            Vector3 baseScale = _rt.localScale;
            const float up = 1.12f;
            const float half = 0.12f;
            float t = 0f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / half);
                _rt.localScale = baseScale * Mathf.Lerp(1f, up, u);
                yield return null;
            }

            t = 0f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / half);
                _rt.localScale = baseScale * Mathf.Lerp(up, 1f, u);
                yield return null;
            }

            _rt.localScale = baseScale;
        }

        public IEnumerator AnimateMouthWhile(IEnumerator whilePlaying)
        {
            if (_isFrogForm)
            {
                yield return whilePlaying;
                yield break;
            }

            yield return Game1MouthVisual.HoldOpenWhile(
                choiceImage,
                _tadpoleClosed,
                _tadpoleOpen,
                whilePlaying);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_inputLocked) return;
            CancelReturnHomeAnimation();
            EndDragVisualImmediate();
            _dragExceededSlop = false;
            _pointerDownScreen = eventData.position;
            _dragVelocityX = 0f;
            _dragWagPhase = 0f;
            if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
            if (_rt != null) _rt.SetAsLastSibling();

            _dragOffset = Vector2.zero;
            _lastDragLocal = _rt != null ? _rt.anchoredPosition : Vector2.zero;
            if (_rt != null && _parentRt != null &&
                TryPointerInParent(eventData, out Vector2 local))
            {
                _dragOffset = local - _rt.anchoredPosition;
                _lastDragLocal = local;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_inputLocked || _rt == null || _parentRt == null) return;
            if (!_dragExceededSlop &&
                Vector2.Distance(_pointerDownScreen, eventData.position) >= DragSlopPixels)
            {
                _dragExceededSlop = true;
                BeginDragVisual();
            }

            if (!TryPointerInParent(eventData, out Vector2 local)) return;

            float dt = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
            _dragVelocityX = (local.x - _lastDragLocal.x) / dt;
            _lastDragLocal = local;

            Vector2 target = local - _dragOffset;
            if (_dragVisualActive && !_isFrogForm)
                target += new Vector2(0f, dragLiftPixels);
            _rt.anchoredPosition = target;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            EndDragVisualImmediate();
            if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
            if (_inputLocked)
            {
                ResetPosition();
                return;
            }

            if (_controller == null)
            {
                ResetPosition();
                return;
            }

            if (_dragExceededSlop)
                _controller.OnChoiceDropped(this, eventData.position);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_inputLocked || _controller == null) return;
            _controller.OnChoiceTapped(this);
        }

        public void ResetPosition()
        {
            EndDragVisualImmediate();
            if (_rt != null) _rt.anchoredPosition = _originalPos;
        }

        public void SnapOverAnchor(RectTransform anchor, RectTransform playArea, Camera eventCamera)
        {
            EndDragVisualImmediate();
            if (_rt == null || anchor == null) return;
            _rt.position = anchor.position;
        }

        private void BeginDragVisual()
        {
            if (_rt == null || _isFrogForm || _dragVisualActive) return;
            _dragVisualActive = true;
            SetPivotKeepPosition(dragPivot);
            _rt.localScale = _baseScale * dragScaleMultiplier;
        }

        private void EndDragVisualImmediate()
        {
            if (_rt == null)
            {
                _dragVisualActive = false;
                return;
            }

            if (_dragVisualActive)
                SetPivotKeepPosition(_basePivot);

            _dragVisualActive = false;
            _rt.localScale = _baseScale;
            _rt.localEulerAngles = new Vector3(0f, 0f, _baseZRotation);
            _dragVelocityX = 0f;
        }

        private void SetPivotKeepPosition(Vector2 newPivot)
        {
            if (_rt == null) return;
            Vector2 deltaPivot = newPivot - _rt.pivot;
            Vector2 size = _rt.rect.size;
            _rt.pivot = newPivot;
            _rt.anchoredPosition += new Vector2(deltaPivot.x * size.x, deltaPivot.y * size.y);
        }

        private bool TryPointerInParent(PointerEventData eventData, out Vector2 local)
        {
            Camera cam = _canvas != null ? _canvas.worldCamera : null;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentRt, eventData.position, cam, out local);
        }
    }
}
