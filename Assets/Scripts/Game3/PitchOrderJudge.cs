using System.Collections.Generic;

namespace Geidai.Game3
{
    public static class PitchOrderJudge
    {
        /// <summary>
        /// ’á‚¢‰¹ ¨ ‚‚¢‰¹‚Ì‡‚É•À‚ñ‚Å‚¢‚é‚©”»’è‚·‚éB
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
        /// ‚‚¢‰¹ ¨ ’á‚¢‰¹‚Ì‡‚É•À‚ñ‚Å‚¢‚é‚©”»’è‚·‚éB
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