using System;
using System.Collections.Generic;
using UnityEngine;

namespace Geidai.Foundation
{
    /// <summary>
    /// ホームメニューの iconKey → Sprite 対応表（US-TECH-07）。
    /// Sさん は Sprite 差し替えまたはエントリ追加でアイコンを更新できる。
    /// </summary>
    [CreateAssetMenu(fileName = "HomeMenuIconCatalog", menuName = "Geidai/Home Menu Icon Catalog", order = 11)]
    public class HomeMenuIconCatalog : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string key;
            public Sprite sprite;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        public Sprite Resolve(string iconKey)
        {
            if (string.IsNullOrEmpty(iconKey) || entries == null) return null;

            foreach (var entry in entries)
            {
                if (entry != null && entry.key == iconKey && entry.sprite != null)
                    return entry.sprite;
            }

            return null;
        }
    }
}
