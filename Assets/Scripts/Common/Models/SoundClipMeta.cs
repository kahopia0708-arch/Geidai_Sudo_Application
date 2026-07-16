using System;

namespace Geidai.Common.Models
{
    /// <summary>
    /// 保存音のメタ情報。id は GUID（BR-04）、wav とメタは対で扱う（BR-05）。
    /// createdAtIso は ISO 8601 文字列（JsonUtility 対応のため文字列で保持）。
    /// U4（FR-10）で表示名・写真・メモ・ニックネームを**後方互換で追記**する。
    /// JsonUtility は欠損フィールドを既定値で読むため、旧 JSON（U1/U3 保存分）も安全に読める。
    /// </summary>
    [Serializable]
    public class SoundClipMeta
    {
        // --- 既存（U1・不変） ---
        public string id;
        public string displayName;
        public string createdAtIso;
        public string wavFileName;

        // --- U4 追記（後方互換・既定 ""） ---
        /// <summary>表示名（FR-10）。空なら作成日付を表示する（BR-COL-11）。displayName とは別。</summary>
        public string title;
        /// <summary>任意の写真ファイル名（sounds/{id}.photo.*）。空＝写真なし。PII（端末外送信・ログ禁止）。</summary>
        public string photoFileName;
        /// <summary>任意メモ（FR-10）。PII（端末外送信・ログ禁止）。</summary>
        public string memo;
        /// <summary>保存時にプロフィールのニックネームを写す（FR-10）。PII（端末外送信・ログ禁止）。</summary>
        public string nickname;

        public SoundClipMeta()
        {
            id = string.Empty;
            displayName = string.Empty;
            createdAtIso = string.Empty;
            wavFileName = string.Empty;
            title = string.Empty;
            photoFileName = string.Empty;
            memo = string.Empty;
            nickname = string.Empty;
        }

        public static SoundClipMeta CreateNew(string displayName)
        {
            var id = Guid.NewGuid().ToString("N");
            return new SoundClipMeta
            {
                id = id,
                displayName = displayName ?? string.Empty,
                createdAtIso = DateTime.UtcNow.ToString("o"),
                wavFileName = id + ".wav",
                title = string.Empty,
                photoFileName = string.Empty,
                memo = string.Empty,
                nickname = string.Empty
            };
        }
    }
}
