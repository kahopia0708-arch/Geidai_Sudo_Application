using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Geidai.Common.Models;
using Geidai.Services;
using Geidai.Services.Navigation;

/// <summary>ブラウンフィールド用 Rec 遷移。NavigationService.GoTo(Rec) 優先。</summary>
public class GoToRecButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private void Start()
    {
        if (button != null)
            button.onClick.AddListener(GoToRec);

        if (!ServiceRegistry.IsRegistered<INavigationService>())
            ServiceRegistry.Register<INavigationService>(new NavigationService());
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(GoToRec);
    }

    private void GoToRec()
    {
        var nav = ServiceRegistry.Resolve<INavigationService>();
        if (nav != null)
        {
            var result = nav.GoTo(SceneId.Rec);
            if (result.IsSuccess) return;
        }

        SceneManager.LoadScene("GeidaiRec");
    }
}
