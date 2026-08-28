using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Foundation
{
    /// <summary>
    /// ホーム右上のニックネームバッジ（装飾プログレス付き）。
    /// </summary>
    public class HomeProfileBadgeView : MonoBehaviour
    {
        [SerializeField] private Image badgeBackground;
        [SerializeField] private Button button;
        [SerializeField] private Text nicknameText;
        [SerializeField] private Transform progressSegmentsRoot;

        public Button Button => button;

        private void Awake()
        {
            if (nicknameText != null) UiFontResolver.ApplyTo(nicknameText, 28);
        }

        public void ApplyChrome(Sprite pillSprite)
        {
            var bg = badgeBackground != null ? badgeBackground : GetComponent<Image>();
            HomeUiImageUtil.ApplyBackground(bg, pillSprite, Color.white);
        }

        public void SetNickname(string nickname)
        {
            if (nicknameText != null)
                nicknameText.text = string.IsNullOrEmpty(nickname) ? string.Empty : nickname;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        /// <summary>装飾のみ — 固定セグメント表示（Q4-C）。</summary>
        public void ShowDecorativeProgress()
        {
            if (progressSegmentsRoot == null) return;
            progressSegmentsRoot.gameObject.SetActive(true);
        }
    }
}
