using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Geidai.Game1
{
    /// <summary>
    /// 左の睡蓮上のお手本カエル。タップで口を開けてお手本の音を出す。
    /// </summary>
    public class FrogTargetView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Button previewButton;
        [SerializeField] private Image targetImage;

        private SoundMatchGameController _controller;
        private Sprite _mouthClosed;
        private Sprite _mouthOpen;

        public Image TargetImage => targetImage;

        public void Setup(
            SoundMatchGameController controller,
            Sprite frogSprite,
            Sprite frogOpenSprite)
        {
            _controller = controller;
            _mouthClosed = frogSprite;
            _mouthOpen = frogOpenSprite;
            if (targetImage == null) targetImage = GetComponent<Image>();

            Game1MouthVisual.SetClosed(targetImage, _mouthClosed);
            Game1CharacterImage.Apply(targetImage);

            if (previewButton != null)
            {
                previewButton.gameObject.SetActive(false);
            }
        }

        public void OpenMouth()
        {
            Game1MouthVisual.SetOpen(targetImage, _mouthOpen, _mouthClosed);
        }

        public void CloseMouth()
        {
            Game1MouthVisual.SetClosed(targetImage, _mouthClosed);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_controller == null) return;
            _controller.OnDemoFrogTapped(this);
        }

        public IEnumerator AnimateMouthWhile(IEnumerator whilePlaying)
        {
            return Game1MouthVisual.HoldOpenWhile(
                targetImage,
                _mouthClosed,
                _mouthOpen,
                whilePlaying);
        }
    }
}
