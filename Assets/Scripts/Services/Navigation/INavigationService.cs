using Geidai.Common.Models;
using Geidai.Common.Results;

namespace Geidai.Services.Navigation
{
    /// <summary>型安全な画面遷移サービス（US-TECH-04 / FR-02 / BR-12〜15）。</summary>
    public interface INavigationService
    {
        Result GoTo(SceneId sceneId);
        Result GoBack();
        SceneId? Current { get; }
    }
}
