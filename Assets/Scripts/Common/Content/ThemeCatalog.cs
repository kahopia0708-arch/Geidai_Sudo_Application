using System.Collections.Generic;
using UnityEngine;

namespace Geidai.Common.Content
{
    /// <summary>
    /// お題一覧（差し替え可能構成 / FR-14 / US-THEME-03）。
    /// Sさん がインスペクタで追加/編集/並べ替え可能な ScriptableObject。
    /// 既定アセットは既存 WeeklyTextController の固定オノマトペを移行して作成する（MCP フォローアップ）。
    /// </summary>
    [CreateAssetMenu(fileName = "ThemeCatalog", menuName = "Geidai/Theme Catalog", order = 0)]
    public class ThemeCatalog : ScriptableObject
    {
        [Tooltip("今週のお題として出題される候補（上から順に週番号で選択）。")]
        [SerializeField] private List<ThemeItem> items = new List<ThemeItem>();

        /// <summary>登録された全項目（無効含む・読み取り専用参照）。</summary>
        public IReadOnlyList<ThemeItem> Items => items;

        /// <summary>本文が有効な項目のみを返す（BR-THEME-11）。</summary>
        public List<ThemeItem> ValidItems()
        {
            var result = new List<ThemeItem>();
            if (items == null) return result;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item != null && item.IsValid) result.Add(item);
            }
            return result;
        }

        /// <summary>有効項目数（週選択の count に使う）。</summary>
        public int ValidCount => ValidItems().Count;

        /// <summary>
        /// 項目を差し替える（既定アセットのプログラム生成/移行・テスト用）。
        /// 通常運用では Sさん がインスペクタで編集する。
        /// </summary>
        public void SetItems(IEnumerable<ThemeItem> newItems)
        {
            items = newItems != null ? new List<ThemeItem>(newItems) : new List<ThemeItem>();
        }
    }
}
