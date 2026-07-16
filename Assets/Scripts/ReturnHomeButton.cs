using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Geidai.Common.Models;
using Geidai.Services;
using Geidai.Services.Navigation;

/// <summary>
/// ブラウンフィールド用「ホームへ戻る」。NavigationService 優先、未登録時はシーン名フォールバック。
/// </summary>
public class ReturnHomeButton : MonoBehaviour
{
    [SerializeField] private Button returnButton;
    [SerializeField] private string homeSceneName = "GeidaiHome";
    [SerializeField] private SceneId homeScene = SceneId.Home;

    private void Start()
    {
        if (returnButton != null)
            returnButton.onClick.AddListener(ReturnHome);

        if (!ServiceRegistry.IsRegistered<INavigationService>())
            ServiceRegistry.Register<INavigationService>(new NavigationService());
    }

    private void OnDestroy()
    {
        if (returnButton != null)
            returnButton.onClick.RemoveListener(ReturnHome);
    }

    private void ReturnHome()
    {
        var nav = ServiceRegistry.Resolve<INavigationService>();
        if (nav != null)
        {
            var result = nav.GoTo(homeScene);
            if (result.IsSuccess) return;
        }

        if (string.IsNullOrEmpty(homeSceneName))
        {
            Debug.LogWarning("homeSceneName is empty");
            return;
        }

        SceneManager.LoadScene(homeSceneName);
    }
}
