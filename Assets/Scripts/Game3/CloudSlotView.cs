
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Geidai.Game3
{
    public class CloudSlotView : MonoBehaviour, IDropHandler
    {
        private PitchItemView _occupant;

        public PitchItemView Occupant => _occupant;
        public bool IsEmpty => _occupant == null;

        [Header("Cloud Bounce")]
        [SerializeField] private float bounceDuration = 0.35f;
        [SerializeField] private float squashAmount = 0.18f;
        [SerializeField] private float stretchAmount = 0.12f;

        private Vector3 _baseScale;
        private Coroutine _bounceCoroutine;

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        private void OnDisable()
        {
            if (_bounceCoroutine != null)
            {
                StopCoroutine(_bounceCoroutine);
                _bounceCoroutine = null;
            }

            transform.localScale = _baseScale;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
                return;

            PitchItemDragHandler dragHandler =
                eventData.pointerDrag.GetComponent<PitchItemDragHandler>();

            if (dragHandler == null)
                return;

            dragHandler.TryPlaceInSlot(this);
        }

        public bool CanAccept(PitchItemView item)
        {
            return _occupant == null || _occupant == item;
        }

        public void Accept(PitchItemView item)
        {
            _occupant = item;

            
        }

        public void Release(PitchItemView item)
        {
            if (_occupant == item)
                _occupant = null;
        }

        public void PlayBounce()
        {
            if (_bounceCoroutine != null)
                StopCoroutine(_bounceCoroutine);

            _bounceCoroutine = StartCoroutine(BounceCoroutine());
        }

        private IEnumerator BounceCoroutine()
        {
            float duration = Mathf.Max(0.01f, bounceDuration);

            Vector3 squash = Vector3.Scale(
                _baseScale,
                new Vector3(
                    1f + squashAmount,
                    1f - squashAmount,
                    1f
                )
            );

            Vector3 stretch = Vector3.Scale(
                _baseScale,
                new Vector3(
                    1f - stretchAmount,
                    1f + stretchAmount,
                    1f
                )
            );

            yield return ScaleTo(squash, duration * 0.3f);
            yield return ScaleTo(stretch, duration * 0.35f);
            yield return ScaleTo(_baseScale, duration * 0.35f);

            transform.localScale = _baseScale;
            _bounceCoroutine = null;
        }

        private IEnumerator ScaleTo(Vector3 target, float duration)
        {
            Vector3 start = transform.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t);

                transform.localScale =
                    Vector3.LerpUnclamped(start, target, t);

                yield return null;
            }

            transform.localScale = target;
        }
    }
}
