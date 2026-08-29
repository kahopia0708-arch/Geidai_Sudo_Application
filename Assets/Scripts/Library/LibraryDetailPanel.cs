using Geidai.Common.Library;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Library
{
    /// <summary>選択中の音の説明パネル（U7b）。</summary>
    public class LibraryDetailPanel : MonoBehaviour
    {
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text descriptionLabel;
        [SerializeField] private Text metaLabel;
        [SerializeField] private GameObject emptyHint;

        public void Clear()
        {
            if (titleLabel != null) titleLabel.text = string.Empty;
            if (descriptionLabel != null) descriptionLabel.text = string.Empty;
            if (metaLabel != null) metaLabel.text = string.Empty;
            if (emptyHint != null) emptyHint.SetActive(true);
        }

        public void Show(LibraryItemView item)
        {
            if (emptyHint != null) emptyHint.SetActive(false);
            if (titleLabel != null)
                titleLabel.text = $"#{item.encyclopediaNumber} {item.displayName}";
            if (descriptionLabel != null)
                descriptionLabel.text = item.description ?? string.Empty;
            if (metaLabel != null)
            {
                string timbre = string.IsNullOrEmpty(item.timbreDisplayName)
                    ? item.timbreTagId
                    : item.timbreDisplayName;
                metaLabel.text = $"{item.category} / {timbre}";
            }
        }
    }
}
