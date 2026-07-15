using UnityEngine;
using UnityEngine.UI;
using Geidai.Common.Results;

namespace Geidai.Common.UI
{
    /// <summary>
    /// 子ども向けの平易なエラー/警告通知（バナー / BR-16, BR-19）。
    /// アイコン＋平易文言で提示し、フォールバック時は警告表示する。
    /// 見た目（Sprite/文言/配色）は Sさん が調整可能（US-TECH-07）。
    /// </summary>
    public class ErrorPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject banner;
        [SerializeField] private Text messageText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Sprite errorIcon;
        [SerializeField] private Sprite warningIcon;

        /// <summary>エラーを平易文言で提示する。</summary>
        public void ShowError(string childFriendlyMessage)
        {
            ShowBanner(childFriendlyMessage, errorIcon);
        }

        /// <summary>フォールバック等の警告を提示する（BR-19）。</summary>
        public void ShowWarning(string childFriendlyMessage)
        {
            ShowBanner(childFriendlyMessage, warningIcon);
            Debug.LogWarning(childFriendlyMessage);
        }

        /// <summary>失敗 Result を受けて通知する（成功時は何もしない）。</summary>
        public void ShowFromResult(Result result)
        {
            if (result.IsSuccess) return;
            ShowError(string.IsNullOrEmpty(result.Message) ? "うまくいかなかったよ" : result.Message);
        }

        public void Hide()
        {
            if (banner != null) banner.SetActive(false);
        }

        private void ShowBanner(string message, Sprite icon)
        {
            if (messageText != null) messageText.text = message;
            if (iconImage != null && icon != null) iconImage.sprite = icon;
            if (banner != null) banner.SetActive(true);
        }
    }
}
