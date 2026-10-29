using System.Collections.Generic;
using Geidai.Common.Library;
using Geidai.Common.Models;
using UnityEngine;

namespace Geidai.Common.Game
{
    /// <summary>
    /// ①音合わせの基準音素材を音図鑑→保存音→fallback の順で選ぶ（純粋ロジック）。
    /// </summary>
    public readonly struct SoundMatchBaseSelection
    {
        public AudioBuffer Buffer { get; }
        public string SoundId { get; }
        public Sprite DisplaySprite { get; }

        public SoundMatchBaseSelection(AudioBuffer buffer, string soundId, Sprite displaySprite)
        {
            Buffer = buffer;
            SoundId = soundId ?? string.Empty;
            DisplaySprite = displaySprite;
        }

        public bool IsValid => Buffer != null;
    }

    public static class SoundMatchBaseSoundSelector
    {
        public static bool TrySelectFromCatalog(
            CuratedSoundCatalog catalog,
            Sprite placeholderTarget,
            int seedValue,
            out SoundMatchBaseSelection selection)
        {
            selection = default;
            if (catalog == null) return false;

            var items = catalog.ValidItems();
            var candidates = new List<CuratedSoundDefinition>();
            foreach (var item in items)
            {
                if (item == null) continue;
                if (!item.allowPitchShift) continue;
                if (!item.HasBasePitch) continue;
                if (item.clipRef == null) continue;
                candidates.Add(item);
            }

            if (candidates.Count == 0) return false;

            int pick = new System.Random(seedValue).Next(0, candidates.Count);
            var selected = candidates[pick];
            var buffer = BufferFromClip(selected.clipRef);
            if (buffer == null) return false;

            var sprite = selected.imageRef != null ? selected.imageRef : placeholderTarget;
            selection = new SoundMatchBaseSelection(buffer, selected.id, sprite);
            return true;
        }

        public static AudioBuffer BufferFromClip(AudioClip clip)
        {
            if (clip == null || clip.samples <= 0) return null;
            var data = new float[clip.samples * clip.channels];
            if (!clip.GetData(data, 0)) return null;
            return new AudioBuffer(data);
        }
    }
}
