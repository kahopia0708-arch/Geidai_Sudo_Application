using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CategorySelectorUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private TMP_Text categoryText;

    [Header("Categories")]
    [SerializeField] private string[] categories = new string[]
    {
        "kikiwake",
        "narabekae",
        "action",
        "kumiawase"
    };

    public event Action<string> OnCategoryChanged;

    private int currentIndex = 0;

    private void Awake()
    {
        UpdateCategoryText();
    }

    private void Start()
    {
        if (leftButton != null)
        {
            leftButton.onClick.AddListener(PreviousCategory);
        }

        if (rightButton != null)
        {
            rightButton.onClick.AddListener(NextCategory);
        }

        NotifyCategoryChanged();
    }

    private void OnDestroy()
    {
        if (leftButton != null)
        {
            leftButton.onClick.RemoveListener(PreviousCategory);
        }

        if (rightButton != null)
        {
            rightButton.onClick.RemoveListener(NextCategory);
        }
    }

    private void PreviousCategory()
    {
        if (categories == null || categories.Length == 0)
        {
            return;
        }

        currentIndex--;

        if (currentIndex < 0)
        {
            currentIndex = categories.Length - 1;
        }

        UpdateCategoryText();
        NotifyCategoryChanged();
    }

    private void NextCategory()
    {
        if (categories == null || categories.Length == 0)
        {
            return;
        }

        currentIndex++;

        if (currentIndex >= categories.Length)
        {
            currentIndex = 0;
        }

        UpdateCategoryText();
        NotifyCategoryChanged();
    }

    private void UpdateCategoryText()
    {
        if (categoryText == null)
        {
            return;
        }

        if (categories == null || categories.Length == 0)
        {
            categoryText.text = string.Empty;
            return;
        }

        categoryText.text = categories[currentIndex];
    }

    private void NotifyCategoryChanged()
    {
        OnCategoryChanged?.Invoke(GetCurrentCategory());
    }

    public string GetCurrentCategory()
    {
        if (categories == null || categories.Length == 0)
        {
            return string.Empty;
        }

        return categories[currentIndex];
    }
}
