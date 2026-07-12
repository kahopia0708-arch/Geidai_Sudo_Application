using System.Collections.Generic;
using UnityEngine;

public class GameCardListUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CategorySelectorUI categorySelector;
    [SerializeField] private Transform cardParent;
    [SerializeField] private GameCardUI cardPrefab;

    [Header("Cards")]
    [SerializeField] private List<GameCardData> cards = new List<GameCardData>();

    private readonly List<GameCardUI> spawnedCards = new List<GameCardUI>();

    private void Start()
    {
        if (categorySelector != null)
        {
            categorySelector.OnCategoryChanged += RefreshCards;
            RefreshCards(categorySelector.GetCurrentCategory());
        }
    }

    private void OnDestroy()
    {
        if (categorySelector != null)
        {
            categorySelector.OnCategoryChanged -= RefreshCards;
        }
    }

    private void RefreshCards(string category)
    {
        ClearCards();

        foreach (GameCardData cardData in cards)
        {
            if (cardData.category != category)
            {
                continue;
            }

            GameCardUI card = Instantiate(cardPrefab, cardParent);
            card.SetData(cardData);
            spawnedCards.Add(card);
        }
    }

    private void ClearCards()
    {
        foreach (GameCardUI card in spawnedCards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }

        spawnedCards.Clear();
    }
}