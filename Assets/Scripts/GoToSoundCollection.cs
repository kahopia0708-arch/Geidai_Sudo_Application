using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Geidai.Common.Models;
using Geidai.Services;
using Geidai.Services.Navigation;

/// <summary>ブラウンフィールド用 Collection 遷移。NavigationService.GoTo(Collection) 優先。</summary>
public class GoToMySoundLibraryButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private void Start()
    {
        if (button != null)
            button.onClick.AddListener(GoToMySoundCollection);

        if (!ServiceRegistry.IsRegistered<INavigationService>())
            ServiceRegistry.Register<INavigationService>(new NavigationService());
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(GoToMySoundCollection);
    }

    private void GoToMySoundCollection()
    {
        var nav = ServiceRegistry.Resolve<INavigationService>();
        if (nav != null)
        {
            var result = nav.GoTo(SceneId.Collection);
            if (result.IsSuccess) return;
        }

        SceneManager.LoadScene("GeidaiCollection");
    }
}
