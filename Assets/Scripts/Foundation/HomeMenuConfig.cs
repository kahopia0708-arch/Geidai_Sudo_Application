using System.Collections.Generic;
using UnityEngine;

namespace Geidai.Foundation
{
    /// <summary>
    /// ホーム導線の構成データ（ScriptableObject / logical-components §1.4）。
    /// HomeScreenController はこの可視項目のみを描画する（データ駆動 / NFR-08）。
    /// Sさん はアセットを編集して並び・ラベル・アイコンを調整できる（US-TECH-07）。
    /// </summary>
    [CreateAssetMenu(fileName = "HomeMenuConfig", menuName = "Geidai/Home Menu Config", order = 10)]
    public class HomeMenuConfig : ScriptableObject
    {
        [SerializeField] private List<HomeMenuItem> items = new List<HomeMenuItem>();

        public IReadOnlyList<HomeMenuItem> Items => items;

        /// <summary>可視項目のみを order 昇順で返す（非表示は除外 / BR-10）。</summary>
        public List<HomeMenuItem> VisibleSorted()
        {
            var result = new List<HomeMenuItem>();
            if (items == null) return result;

            foreach (var item in items)
            {
                if (item != null && item.visible) result.Add(item);
            }
            result.Sort((a, b) => a.order.CompareTo(b.order));
            return result;
        }
    }
}
