using System;
using System.Collections.Generic;
using Geidai.Common.Library;
using UnityEngine;

namespace Geidai.Library
{
    /// <summary>音図鑑スクロール一覧。</summary>
    public class CuratedSoundListView : MonoBehaviour
    {
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private CuratedSoundItemView itemPrefab;
        [SerializeField] private GameObject emptyState;

        private readonly List<CuratedSoundItemView> _pool = new List<CuratedSoundItemView>();

        public event Action<LibraryItemView> ItemPlayRequested;

        public void SetItems(IReadOnlyList<LibraryItemView> items)
        {
            int count = items != null ? items.Count : 0;
            if (emptyState != null) emptyState.SetActive(count == 0);
            EnsurePool(count);

            for (int i = 0; i < _pool.Count; i++)
            {
                var view = _pool[i];
                if (i < count)
                {
                    view.gameObject.SetActive(true);
                    view.Bind(items[i], OnPlay);
                }
                else
                {
                    view.gameObject.SetActive(false);
                }
            }
        }

        private void EnsurePool(int count)
        {
            if (itemPrefab == null || contentRoot == null) return;
            while (_pool.Count < count)
                _pool.Add(UnityEngine.Object.Instantiate(itemPrefab, contentRoot));
        }

        private void OnPlay(LibraryItemView item) => ItemPlayRequested?.Invoke(item);
    }
}
