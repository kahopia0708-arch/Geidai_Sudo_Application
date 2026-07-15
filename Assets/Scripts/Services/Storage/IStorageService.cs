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
    }
}
