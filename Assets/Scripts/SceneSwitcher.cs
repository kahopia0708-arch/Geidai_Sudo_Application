using UnityEngine;
using UnityEngine.SceneManagement;
using Geidai.Common.Models;
using Geidai.Services;
using Geidai.Services.Audio;
using Geidai.Services.Content;
using Geidai.Services.Navigation;
using Geidai.Services.Storage;

/// <summary>
/// ブラウンフィールド用シーン遷移（Main画面 / game_Home）。
/// 既定は <see cref="INavigationService"/> 経由。未登録時のみ文字列シーン名へフォールバック。
/// </summary>
public class SceneSwitcher : MonoBehaviour
{
    [Header("移動先のシーン名（フォールバック）")]
    [SerializeField] private string sceneName = "GeidaiHome";

    [Header("ホーム画面のシーン名（フォールバック）")]
    [SerializeField] private string homeSceneName = "GeidaiHome";

    [SerializeField] private SceneId targetScene = SceneId.Home;
    [SerializeField] private SceneId homeScene = SceneId.Home;

    private void Awake()
    {
        EnsureServices();
    }

    private static void EnsureServices()
    {
        if (!ServiceRegistry.IsRegistered<INavigationService>())
            ServiceRegistry.Register<INavigationService>(new NavigationService());
        if (!ServiceRegistry.IsRegistered<IStorageService>())
            ServiceRegistry.Register<IStorageService>(new StorageService());
        if (!ServiceRegistry.IsRegistered<IContentService>())
            ServiceRegistry.Register<IContentService>(new ContentService());
        if (!ServiceRegistry.IsRegistered<IAudioService>())
            ServiceRegistry.Register<IAudioService>(new AudioService());
    }

    public void SwitchScene()
    {
        EnsureServices();
        var nav = ServiceRegistry.Resolve<INavigationService>();
        if (nav != null)
        {
            var result = nav.GoTo(targetScene);
            if (result.IsSuccess) return;
            Debug.LogWarning($"[SceneSwitcher] GoTo({targetScene}) failed: {result.Code}");
        }

        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.LogWarning("sceneName is empty!");
    }

    public void ReturnHome()
    {
        EnsureServices();
        var nav = ServiceRegistry.Resolve<INavigationService>();
        if (nav != null)
        {
            var result = nav.GoTo(homeScene);
            if (result.IsSuccess) return;
            Debug.LogWarning($"[SceneSwitcher] GoTo({homeScene}) failed: {result.Code}");
        }

        if (!string.IsNullOrEmpty(homeSceneName))
            SceneManager.LoadScene(homeSceneName);
        else
            Debug.LogWarning("homeSceneName is empty!");
    }

    public void SwitchSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
