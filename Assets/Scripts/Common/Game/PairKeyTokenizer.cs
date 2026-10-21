using System;
using System.Collections.Generic;

namespace Geidai.Common.Game
{
    /// <summary>カタログ <c>pairKey</c>（カンマ区切り可）の正規化。</summary>
    public static class PairKeyTokenizer
    {
        public static IReadOnlyList<string> Tokenize(string pairKey)
        {
            var tokens = new List<string>();
            if (string.IsNullOrWhiteSpace(pairKey)) return tokens;

            var parts = pairKey.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                var t = parts[i]?.Trim();
                if (string.IsNullOrEmpty(t)) continue;
                if (!tokens.Contains(t)) tokens.Add(t);
            }
            return tokens;
        }
    }
}
