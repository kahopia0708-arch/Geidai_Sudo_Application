using UnityEngine;
using UnityEngine.UI;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Common.UI;
using Geidai.Common.Utils;
using Geidai.Services;
using Geidai.Services.Navigation;

namespace Geidai.Foundation
{
    /// <summary>
    /// ホーム画面のコントローラ（HomeScreenController / US-NAV-02 / BR-10〜15 / nfr-design §3）。
    /// HomeMenuConfig の可視項目のみをデータ駆動で描画し、NavigationService 経由で遷移する。
    /// 端末バックは終了確認ダイアログ（既定=いいえ）で誤操作を防ぐ（NFR-05）。
    /// </summary>
    public class HomeScreenController : ScreenRootBase
    {
        [Header("U2 Home")]
        [SerializeField] private HomeMenuConfig menuConfig;
        [Tooltip("メニュー項目ボタンの配置先")]
        [SerializeField] private Transform menuContainer;
        [Tooltip("複製元のメニューボタン（Button＋子 Text を持つ枠）")]
        [SerializeField] private Button menuButtonPrefab;
        [SerializeField] private ErrorPresenter errorPresenter;
        [SerializeField] private ConfirmDialog confirmDialog;

        protected override void OnShow()
        {
            BuildMenu();
        }

        /// <summary>可視項目のみを order 昇順で描画する（BR-10）。</summary>
        public void BuildMenu()
        {
            if (menuConfig == null || menuContainer == null || menuButtonPrefab == null)
            {
                SafeLogger.Warn("[Home] menu not fully wired; skip build.");
                return;
            }

            // プレハブが非アクティブでもコンテナ／複製インスタンスは必ず表示する。
            menuContainer.gameObject.SetActive(true);

            for (int i = menuContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(menuContainer.GetChild(i).gameObject);
            }

            foreach (var item in menuConfig.VisibleSorted())
            {
                var button = Instantiate(menuButtonPrefab, menuContainer);
                button.gameObject.SetActive(true);
                button.gameObject.name = $"home-menu-{item.moduleId}";
                button.interactable = item.enabled;

                var label = button.GetComponentInChildren<Text>(true);
                if (label != null) label.text = item.label;

                var captured = item.moduleId;
                button.onClick.AddListener(() => Navigate(captured));
            }
        }

        private void Navigate(ModuleId moduleId)
        {
            var nav = ServiceRegistry.Resolve<INavigationService>();
            if (nav == null)
            {
                SafeLogger.Warn("[Home] navigation service not registered.");
                return;
            }

            var sceneId = ModuleRouter.ToSceneId(moduleId);
            var result = nav.GoTo(sceneId);
            if (!result.IsSuccess && errorPresenter != null)
            {
                errorPresenter.ShowWarning(result.Code == ResultCode.NotFound
                    ? "このボタンはいま じゅんびちゅうです。"
                    : "がめんをひらけませんでした。");
            }
        }

        /// <summary>ホームでの端末バックは終了確認（既定=いいえ）で誤操作を防ぐ（Q3=A / BR-15）。</summary>
        public override void OnBackPressed()
        {
            if (confirmDialog != null)
            {
                if (confirmDialog.IsOpen) return;
                confirmDialog.Show("おわる？", "アプリをおわりますか？", QuitApp, null);
            }
            else
            {
                QuitApp();
            }
        }

        private void QuitApp()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
