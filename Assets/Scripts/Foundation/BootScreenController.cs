using UnityEngine;
using UnityEngine.UI;
using Geidai.Common.UI;
using Geidai.Common.Utils;
using Geidai.Services;
using Geidai.Services.Storage;
using Geidai.Services.Navigation;

namespace Geidai.Foundation
{
    /// <summary>
    /// 起動画面のコントローラ（BootScreenController / nfr-design §1 / US-NAV-01 / BR-01〜04）。
    /// 軽量な状態機械（Idle→Checking→Routing/Error）。「はじめる」タップ後に
    /// プロフィールを読み、StartupRouter の決定に従って Register/Home へ遷移する。
    /// 判定はサービス層へ委譲し、UI は分岐のみを担う（子ども配慮の明示的開始 / Q2=A）。
    /// </summary>
    public class BootScreenController : ScreenRootBase
    {
        public enum BootState { Idle, Checking, Routing, Error }

        [Header("U2 Boot")]
        [SerializeField] private Button beginButton;
        [SerializeField] private ErrorPresenter errorPresenter;

        public BootState State { get; private set; } = BootState.Idle;

        private void Awake()
        {
            if (beginButton != null) beginButton.onClick.AddListener(OnBeginTapped);
        }

        private void OnDestroy()
        {
            if (beginButton != null) beginButton.onClick.RemoveListener(OnBeginTapped);
        }

        /// <summary>「はじめる」タップ起点（明示的開始 / Q2=A）。</summary>
        public void OnBeginTapped()
        {
            if (State != BootState.Idle) return;
            State = BootState.Checking;
            RouteByProfile();
        }

        private void RouteByProfile()
        {
            var storage = ServiceRegistry.Resolve<IStorageService>();
            var nav = ServiceRegistry.Resolve<INavigationService>();

            if (storage == null || nav == null)
            {
                State = BootState.Error;
                SafeLogger.Warn("[Boot] services not registered; routing to Register.");
                if (nav != null) nav.GoTo(Common.Models.SceneId.Register);
                return;
            }

            var load = storage.LoadProfile();
            var decision = StartupRouter.Resolve(load);

            if (decision.ShowFallbackWarning)
            {
                State = BootState.Error;
                if (errorPresenter != null)
                    errorPresenter.ShowWarning("データをよみこめませんでした。さいしょからはじめます。");
                SafeLogger.Warn($"[Boot] profile load fallback: {load.Code}");
            }

            State = BootState.Routing;
            var result = nav.GoTo(decision.Target);
            if (!result.IsSuccess)
            {
                State = BootState.Error;
                if (errorPresenter != null) errorPresenter.ShowFromResult(result);
                SafeLogger.Warn($"[Boot] navigation failed: {result.Code} {result.Message}");
            }
        }
    }
}
