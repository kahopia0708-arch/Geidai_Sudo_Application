using System;

namespace Geidai.Common.Models
{
    /// <summary>
    /// 保存音のメタ情報。id は GUID（BR-04）、wav とメタは対で扱う（BR-05）。
    /// createdAtIso は ISO 8601 文字列（JsonUtility 対応のため文字列で保持）。
    /// </summary>
    [Serializable]
    public class SoundClipMeta
    {
        public string id;
        public string displayName;
        public string createdAtIso;
        public string wavFileName;

        public SoundClipMeta()
        {
            id = string.Empty;
            displayName = string.Empty;
            createdAtIso = string.Empty;
            wavFileName = string.Empty;
        }

        public static SoundClipMeta CreateNew(string displayName)
        {
            var id = Guid.NewGuid().ToString("N");
            return new SoundClipMeta
            {
                id = id,
                displayName = displayName ?? string.Empty,
                createdAtIso = DateTime.UtcNow.ToString("o"),
                wavFileName = id + ".wav"
            };
        }
    }
}
