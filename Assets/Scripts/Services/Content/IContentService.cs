using Geidai.Common.Results;

namespace Geidai.Services.Content
{
    /// <summary>
    /// コンテンツ取得サービスの器（お題テキスト/ゲームパラメータ等 / NFR-05）。
    /// Sさん が差し替えるデータへのアクセスを抽象化。本実装は U5/U6。
    /// </summary>
    public interface IContentService
    {
        Result<string> GetText(string key);
    }
}
