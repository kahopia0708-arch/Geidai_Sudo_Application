using System;

namespace Geidai.Common.Create
{
    /// <summary>レシピの1レイヤー（素材ID＋パラメータ。元音バイナリは含まない）。</summary>
    [Serializable]
    public class SoundRecipeLayer
    {
        public string curatedSoundId;
        public float volume = 1f;
        public int pitchSemitones;
        public float reverb;
        public RecipeTimbreKind timbre;

        public SoundRecipeLayer Clone()
        {
            return new SoundRecipeLayer
            {
                curatedSoundId = curatedSoundId,
                volume = volume,
                pitchSemitones = pitchSemitones,
                reverb = reverb,
                timbre = timbre
            };
        }
    }
}
