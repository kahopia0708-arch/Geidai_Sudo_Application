using System.Collections.Generic;
using UnityEngine;

namespace Geidai.Common.Library
{
    /// <summary>
    /// 制作側音図鑑カタログ（差し替え可能 / FR-20 / US-TECH-07）。
    /// 企画・デザイン担当がインスペクタで編集する ScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = "CuratedSoundCatalog", menuName = "Geidai/Curated Sound Catalog", order = 10)]
    public class CuratedSoundCatalog : ScriptableObject
    {
        [SerializeField] private List<CuratedSoundDefinition> items = new List<CuratedSoundDefinition>();

        public IReadOnlyList<CuratedSoundDefinition> Items => items;

        public List<CuratedSoundDefinition> ValidItems()
        {
            var result = new List<CuratedSoundDefinition>();
            if (items == null) return result;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item != null && item.IsValid) result.Add(item);
            }
            return result;
        }

        public CuratedSoundDefinition FindById(string id)
        {
            if (string.IsNullOrEmpty(id) || items == null) return null;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item != null && item.id == id) return item;
            }
            return null;
        }

        public void SetItems(IEnumerable<CuratedSoundDefinition> newItems)
        {
            items = newItems != null ? new List<CuratedSoundDefinition>(newItems) : new List<CuratedSoundDefinition>();
        }
    }
}
