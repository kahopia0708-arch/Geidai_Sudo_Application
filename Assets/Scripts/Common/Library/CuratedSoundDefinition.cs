using System;
using UnityEngine;

namespace Geidai.Common.Library
{
    /// <summary>
    /// 制作側同梱音の1件定義（U7a 新スキーマ）。
    /// id・encyclopediaNumber・displayName・timbreTagId・category・clip が揃わない定義は一覧から除外し得る。
    /// </summary>
    [Serializable]
    public class CuratedSoundDefinition
    {
        public const int UnsetPitchMidi = -1;

        public string id;
        public int encyclopediaNumber;
        public string displayName;
        public string reading;
        [TextArea] public string description;
        public Sprite imageRef;
        public string timbreTagId;
        public int basePitchMidi = UnsetPitchMidi;
        public LoudnessBand loudnessBand = LoudnessBand.None;
        public DurationBand durationBand = DurationBand.None;
        public string pairKey;
        public bool allowPitchShift = true;
        public string[] difficultyTags;
        public string category;
        public AudioClip clipRef;
        public bool initiallyUnlocked;

        public bool HasBasePitch => basePitchMidi >= 0 && basePitchMidi <= 127;

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(id)
            && encyclopediaNumber >= 1
            && !string.IsNullOrWhiteSpace(displayName)
            && !string.IsNullOrWhiteSpace(timbreTagId)
            && !string.IsNullOrWhiteSpace(category)
            && clipRef != null;
    }
}
