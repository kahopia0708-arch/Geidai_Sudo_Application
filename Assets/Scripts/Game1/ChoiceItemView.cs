using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Geidai.Common.Game;
using Geidai.Common.UI;

namespace Geidai.Game1
{
    /// <summary>
    /// 図鑑楽器（選択肢）。タップで音確認、弟子へドラッグで答え合わせ。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ChoiceItemView : MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IPointerClickHandler
    {
        private const float DragSlopPixels = 14f;

        [Header("ドラッグ演出")]
        [SerializeField] private Vector2 dragPivot = new Vector2(0.5f, 0.5f);
        [SerializeField] private float dragLiftPixels = 12f;
        [SerializeField] private float dragScaleMultiplier = 1.06f;

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
        private RectTransform _poolParent;
        private Canvas _canvas;
        private Vector2 _originalPos;
        private Vector2 _dragOffset;
        private bool _isHeldOnApprentice;
        private bool _useInstrumentFeel = true;
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

        private void OnEnable()
        {
            InstrumentFloorShadow.Ensure(_rt != null ? _rt : transform as RectTransform);
        }

        private void CaptureBaseTransform()
        {
            if (_rt == null) return;
            _basePivot = _rt.pivot;
            _baseScale = _rt.localScale;
            _baseZRotation = _rt.localEulerAngles.z;
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
            _isHeldOnApprentice = false;

            if (_rt == null) _rt = transform as RectTransform;
            if (choiceImage == null) choiceImage = GetComponent<Image>();
            BindChoicePoolParent(controller);
            Game1CharacterImage.Apply(choiceImage);
            SetInstrument(tadpoleSprite);
            if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
            EndDragVisualImmediate();
            if (previewButton != null)
                previewButton.gameObject.SetActive(false);
        }

        /// <summary>池の初期配置へ戻し、<see cref="_originalPos"/> を更新する。</summary>
        public void ApplyPoolHomeLayout(int slotIndex, int choiceCount = 3)
        {
            if (_rt == null) return;
            BindChoicePoolParent(_controller);
            CancelReturnHomeAnimation();
            EndDragVisualImmediate();
            _isHeldOnApprentice = false;
            SetInputLocked(false);
            RestoreCanvasGroupForInteraction();
            ReparentToPool();
            ApplyPoolAnchorSizing();

            float x = Game1StoryboardLayout.ChoiceX(slotIndex, choiceCount);
            _rt.anchoredPosition = new Vector2(x, 0f);
            _rt.localRotation = Quaternion.identity;
            _originalPos = _rt.anchoredPosition;
            CaptureBaseTransform();
            SetInstrumentShadowVisible(true);
        }

        public void ApplyPoolHomeLayout(int slotIndex)
        {
            ApplyPoolHomeLayout(slotIndex, 3);
        }

        public void SetInstrument(Sprite instrumentSprite)
        {
            if (_controller != null && _controller.Config != null)
            {
                instrumentSprite = Game1LegacyTadpoleMotif.ResolveInstrumentSprite(
                    _controller.Config,
                    instrumentSprite,
                    Game1LegacyTadpoleMotif.FirstCatalogInstrumentSprite(_controller.Config));
            }

            if (instrumentSprite != null) _tadpoleClosed = instrumentSprite;
            _isHeldOnApprentice = false;
            Game1MouthVisual.SetClosed(choiceImage, _tadpoleClosed);
        }

        public void SetAsTadpole(Sprite tadpoleSprite)
        {
            SetInstrument(tadpoleSprite);
        }

        public void EvolveToFrog()
        {
            ConfirmHeldMatch();
        }

        public void ConfirmHeldMatch()
        {
            _isHeldOnApprentice = true;
            if (choiceImage != null && _tadpoleClosed != null)
                Game1MouthVisual.SetClosed(choiceImage, _tadpoleClosed);
            SetInstrumentShadowVisible(false);
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
            if (_isHeldOnApprentice || _tadpoleOpen == null) return;
            Game1MouthVisual.SetOpen(choiceImage, _tadpoleOpen, _tadpoleClosed);
        }

        public void CloseMouth()
        {
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
            _isHeldOnApprentice = false;
            BindChoicePoolParent(_controller);
            EndDragVisualImmediate();
            ReparentToPool();
            if (_rt.parent == _poolParent)
            {
                _rt.anchorMin = new Vector2(0.5f, Game1StoryboardLayout.ChoicePoolAnchorY);
                _rt.anchorMax = new Vector2(0.5f, Game1StoryboardLayout.ChoicePoolAnchorY);
                _rt.sizeDelta = Game1StoryboardLayout.ChoiceSize;
            }

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
            SetInstrumentShadowVisible(true);
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
            if (_isHeldOnApprentice)
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
            SetInstrumentShadowVisible(false);
            _dragExceededSlop = false;
            _pointerDownScreen = eventData.position;
            _dragVelocityX = 0f;
            _dragWagPhase = 0f;
            if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
            if (_rt != null) _rt.SetAsLastSibling();

            _dragOffset = Vector2.zero;
            _lastDragLocal = _rt != null ? _rt.anchoredPosition : Vector2.zero;
            if (_rt != null && _poolParent != null &&
                TryPointerInParent(eventData, out Vector2 local))
            {
                _dragOffset = local - _rt.anchoredPosition;
                _lastDragLocal = local;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_inputLocked || _rt == null || _poolParent == null) return;
            if (!_dragExceededSlop &&
                Vector2.Distance(_pointerDownScreen, eventData.position) >= DragSlopPixels)
            {
                _dragExceededSlop = true;
                BeginDragVisual();
            }

            if (!TryPointerInParent(eventData, out Vector2 local)) return;

            _lastDragLocal = local;
            Vector2 target = local - _dragOffset;
            if (_dragVisualActive)
                target += new Vector2(0f, dragLiftPixels);
            _rt.anchoredPosition = target;
        }

        public IEnumerator AnimateMagnetSnap(RectTransform anchor, float durationSeconds)
        {
            EndDragVisualImmediate();
            if (_rt == null || anchor == null) yield break;
            Vector3 start = _rt.position;
            Vector3 end = anchor.position;
            float duration = Mathf.Max(0.08f, durationSeconds);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t);
                _rt.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            _rt.position = end;
            AttachToSnapAnchor(anchor);
        }

