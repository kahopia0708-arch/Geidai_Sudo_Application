using System.Collections;
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

        // ドラッグ直後のButtonクリックを無視するため
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
        public void Setup(Game3Controller controller,int cents,Sprite soundImage)
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
        }

        /// <summary>
        /// ドラッグが始まったことを通知する。
        /// </summary>
        public void NotifyDragStarted()
        {
            if (_controller != null)
            {
                _controller.PreviewPitch(_cents);
            }
            _suppressClick = true;
        }

        /// <summary>
        /// ドラッグ処理が完全に終わった後、
        /// 次のフレームから再びクリックを許可する。
        /// </summary>
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
            // ドラッグによって発生したクリックなら音を鳴らさない
            if (_suppressClick)
                return;

            if (_controller == null)
                return;

            _controller.PreviewPitch(_cents);
        }

        public void NotifyOrderChanged()
        {
            if (_controller == null)
                return;

            _controller.CheckCurrentOrder();
        }
    }
}