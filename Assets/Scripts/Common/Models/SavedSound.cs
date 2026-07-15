using System;

namespace Geidai.Common.Models
{
    /// <summary>
    /// 保存音の集約（メタ＋加工設定の対）。
    /// 永続化では {id}.wav と {id}.meta.json を対で扱う（BR-05）。
    /// </summary>
    [Serializable]
    public class SavedSound
    {
        public SoundClipMeta meta;
        public SoundEffectSettingsData settings;

        public SavedSound()
        {
            meta = new SoundClipMeta();
            settings = new SoundEffectSettingsData();
        }

        public SavedSound(SoundClipMeta meta, SoundEffectSettingsData settings)
        {
            this.meta = meta;
            this.settings = settings;
        }
    }
}
