using System.Collections.Generic;

namespace Geidai.Game3
{
    public static class PitchOrderJudge
    {
        /// <summary>
        /// 低い音 → 高い音の順に並んでいるか判定する。
        /// </summary>
        public static bool IsAscending(IReadOnlyList<int> cents)
        {
            if (cents == null || cents.Count < 2)
                return false;

            for (int i = 1; i < cents.Count; i++)
            {
                if (cents[i - 1] >= cents[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 高い音 → 低い音の順に並んでいるか判定する。
        /// </summary>
        public static bool IsDescending(IReadOnlyList<int> cents)
        {
            if (cents == null || cents.Count < 2)
                return false;

            for (int i = 1; i < cents.Count; i++)
            {
                if (cents[i - 1] <= cents[i])
                    return false;
            }

            return true;
        }
    }
}