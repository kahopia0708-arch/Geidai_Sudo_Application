using System.Collections.Generic;
using Geidai.Common.Models;
using Geidai.Common.Results;

namespace Geidai.Services.Storage
{
    /// <summary>
    /// ローカル永続化サービス（NFR-07 / BR-04〜07 / US-TECH-06）。
    /// U4 で全書込を <see cref="Geidai.Services.IO.AtomicFile"/> の原子的置換へ統一し、
    /// 破損スキップ・空フォールバックを徹底する（NFR-COL-R1〜R4）。既存シグネチャは不変。
    /// </summary>
    public interface IStorageService
    {
        Result<UserProfile> LoadProfile();
        Result SaveProfile(UserProfile profile);
        Result<List<SavedSound>> ListSounds();
        Result<SavedSound> LoadSound(string id);

        /// <summary>
        /// 録音音声（生 <see cref="AudioBuffer"/>）と加工設定・メタ（<see cref="SavedSound"/>）を
        /// 対で保存する（U3 追加 / US-REC-03）。U4 で wav→meta を原子的置換＋対整合に強化。
        /// </summary>
        Result SaveSound(SavedSound sound, AudioBuffer buffer);

        /// <summary>
        /// 音（wav＋meta＋写真）を一括削除する（U4 / US-COL-01）。
        /// 欠損は無視（ベストエフォート）。確認は呼び出し側（ConfirmDialog）で行う。
        /// </summary>
        Result DeleteSound(string id);

        /// <summary>
        /// メタのみを更新する（U4 / US-COL-02）。既存 meta.json の settings を保持したまま
        /// <see cref="SoundClipMeta"/> を差し替え、原子的置換で書き込む。無ければ新規作成しない
        /// （対 wav が無いメタ単独は作らない）。
        /// </summary>
        Result SaveMeta(SoundClipMeta meta);

        /// <summary>
        /// 写真を取り込む（U4 / US-COL-02）。<paramref name="sourceTempPath"/> を
        /// sounds/{id}.photo.&lt;ext&gt; へ原子的コピーし、更新後の photoFileName を返す。
        /// meta の photoFileName 更新は <see cref="SaveMeta"/> で行う（呼び出し側が反映）。
        /// </summary>
        Result<string> SavePhoto(string id, string sourceTempPath);

        /// <summary>写真ファイルを削除する（U4 / US-COL-02）。欠損は無視。</summary>
        Result RemovePhoto(string id);

        /// <summary>
        /// 写真バイト列を読み込む（U4 / 一覧サムネ・詳細表示）。写真が無ければ NotFound。
        /// 端末内のみ・PII をログに出さない（NFR-COL-Priv2）。
        /// </summary>
        Result<byte[]> LoadPhoto(string id);

        /// <summary>
        /// 保存 wav をデコードして再生用 <see cref="AudioBuffer"/> を得る（U4 / US-COL-01 視聴）。
        /// 破損/欠損は失敗コードで返す。
        /// </summary>
        Result<AudioBuffer> LoadSoundBuffer(string id);
    }
}
