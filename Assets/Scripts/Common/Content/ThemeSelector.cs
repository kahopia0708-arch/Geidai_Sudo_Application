using System;

namespace Geidai.Common.Content
{
    /// <summary>
    /// 「今週のお題」選択の純粋関数（U5 / NFR-U5-01/04 / P1）。
    /// 既存 WeeklyTextController の週番号ロジックを純粋移植（月曜起点・年初前は前年最終週）。
    /// 副作用なし・O(1)・決定的（PBT 対象）。時刻は呼び出し側から引数で注入する。
    /// </summary>
    public static class ThemeSelector
    {
        /// <summary>
        /// 指定日時の週で選ばれるお題の index を返す。
        /// count &lt;= 0 なら -1（お題なし）。それ以外は 0..count-1 に正規化した剰余。
        /// </summary>
        public static int SelectIndex(DateTime date, int count)
        {
            if (count <= 0) return -1;
            int week = WeekOfYear(date);
            return ((week % count) + count) % count;
        }

        /// <summary>
        /// 月曜起点の週番号（1 起点）。年初の最初の月曜より前は前年の最終週として扱う（既存挙動踏襲）。
        /// </summary>
        public static int WeekOfYear(DateTime date)
        {
            DateTime startOfYear = new DateTime(date.Year, 1, 1);
            int daysToMonday = ((int)DayOfWeek.Monday - (int)startOfYear.DayOfWeek + 7) % 7;
            DateTime firstMonday = startOfYear.AddDays(daysToMonday);

            if (date < firstMonday)
            {
                // 去年の最終週（12/31 基準）
                DateTime lastYearEnd = new DateTime(date.Year - 1, 12, 31);
                return WeekOfYearFromDate(lastYearEnd);
            }

            return WeekOfYearFromDate(date);
        }

        private static int WeekOfYearFromDate(DateTime date)
        {
            DateTime startOfYear = new DateTime(date.Year, 1, 1);
            int daysToMonday = ((int)DayOfWeek.Monday - (int)startOfYear.DayOfWeek + 7) % 7;
            DateTime firstMonday = startOfYear.AddDays(daysToMonday);

            if (date < firstMonday) return 52;

            TimeSpan diff = date - firstMonday;
            return (diff.Days / 7) + 1;
        }
    }
}
