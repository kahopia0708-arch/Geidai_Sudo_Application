using System;

namespace Geidai.Common.Create
{
    /// <summary>
    /// 音づくりレシピ（最大2レイヤー / FR-25〜27）。
    /// 同梱 AudioClip は含まず ID 参照のみ。
    /// </summary>
    [Serializable]
    public class SoundRecipe
    {
        public string id;
        public string title;
        public string createdAtIso;
        public SoundRecipeLayer layerA;
        public SoundRecipeLayer layerB;

        public int LayerCount
        {
            get
            {
                int n = 0;
                if (layerA != null && !string.IsNullOrEmpty(layerA.curatedSoundId)) n++;
                if (layerB != null && !string.IsNullOrEmpty(layerB.curatedSoundId)) n++;
                return n;
            }
        }

        public SoundRecipe Clone()
        {
            return new SoundRecipe
            {
                id = id,
                title = title,
                createdAtIso = createdAtIso,
                layerA = layerA?.Clone(),
                layerB = layerB?.Clone()
            };
        }
    }
}
