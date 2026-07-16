using UnityEngine;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Common.UI;
using Geidai.Services;
using Geidai.Services.Navigation;

namespace Geidai.Theme
{
    /// <summary>
    /// 専用「お題」画面（SceneId.Theme / U5 / P5）。ScreenRootBase を継承し、
    /// 表示時に WeeklyThemeController を更新、端末バック/戻るでホームへ遷移する。
    /// レスポンシブ/SafeArea は基底が適用。実配置・意匠は Sさん がシーンで調整（US-TECH-07）。
    /// </summary>
    public class WeeklyThemeScreenController : ScreenRootBase
    {
        [SerializeField] private WeeklyThemeController themeController;
        [SerializeField] private ErrorPresenter errorPresenter;

        protected override void OnShow()
        {
            if (themeController != null) themeController.Refresh();
        }

        public override void OnBackPressed()
        {
            var nav = ServiceRegistry.Resolve<INavigationService>();
            if (nav == null)
            {
                base.OnBackPressed();
                return;
            }

            Result result = nav.GoTo(SceneId.Home);
            if (!result.IsSuccess && errorPresenter != null) errorPresenter.ShowFromResult(result);
        }
    }
}