        /// <summary>弟子の手元スナップに親子付け（ローカル原点＝手の位置）。</summary>
        public void AttachToSnapAnchor(RectTransform anchor)
        {
            if (_rt == null || anchor == null) return;
            EndDragVisualImmediate();
            _rt.SetParent(anchor, false);
            _rt.anchorMin = new Vector2(0.5f, 0.5f);
            _rt.anchorMax = new Vector2(0.5f, 0.5f);
            _rt.pivot = new Vector2(0.5f, 0.5f);
            _rt.anchoredPosition = Vector2.zero;
            _rt.localRotation = Quaternion.identity;
            _rt.localScale = _baseScale;
            SetInstrumentShadowVisible(false);
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

            if (_isHeldOnApprentice)
            {
                ResetPosition();
                return;
            }

            if (!_dragExceededSlop)
            {
                SetInstrumentShadowVisible(true);
                return;
            }

            _controller.OnChoiceDropped(this, eventData.position);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_inputLocked || _controller == null) return;
            _controller.OnChoiceTapped(this);
        }

        /// <summary>正解後の次の問題へ進む前に、弟子スナップから池へ戻す。</summary>
        public void PrepareForPoolHome(RectTransform poolParent)
        {
            _isHeldOnApprentice = false;
            if (poolParent != null)
                _poolParent = poolParent;
            CancelReturnHomeAnimation();
            EndDragVisualImmediate();
            SetInputLocked(false);
            RestoreCanvasGroupForInteraction();
            ReparentToPool();
            ApplyPoolAnchorSizing();
        }

        /// <summary>全問終了時：池へ戻し、操作を止めて非表示。</summary>
        public void FinalizeForSessionEnd(RectTransform poolParent)
        {
            PrepareForPoolHome(poolParent);
            SetInstrumentShadowVisible(false);
            gameObject.SetActive(false);
        }

        public void ResetPosition()
        {
            _isHeldOnApprentice = false;
            EndDragVisualImmediate();
            BindChoicePoolParent(_controller);
            ReparentToPool();
            if (_rt != null && _poolParent != null && _rt.parent == _poolParent)
            {
                _rt.anchorMin = new Vector2(0.5f, Game1StoryboardLayout.ChoicePoolAnchorY);
                _rt.anchorMax = new Vector2(0.5f, Game1StoryboardLayout.ChoicePoolAnchorY);
                _rt.sizeDelta = Game1StoryboardLayout.ChoiceSize;
                _rt.anchoredPosition = _originalPos;
            }
            else if (_rt != null)
            {
                _rt.anchoredPosition = _originalPos;
            }

            SetInstrumentShadowVisible(true);
        }

        public void SnapOverAnchor(RectTransform anchor, RectTransform playArea, Camera eventCamera)
        {
            AttachToSnapAnchor(anchor);
        }

        private void ReparentToPool()
        {
            if (_rt == null || _poolParent == null) return;
            if (_rt.parent == _poolParent) return;
            _rt.SetParent(_poolParent, false);
        }

        private void BindChoicePoolParent(SoundMatchGameController controller)
        {
            if (controller != null && controller.ChoicePoolParent != null)
                _poolParent = controller.ChoicePoolParent;
            else if (_poolParent == null && _rt != null)
                _poolParent = _rt.parent as RectTransform;
        }

        private void ApplyPoolAnchorSizing()
        {
            if (_rt == null || _poolParent == null || _rt.parent != _poolParent) return;
            _rt.anchorMin = new Vector2(0.5f, Game1StoryboardLayout.ChoicePoolAnchorY);
            _rt.anchorMax = new Vector2(0.5f, Game1StoryboardLayout.ChoicePoolAnchorY);
            _rt.pivot = new Vector2(0.5f, 0.5f);
            _rt.sizeDelta = Game1StoryboardLayout.ChoiceSize;
            _rt.localScale = _baseScale;
        }

        private void RestoreCanvasGroupForInteraction()
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        private void SetInstrumentShadowVisible(bool visible)
        {
            InstrumentFloorShadow.SetVisible(_rt != null ? _rt : transform as RectTransform, visible);
        }

        private void BeginDragVisual()
        {
            if (_rt == null || _isHeldOnApprentice || _dragVisualActive) return;
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
                _poolParent, eventData.position, cam, out local);
        }
    }
}
