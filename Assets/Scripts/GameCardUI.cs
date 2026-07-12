using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image thumbnailImage;

    public void SetData(GameCardData data)
    {
        if (data == null)
        {
            return;
        }

        if (titleText != null)
        {
            titleText.text = data.title;
        }

        if (descriptionText != null)
        {
            descriptionText.text = data.description;
        }

        if (thumbnailImage != null)
        {
            thumbnailImage.sprite = data.thumbnail;
            thumbnailImage.enabled = data.thumbnail != null;
        }
    }
}