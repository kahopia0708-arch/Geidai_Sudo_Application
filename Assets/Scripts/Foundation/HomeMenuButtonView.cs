using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Foundation
{
    /// <summary>
    /// ホームメニュー1行（白角丸・左アイコン・右ラベル）の見た目バインディング。
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class HomeMenuButtonView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Text labelText;
        [SerializeField] private GameObject iconRoot;

        public Button Button => _button != null ? _button : (_button = GetComponent<Button>());

        private Button _button;
        private Image _background;

        private void Awake()
        {
            _background = GetComponent<Image>();
            if (labelText != null) UiFontResolver.ApplyTo(labelText, 48);
        }

        public void ApplyChrome(Sprite pillSprite)
        {
            HomeUiImageUtil.ApplyBackground(_background ?? GetComponent<Image>(), pillSprite, Color.white);
        }

        public void Apply(string label, Sprite icon)
        {
            if (labelText != null)
            {
                UiFontResolver.ApplyTo(labelText, 48);
                labelText.text = label ?? string.Empty;
            }

            bool hasIcon = icon != null;
            if (iconRoot != null) iconRoot.SetActive(hasIcon);
            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = hasIcon;
            }
        }
    }
}
