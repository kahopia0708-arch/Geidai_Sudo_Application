using UnityEngine;

namespace Geidai.Game1
{
    /// <summary>
    /// 1 つの選択肢に割り当てる図鑑 UI（画像＋再生用 soundId）。
    /// </summary>
    public readonly struct SoundMatchChoicePresentation
    {
        public Sprite Sprite { get; }
        public string SoundId { get; }

        public SoundMatchChoicePresentation(Sprite sprite, string soundId)
        {
            Sprite = sprite;
            SoundId = soundId ?? string.Empty;
        }
    }
}
