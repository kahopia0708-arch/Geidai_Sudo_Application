using UnityEngine;

namespace Geidai.Common.Library
{
    /// <summary>音図鑑一覧行の表示投影（実行時）。</summary>
    public struct LibraryItemView
    {
        public string id;
        public int encyclopediaNumber;
        public string displayName;
        public string category;
        public string description;
        public string timbreTagId;
        public string timbreDisplayName;
        public Sprite image;
        public bool isUnlocked;
        public AudioClip clip;

        public static LibraryItemView From(
            CuratedSoundDefinition def,
            bool unlocked,
            TimbreTagCatalog timbreCatalog = null)
        {
            string timbreId = def != null ? (def.timbreTagId ?? string.Empty) : string.Empty;
            string timbreName = string.Empty;
            if (timbreCatalog != null && !string.IsNullOrEmpty(timbreId))
            {
                var tag = timbreCatalog.FindById(timbreId);
                if (tag != null) timbreName = tag.displayName ?? string.Empty;
            }

            return new LibraryItemView
            {
                id = def != null ? def.id : string.Empty,
                encyclopediaNumber = def != null ? def.encyclopediaNumber : 0,
                displayName = def != null ? def.displayName : string.Empty,
                category = def != null ? (def.category ?? string.Empty) : string.Empty,
                description = def != null ? (def.description ?? string.Empty) : string.Empty,
                timbreTagId = timbreId,
                timbreDisplayName = timbreName,
                image = def != null ? def.imageRef : null,
                isUnlocked = unlocked,
                clip = def != null ? def.clipRef : null
            };
        }
    }
}
