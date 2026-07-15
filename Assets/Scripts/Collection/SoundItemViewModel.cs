using System;
using System.Globalization;
using Geidai.Common.Models;

namespace Geidai.Collection
{
    /// <summary>
    /// 一覧描画に必要な最小情報（<see cref="SavedSound"/> からの投影 / frontend-components §3）。
    /// 表示のたびに再計算しないよう一度作ってキャッシュする（NFR-COL-P1）。
    /// </summary>
    public class SoundItemViewModel
    {
        public string id;
        public string displayTitle;  // title が空なら日付
        public string createdAtIso;
        public bool hasPhoto;

        public static SoundItemViewModel From(SavedSound sound)
        {
            var vm = new SoundItemViewModel();
            if (sound == null || sound.meta == null) return vm;

            var m = sound.meta;
            vm.id = m.id;
            vm.createdAtIso = m.createdAtIso;
            vm.hasPhoto = !string.IsNullOrEmpty(m.photoFileName);
            vm.displayTitle = !string.IsNullOrEmpty(m.title) ? m.title : FormatDate(m.createdAtIso);
            return vm;
        }

        /// <summary>title 空時の代替表示（作成日付・BR-COL-11）。パース不能なら元文字列。</summary>
        public static string FormatDate(string createdAtIso)
        {
            if (string.IsNullOrEmpty(createdAtIso)) return "(なまえなし)";
            if (DateTime.TryParse(createdAtIso, null, DateTimeStyles.RoundtripKind, out var dt))
                return dt.ToLocalTime().ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);
            return createdAtIso;
        }
    }
}
