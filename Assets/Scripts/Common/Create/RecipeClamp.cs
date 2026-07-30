namespace Geidai.Common.Create
{
    /// <summary>レシピ加工パラメータのクランプ定数（BR-CREATE-09）。</summary>
    public static class RecipeClamp
    {
        public const float VolumeMin = 0f;
        public const float VolumeMax = 1f;
        public const int PitchSemitonesMin = -12;
        public const int PitchSemitonesMax = 12;
        public const float ReverbMin = 0f;
        public const float ReverbMax = 1f;

        public static float ClampVolume(float v)
        {
            if (v < VolumeMin) return VolumeMin;
            if (v > VolumeMax) return VolumeMax;
            return v;
        }

        public static int ClampPitch(int semitones)
        {
            if (semitones < PitchSemitonesMin) return PitchSemitonesMin;
            if (semitones > PitchSemitonesMax) return PitchSemitonesMax;
            return semitones;
        }

        public static float ClampReverb(float r)
        {
            if (r < ReverbMin) return ReverbMin;
            if (r > ReverbMax) return ReverbMax;
            return r;
        }
    }
}
