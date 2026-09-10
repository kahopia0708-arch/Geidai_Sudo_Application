using System;

namespace Geidai.Common.Library
{
    /// <summary>音色タグ語彙の1件（TimbreTagCatalog）。</summary>
    [Serializable]
    public class TimbreTagDefinition
    {
        public string id;
        public string displayName;
        public int sortOrder;

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(displayName);
    }
}
