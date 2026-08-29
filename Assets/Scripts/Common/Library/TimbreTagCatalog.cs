using System.Collections.Generic;
using UnityEngine;

namespace Geidai.Common.Library
{
    /// <summary>
    /// 音色タグの制御語彙（Editor から追加・変更・削除）。
    /// </summary>
    [CreateAssetMenu(fileName = "TimbreTagCatalog", menuName = "Geidai/Timbre Tag Catalog", order = 12)]
    public class TimbreTagCatalog : ScriptableObject
    {
        [SerializeField] private List<TimbreTagDefinition> tags = new List<TimbreTagDefinition>();

        public IReadOnlyList<TimbreTagDefinition> Tags => tags;

        public void SetTags(IEnumerable<TimbreTagDefinition> newTags)
        {
            tags = newTags != null ? new List<TimbreTagDefinition>(newTags) : new List<TimbreTagDefinition>();
        }

        public TimbreTagDefinition FindById(string id)
        {
            if (string.IsNullOrEmpty(id) || tags == null) return null;
            for (int i = 0; i < tags.Count; i++)
            {
                var t = tags[i];
                if (t != null && t.id == id) return t;
            }
            return null;
        }

        public bool ContainsId(string id) => FindById(id) != null;

        public List<TimbreTagDefinition> ValidTags()
        {
            var result = new List<TimbreTagDefinition>();
            if (tags == null) return result;
            for (int i = 0; i < tags.Count; i++)
            {
                var t = tags[i];
                if (t != null && t.IsValid) result.Add(t);
            }
            result.Sort((a, b) => a.sortOrder.CompareTo(b.sortOrder));
            return result;
        }
    }
}
