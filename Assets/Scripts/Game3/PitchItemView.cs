using System.Collections;
using Geidai.Common.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Game3
{
    [RequireComponent(typeof(Button))]
    public class PitchItemView : MonoBehaviour
    {
        private Game3Controller _controller;
        private int _cents;
        private Button _button;

        private Image _image;
        private Sprite _defaultSprite;
        private Color _defaultColor;

       
        private bool _suppressClick;

        public int Cents => _cents;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _image = GetComponent<Image>();

            if (_image != null)
            {
                _defaultSprite = _image.sprite;
                _defaultColor = _image.color;
            }

            _button.onClick.AddListener(OnClicked);
        }

            private void OnEnable()
        {
            InstrumentFloorShadow.Ensure(transform as RectTransform);
        }

        public void Setup(Game3Controller controller, int cents, Sprite soundImage)
        {
            _controller = controller;
            _cents = cents;

            if (_image != null)
            {
                if (soundImage != null)
                {
                    _image.sprite = soundImage;
                    _image.color = Color.white;
                    _image.preserveAspect = true;
                }
                else
                {
                    _image.sprite = _defaultSprite;
                    _image.color = _defaultColor;
                }
            }
            SetShadowVisible(true);
        }

      
        public void NotifyDragStarted()
        {
            if (_controller != null)
            {
                _controller.PreviewPitch(_cents);
            }
            _suppressClick = true;
        }

       
        public void NotifyDragFinished()
        {
            StartCoroutine(EnableClickNextFrame());
        }

        private IEnumerator EnableClickNextFrame()
        {
            yield return null;
            _suppressClick = false;
        }

        private void OnClicked()
        {
           
            if (_suppressClick)
                return;

            if (_controller == null)
                return;

            _controller.PreviewPitch(_cents);
        }
        public void SetShadowVisible(bool visible)
        {
            RectTransform rect = transform as RectTransform;
            if (rect == null) return;

            InstrumentFloorShadow.Ensure(rect);
            InstrumentFloorShadow.SetVisible(rect, visible);
        }
        public void NotifyOrderChanged()
        {
            if (_controller == null)
                return;

            _controller.CheckCurrentOrder();
        }
    }
}