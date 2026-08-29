using System;
using Geidai.Common.Library;
using Geidai.Foundation;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Library
{
    /// <summary>
    /// 音図鑑グリッドの1セル。サムネイルタップで選択（試聴は詳細側）。
    /// ロック時はシルエット風に暗く表示。
    /// </summary>
    public class CuratedSoundItemView : MonoBehaviour
    {
        [SerializeField] private Button selectButton;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image frameImage;
        [SerializeField] private Text numberLabel;
        [SerializeField] private Text nameLabel;
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private Sprite placeholderSprite;

        private LibraryItemView _item;
        private Action<LibraryItemView> _onSelect;

        public void Bind(
            LibraryItemView item,
            Action<LibraryItemView> onPlay,
            Action<LibraryItemView> onSelect = null,
            Sprite placeholder = null)
        {
            // onPlay はグリッドでは使わない（詳細の画像タップで再生）。シグネチャ互換のため残す。
            _item = item;
            _onSelect = onSelect;
            if (placeholder != null) placeholderSprite = placeholder;

            if (numberLabel != null)
            {
                numberLabel.text = item.encyclopediaNumber > 0 ? item.encyclopediaNumber.ToString("000") : string.Empty;
                numberLabel.color = HomeUiTheme.MenuText;
            }

            if (nameLabel != null)
            {
                nameLabel.text = item.displayName ?? string.Empty;
                nameLabel.color = HomeUiTheme.MenuText;
            }

            if (iconImage != null)
            {
                iconImage.sprite = item.image != null ? item.image : placeholderSprite;
                iconImage.enabled = iconImage.sprite != null;
                iconImage.preserveAspect = true;
                // 未解除: シルエット風。解除済み: そのまま
                iconImage.color = item.isUnlocked
                    ? Color.white
                    : new Color(0.12f, 0.14f, 0.18f, 1f);
            }

            if (frameImage != null)
                HomeUiImageUtil.ApplyPillFill(frameImage, HomeUiTheme.PanelFill);

            if (lockOverlay != null)
                lockOverlay.SetActive(!item.isUnlocked);

            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(OnSelectClicked);
            }
        }

        private void OnSelectClicked() => _onSelect?.Invoke(_item);
    }
}
