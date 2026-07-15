using System;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Collection
{
    /// <summary>
    /// 一覧の1項目ビュー（frontend-components §3）。
    /// <see cref="SoundItemViewModel"/> を受けてタイトル/日付/サムネ有無/操作ボタンを描画する。
    /// 実際のレイアウト・見た目は S さんが調整（US-TECH-07）。相対レイアウト前提（NFR-COL-P1）。
    /// </summary>
    public class SoundListItemView : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text dateText;
        [SerializeField] private Image thumbImage;
        [SerializeField] private GameObject noPhotoPlaceholder;
        [SerializeField] private Button openButton;
        [SerializeField] private Button playButton;

        private string _id;
        private Action<string> _onOpen;
        private Action<string> _onPlay;
        private bool _hooked;

        public string Id => _id;

        public void Bind(SoundItemViewModel vm, Action<string> onOpen, Action<string> onPlay)
        {
            _id = vm != null ? vm.id : null;
            _onOpen = onOpen;
            _onPlay = onPlay;

            if (titleText != null) titleText.text = vm != null ? vm.displayTitle : string.Empty;
            if (dateText != null) dateText.text = vm != null ? SoundItemViewModel.FormatDate(vm.createdAtIso) : string.Empty;

            // サムネは既定で非表示（遅延読み込みで後から差し込む / NFR-COL-P1）。
            if (thumbImage != null)
            {
                thumbImage.sprite = null;
                thumbImage.enabled = false;
            }
            if (noPhotoPlaceholder != null)
                noPhotoPlaceholder.SetActive(vm == null || !vm.hasPhoto);

            HookOnce();
        }

        /// <summary>遅延読み込んだサムネを差し込む（null なら placeholder のまま）。</summary>
        public void SetThumbnail(Sprite sprite)
        {
            if (thumbImage == null) return;
            if (sprite == null)
            {
                thumbImage.enabled = false;
                if (noPhotoPlaceholder != null) noPhotoPlaceholder.SetActive(true);
                return;
            }
            thumbImage.sprite = sprite;
            thumbImage.enabled = true;
            if (noPhotoPlaceholder != null) noPhotoPlaceholder.SetActive(false);
        }

        private void HookOnce()
        {
            if (_hooked) return;
            if (openButton != null) openButton.onClick.AddListener(() => _onOpen?.Invoke(_id));
            if (playButton != null) playButton.onClick.AddListener(() => _onPlay?.Invoke(_id));
            _hooked = true;
        }
    }
}
