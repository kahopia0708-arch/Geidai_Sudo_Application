using UnityEngine;

namespace Geidai.Common.Game
{
    /// <summary>神経衰弱の1枚。同一 <see cref="SoundId"/>（＝同じ音）の2枚でペア成立。</summary>
    public readonly struct MemoryMatchCardSpec
    {
        public string SoundId { get; }
        public string DisplayName { get; }
        /// <summary>ペア判定用（通常は <see cref="SoundId"/> と同じ）。</summary>
        public string MatchGroupId { get; }
        public AudioClip Clip { get; }
        public Sprite Image { get; }

        public MemoryMatchCardSpec(string soundId, string displayName, string matchGroupId, AudioClip clip, Sprite image)
        {
            SoundId = soundId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            MatchGroupId = matchGroupId ?? string.Empty;
            Clip = clip;
            Image = image;
        }
    }
}
