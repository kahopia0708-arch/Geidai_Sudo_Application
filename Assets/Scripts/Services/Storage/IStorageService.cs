using System.Collections.Generic;
using Geidai.Common.Models;
using Geidai.Common.Results;

namespace Geidai.Services.Storage
{
    /// <summary>
    /// ローカル永続化サービス（NFR-07 / BR-04〜07）。
    /// U1 は最小実装（基本保存＋対ファイル整合チェック）。原子的置換・詳細復旧は U4。
    /// </summary>
    public interface IStorageService
    {
        Result<UserProfile> LoadProfile();
        Result SaveProfile(UserProfile profile);
        Result<List<SavedSound>> ListSounds();
        Result<SavedSound> LoadSound(string id);

        /// <summary>
        /// 録音音声（生 <see cref="AudioBuffer"/>）と加工設定・メタ（<see cref="SavedSound"/>）を
        /// 対で保存する（U3 追加 / US-REC-03）。U3 は最小実装（wav→meta・失敗時 wav 削除）で、
        /// 原子的置換・破損復旧の本実装は U4。
        /// </summary>
        Result SaveSound(SavedSound sound, AudioBuffer buffer);
    }
}
