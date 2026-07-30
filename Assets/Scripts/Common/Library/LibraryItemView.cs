namespace Geidai.Common.Library
{
    /// <summary>音図鑑一覧行の表示投影（実行時）。</summary>
    public struct LibraryItemView
    {
        public string id;
        public string displayName;
        public string category;
        public string description;
        public bool isUnlocked;
        public UnityEngine.AudioClip clip;

        public static LibraryItemView From(CuratedSoundDefinition def, bool unlocked)
        {
            return new LibraryItemView
            {
                id = def != null ? def.id : string.Empty,
                displayName = def != null ? def.displayName : string.Empty,
                category = def != null ? (def.category ?? string.Empty) : string.Empty,
                description = def != null ? (def.description ?? string.Empty) : string.Empty,
                isUnlocked = unlocked,
                clip = def != null ? def.clipRef : null
            };
        }
    }
}
