using System.Collections.Generic;
using Geidai.Common.Models;

namespace Geidai.Common.Collection
{
    /// <summary>
    /// 一覧読込の結果（有効項目＋読み飛ばし件数 / Q3=A・NFR-COL-R2）。
    /// 破損/対 wav 欠損は安全にスキップし、他は正常に返す（US-COL-04）。
    /// </summary>
    public class LoadOutcome
    {
        /// <summary>有効に読めた項目（破損・欠損は除外）。</summary>
        public List<SavedSound> items;

        /// <summary>破損/対欠損で読み飛ばした件数（ログ/デバッグ用途・PII を含めない）。</summary>
        public int skippedCount;

        public LoadOutcome()
        {
            items = new List<SavedSound>();
            skippedCount = 0;
        }

        public LoadOutcome(List<SavedSound> items, int skippedCount)
        {
            this.items = items ?? new List<SavedSound>();
            this.skippedCount = skippedCount;
        }
    }
}
