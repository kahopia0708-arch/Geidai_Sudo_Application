using UnityEngine;
using UnityEngine.UI;
using Geidai.Common.Models;
using Geidai.Common.UI;
using Geidai.Common.Utils;
using Geidai.Services;
using Geidai.Services.Navigation;

namespace Geidai.Foundation
{
    /// <summary>
    /// おとあそび（ゲーム選択）画面。Geidai 配下の新シーン用（FR-HOME-07）。
    /// </summary>
    public class GameSelectScreenController : ScreenRootBase
    {
        [SerializeField] private Button game1Button;
        [SerializeField] private Button backButton;
        [SerializeField] private ErrorPresenter errorPresenter;

        private void Awake()
        {
            if (game1Button != null) game1Button.onClick.AddListener(GoGame1);
            if (backButton != null) backButton.onClick.AddListener(GoHome);
        }

        private void OnDestroy()
        {
            if (game1Button != null) game1Button.onClick.RemoveListener(GoGame1);
            if (backButton != null) backButton.onClick.RemoveListener(GoHome);
        }

        private void GoGame1()
        {
            Navigate(SceneId.Game1);
        }

        private void GoHome()
        {
            Navigate(SceneId.Home);
        }

        private void Navigate(SceneId sceneId)
        {
            var nav = ServiceRegistry.Resolve<INavigationService>();
            if (nav == null)
            {
                SafeLogger.Warn("[GameSelect] navigation service not registered.");
                return;
            }

            var result = nav.GoTo(sceneId);
            if (!result.IsSuccess && errorPresenter != null)
                errorPresenter.ShowFromResult(result);
        }
    }
}
