using Geidai.Common.Results;

namespace Geidai.Services.Content
{
    /// <summary>
    /// ContentService の最小の器。U1 では未実装（NotImplemented を Result で返す）。
    /// お題/ゲームパラメータの取得は U5/U6 で ScriptableObject/JSON をもとに実装する。
    /// </summary>
    public class ContentService : IContentService
    {
        public Result<string> GetText(string key)
        {
            return Result<string>.Fail(ResultCode.NotImplemented, "コンテンツ取得は後続ユニットで実装します。");
        }
    }
}
