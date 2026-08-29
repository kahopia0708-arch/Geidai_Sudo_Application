using System.Collections.Generic;

namespace Geidai.Common.Library
{
    /// <summary>
    /// 図鑑一覧のソート・フィルタ（純粋 / U7a）。
    /// </summary>
    public static class LibraryQuery
    {
        public static List<CuratedSoundDefinition> SortByEncyclopediaNumber(
            IReadOnlyList<CuratedSoundDefinition> items)
        {
            var list = CopyValid(items);
            list.Sort(CompareByNumberThenId);
            return list;
        }

        public static List<CuratedSoundDefinition> Filter(
            IReadOnlyList<CuratedSoundDefinition> items,
            string category,
            string timbreTagId)
        {
            var list = CopyValid(items);
            bool filterCategory = !string.IsNullOrWhiteSpace(category);
            bool filterTimbre = !string.IsNullOrWhiteSpace(timbreTagId);

            if (!filterCategory && !filterTimbre) return list;

            var result = new List<CuratedSoundDefinition>();
            for (int i = 0; i < list.Count; i++)
            {
                var d = list[i];
                if (filterCategory && d.category != category) continue;
                if (filterTimbre && d.timbreTagId != timbreTagId) continue;
                result.Add(d);
            }
            return result;
        }

        public static List<CuratedSoundDefinition> SortAndFilter(
            IReadOnlyList<CuratedSoundDefinition> items,
            string category,
            string timbreTagId)
        {
            return SortByEncyclopediaNumber(Filter(items, category, timbreTagId));
        }

        private static List<CuratedSoundDefinition> CopyValid(IReadOnlyList<CuratedSoundDefinition> items)
        {
            var list = new List<CuratedSoundDefinition>();
            if (items == null) return list;
            for (int i = 0; i < items.Count; i++)
            {
                var d = items[i];
                if (d != null && d.IsValid) list.Add(d);
            }
            return list;
        }

        private static int CompareByNumberThenId(CuratedSoundDefinition a, CuratedSoundDefinition b)
        {
            int c = a.encyclopediaNumber.CompareTo(b.encyclopediaNumber);
            if (c != 0) return c;
            return string.CompareOrdinal(a.id, b.id);
        }
    }
}
