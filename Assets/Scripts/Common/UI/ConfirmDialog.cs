using System;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Common.UI
{
    /// <summary>
    /// 「はい / いいえ」の確認ダイアログ（logical-components §1.5 / NFR-05 誤操作防止）。
    /// 既定フォーカスは「いいえ」（危険側を選びにくくする）。ホームの終了確認や
    /// 将来の削除確認（U4）など横断で再利用するため Common.UI に配置する。
    /// 見た目（Sprite/配色/文言）は Sさん が調整可能（US-TECH-07）。
    /// </summary>
    public class ConfirmDialog : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text titleText;
        [SerializeField] private Text messageText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        private Action _onYes;
        private Action _onNo;

        private void Awake()
        {
            if (yesButton != null) yesButton.onClick.AddListener(HandleYes);
            if (noButton != null) noButton.onClick.AddListener(HandleNo);
            Hide();
        }

        private void OnDestroy()
        {
            if (yesButton != null) yesButton.onClick.RemoveListener(HandleYes);
            if (noButton != null) noButton.onClick.RemoveListener(HandleNo);
        }

        /// <summary>ダイアログを表示する。既定フォーカスは「いいえ」。</summary>
        public void Show(string title, string message, Action onYes, Action onNo = null)
        {
            _onYes = onYes;
            _onNo = onNo;

            if (titleText != null) titleText.text = title;
            if (messageText != null) messageText.text = message;
            if (root != null) root.SetActive(true);

            if (noButton != null) noButton.Select();
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        public bool IsOpen => root != null && root.activeSelf;

        private void HandleYes()
        {
            Hide();
            _onYes?.Invoke();
        }

        private void HandleNo()
        {
            Hide();
            _onNo?.Invoke();
        }
    }
}
