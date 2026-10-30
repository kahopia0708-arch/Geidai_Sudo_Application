using System.Collections.Generic;
using System.Linq;
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
            return TrySelectFromCatalog(catalog, placeholderTarget, seedValue, null, out selection);
        }

        /// <param name="excludeSoundIds">同一ゲーム内で既に使った id を避ける（候補不足時は無視して再抽選）。</param>
        public static bool TrySelectFromCatalog(
            CuratedSoundCatalog catalog,
            Sprite placeholderTarget,
            int seedValue,
            IReadOnlyCollection<string> excludeSoundIds,
            out SoundMatchBaseSelection selection)
        {
            selection = default;
            if (catalog == null) return false;

            var candidates = CollectGameplayCandidates(catalog);
            if (candidates.Count == 0) return false;

            var pool = FilterExcluded(candidates, excludeSoundIds);
            if (pool.Count == 0)
                pool = candidates;

            int pick = new System.Random(seedValue).Next(0, pool.Count);
            var selected = pool[pick];
            var buffer = BufferFromClip(selected.clipRef);
            if (buffer == null) return false;

            var sprite = selected.imageRef != null ? selected.imageRef : null;
            selection = new SoundMatchBaseSelection(buffer, selected.id, sprite);
            return true;
        }

        private static List<CuratedSoundDefinition> CollectGameplayCandidates(CuratedSoundCatalog catalog)
        {
            var candidates = new List<CuratedSoundDefinition>();
            foreach (var item in catalog.ValidItems())
            {
                if (item == null) continue;
                if (!item.allowPitchShift) continue;
                if (item.clipRef == null) continue;
                if (item.imageRef == null) continue;
                candidates.Add(item);
            }

            return candidates;
        }

        private static List<CuratedSoundDefinition> FilterExcluded(
            List<CuratedSoundDefinition> candidates,
            IReadOnlyCollection<string> excludeSoundIds)
        {
            if (excludeSoundIds == null || excludeSoundIds.Count == 0)
                return candidates;

            var pool = new List<CuratedSoundDefinition>();
            for (int i = 0; i < candidates.Count; i++)
            {
                var item = candidates[i];
                if (item == null || string.IsNullOrEmpty(item.id)) continue;
                if (IsExcludedId(excludeSoundIds, item.id)) continue;
                pool.Add(item);
            }

            return pool;
        }

        private static bool IsExcludedId(IReadOnlyCollection<string> excludeSoundIds, string id)
        {
            if (excludeSoundIds == null || excludeSoundIds.Count == 0) return false;
            foreach (var excluded in excludeSoundIds)
            {
                if (excluded == id) return true;
            }

            return false;
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
