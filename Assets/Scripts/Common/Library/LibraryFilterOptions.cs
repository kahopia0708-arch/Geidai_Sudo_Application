using System.Collections.Generic;

namespace Geidai.Common.Library
{
    /// <summary>
    /// 図鑑フィルタ用ドロップダウン選択肢（純粋 / U7b）。
    /// 先頭ラベル「すべて」に対応する値は null または空文字。
    /// </summary>
    public static class LibraryFilterOptions
    {
        public const string AllLabel = "すべて";

        public static List<string> UniqueCategories(IReadOnlyList<CuratedSoundDefinition> items)
        {
            var result = new List<string>();
            if (items == null) return result;
            var seen = new HashSet<string>();
            for (int i = 0; i < items.Count; i++)
            {
                var d = items[i];
                if (d == null || !d.IsValid) continue;
                var c = d.category;
                if (string.IsNullOrWhiteSpace(c)) continue;
                if (seen.Add(c)) result.Add(c);
            }
            result.Sort(System.StringComparer.Ordinal);
            return result;
        }

        public static List<string> CategoryLabels(IReadOnlyList<CuratedSoundDefinition> items)
        {
            var labels = new List<string> { AllLabel };
            labels.AddRange(UniqueCategories(items));
            return labels;
        }

        /// <summary>インデックス 0 = すべて（フィルタ値 null）。</summary>
        public static string CategoryValueAt(IReadOnlyList<string> labels, int index)
        {
            if (labels == null || index <= 0 || index >= labels.Count) return null;
            return labels[index];
        }

        public static List<string> TimbreLabels(TimbreTagCatalog catalog)
        {
            var labels = new List<string> { AllLabel };
            if (catalog == null) return labels;
            var tags = catalog.ValidTags();
            for (int i = 0; i < tags.Count; i++)
                labels.Add(tags[i].displayName ?? tags[i].id);
            return labels;
        }

        public static List<string> TimbreIds(TimbreTagCatalog catalog)
        {
            var ids = new List<string> { null };
            if (catalog == null) return ids;
            var tags = catalog.ValidTags();
            for (int i = 0; i < tags.Count; i++)
                ids.Add(tags[i].id);
            return ids;
        }

        public static string TimbreValueAt(IReadOnlyList<string> ids, int index)
        {
            if (ids == null || index <= 0 || index >= ids.Count) return null;
            return ids[index];
        }

        /// <summary>フィルタ後も選択 id が残れば維持、無ければ null（NFR Q3=A）。</summary>
        public static string ResolveSelectionAfterFilter(
            string selectedId,
            IReadOnlyList<LibraryItemView> filteredItems)
        {
            if (string.IsNullOrEmpty(selectedId) || filteredItems == null) return null;
            for (int i = 0; i < filteredItems.Count; i++)
            {
                if (filteredItems[i].id == selectedId) return selectedId;
            }
            return null;
        }

        public static bool TryGetItem(
            IReadOnlyList<LibraryItemView> items,
            string id,
            out LibraryItemView item)
        {
            item = default;
            if (items == null || string.IsNullOrEmpty(id)) return false;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].id == id)
                {
                    item = items[i];
                    return true;
                }
            }
            return false;
        }
    }
}
