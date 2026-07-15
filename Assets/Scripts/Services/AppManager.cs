using UnityEngine;
using Geidai.Common.Models;
using Geidai.Common.Utils;
using Geidai.Services.Storage;
using Geidai.Services.Navigation;
using Geidai.Services.Content;

namespace Geidai.Services
{
    /// <summary>
    /// アプリ起動のオーケストレーション（logical-components §2.1）。
    /// サービスを ServiceRegistry へ登録し、初回起動判定に基づき最初の遷移を決める。
    /// U1 ではシーンが未整備のため、既定では遷移を行わない（navigateOnStart=false）。
    /// </summary>
    public class AppManager : MonoBehaviour
    {
        [SerializeField] private bool navigateOnStart = false;

        private void Awake()
        {
            Bootstrap();
        }

        public void Bootstrap()
        {
            if (!ServiceRegistry.IsRegistered<IStorageService>())
                ServiceRegistry.Register<IStorageService>(new StorageService());
            if (!ServiceRegistry.IsRegistered<INavigationService>())
                ServiceRegistry.Register<INavigationService>(new NavigationService());
            if (!ServiceRegistry.IsRegistered<IContentService>())
                ServiceRegistry.Register<IContentService>(new ContentService());

            SafeLogger.Log("[AppManager] services registered.");

            if (navigateOnStart)
                NavigateToInitialScene();
        }

        /// <summary>プロフィールの有無で初回遷移先を決める（BR-13）。</summary>
        public SceneId ResolveInitialScene()
        {
            var storage = ServiceRegistry.Resolve<IStorageService>();
            if (storage == null) return SceneId.Register;

            var result = storage.LoadProfile();
            return result.IsSuccess && result.Value != null ? SceneId.Home : SceneId.Register;
        }

        private void NavigateToInitialScene()
        {
            var nav = ServiceRegistry.Resolve<INavigationService>();
            if (nav == null) return;

            var target = ResolveInitialScene();
            var result = nav.GoTo(target);
            if (!result.IsSuccess)
                SafeLogger.Warn($"[AppManager] initial navigation failed: {result.Code} {result.Message}");
        }
    }
}
