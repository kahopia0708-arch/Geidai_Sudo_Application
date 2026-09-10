using UnityEngine;
using UnityEngine.UI;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Common.UI;
using Geidai.Common.Utils;
using Geidai.Services;
using Geidai.Services.Navigation;
using Geidai.Services.Storage;

namespace Geidai.Foundation
{
    /// <summary>
    /// ホーム画面のコントローラ（HomeScreenController / US-NAV-02 / BR-10〜15 / nfr-design §3）。
    /// HomeMenuConfig の可視項目のみをデータ駆動で描画し、NavigationService 経由で遷移する。
    /// 起動シーンとしてプロフィール未登録時は Register へ自動遷移（FR-HOME-06）。
    /// </summary>
    public class HomeScreenController : ScreenRootBase
    {
        private static readonly Color HomeBackgroundColor = HomeUiTheme.Background;

        [Header("U2 Home")]
        [SerializeField] private HomeMenuConfig menuConfig;
        [SerializeField] private HomeMenuIconCatalog iconCatalog;
        [SerializeField] private Transform menuContainer;
        [SerializeField] private Button menuButtonPrefab;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private HomeProfileBadgeView profileBadge;
        [SerializeField] private HomeProfilePanelView profilePanel;
        [SerializeField] private ErrorPresenter errorPresenter;
        [SerializeField] private ConfirmDialog confirmDialog;

        private bool _startupGateHandled;

        private void Awake()
        {
            if (profileBadge != null && profileBadge.Button != null)
                profileBadge.Button.onClick.AddListener(OnProfileBadgeTapped);

            if (profilePanel != null)
            {
                profilePanel.SettingsRequested += OnProfileSettingsRequested;
                profilePanel.CloseRequested += OnProfilePanelClose;
            }
        }

        private void OnDestroy()
        {
            if (profileBadge != null && profileBadge.Button != null)
                profileBadge.Button.onClick.RemoveListener(OnProfileBadgeTapped);

            if (profilePanel != null)
            {
                profilePanel.SettingsRequested -= OnProfileSettingsRequested;
                profilePanel.CloseRequested -= OnProfilePanelClose;
            }
        }

        protected override void OnShow()
        {
            ApplyBackground();
            if (!EnsureStartupGate()) return;

            RefreshProfileUi();
            ApplyChromeSprites();
            BuildMenu();
        }

        private void ApplyChromeSprites()
        {
            var pill = HomeUiImageUtil.ResolvePillSprite(iconCatalog);
            if (profileBadge != null) profileBadge.ApplyChrome(pill);
            if (profilePanel != null) profilePanel.Initialize(iconCatalog);
        }

        private void ApplyBackground()
        {
            if (backgroundImage != null)
                backgroundImage.color = HomeBackgroundColor;
        }

        /// <summary>プロフィール未登録時は Register へ（Boot 廃止 / FR-HOME-06）。</summary>
        private bool EnsureStartupGate()
        {
            if (_startupGateHandled) return true;
            _startupGateHandled = true;

            var storage = ServiceRegistry.Resolve<IStorageService>();
            var nav = ServiceRegistry.Resolve<INavigationService>();
            if (storage == null || nav == null)
            {
                SafeLogger.Warn("[Home] services not registered; skip startup gate.");
                return true;
            }

            var load = storage.LoadProfile();
            var decision = HomeStartupGate.Evaluate(load);
            if (decision.Target == SceneId.Home) return true;

            if (decision.ShowFallbackWarning && errorPresenter != null)
                errorPresenter.ShowWarning("データをよみこめませんでした。さいしょからはじめます。");

            nav.GoTo(decision.Target);
            return false;
        }

        private void RefreshProfileUi()
        {
            string nickname = LoadNickname();
            bool hasProfile = !string.IsNullOrEmpty(nickname);

            if (profileBadge != null)
            {
                profileBadge.SetVisible(hasProfile);
                if (hasProfile)
                {
                    profileBadge.SetNickname(nickname);
                    profileBadge.ShowDecorativeProgress();
                }
            }

            if (profilePanel != null && profilePanel.IsOpen)
                profilePanel.Show(nickname);
        }

        private static string LoadNickname()
        {
            var storage = ServiceRegistry.Resolve<IStorageService>();
            if (storage == null) return string.Empty;

            var load = storage.LoadProfile();
            if (!load.IsSuccess || load.Value == null) return string.Empty;
            return load.Value.nickname ?? string.Empty;
        }

        /// <summary>
        /// 可視項目のみを order 昇順で描画する（BR-10）。
        /// Edit Mode でも呼べる（プレビュー用。遷移は Play 時のみ配線）。
        /// </summary>
        public void BuildMenu()
        {
            if (menuConfig == null || menuContainer == null || menuButtonPrefab == null)
            {
                SafeLogger.Warn("[Home] menu not fully wired; skip build.");
                return;
            }

            menuContainer.gameObject.SetActive(true);
            ClearMenuChildren();

            foreach (var item in menuConfig.VisibleSorted())
            {
                var button = Instantiate(menuButtonPrefab, menuContainer);
                button.gameObject.SetActive(true);
                button.gameObject.name = $"home-menu-{item.moduleId}";
                button.interactable = item.enabled;

                if (!Application.isPlaying)
                    button.gameObject.hideFlags = HideFlags.DontSave;

                var view = button.GetComponent<HomeMenuButtonView>();
                if (view != null)
                {
                    view.ApplyChrome(HomeUiImageUtil.ResolvePillSprite(iconCatalog));
                    view.Apply(item.label, iconCatalog != null ? iconCatalog.Resolve(item.iconKey) : null);
                }
                else
                {
                    var label = button.GetComponentInChildren<Text>(true);
                    if (label != null) label.text = item.label;
                }

                if (Application.isPlaying)
                {
                    var captured = item.moduleId;
                    button.onClick.AddListener(() => Navigate(captured));
                }
            }
        }

        /// <summary>Edit Mode プレビュー用。メニュー子を消す。</summary>
        public void ClearMenuPreview()
        {
            if (menuContainer == null) return;
            ClearMenuChildren();
        }

        private void ClearMenuChildren()
        {
            if (menuContainer == null) return;
            for (int i = menuContainer.childCount - 1; i >= 0; i--)
            {
                var go = menuContainer.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(go);
                else DestroyImmediate(go);
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

        private void OnProfileBadgeTapped()
        {
            if (profilePanel == null) return;
            if (profileBadge != null) profileBadge.SetVisible(false);
            profilePanel.Show(LoadNickname());
        }

        private void OnProfilePanelClose()
        {
            if (profilePanel != null) profilePanel.Hide();
            RefreshProfileUi();
        }

        private void OnProfileSettingsRequested()
        {
            var nav = ServiceRegistry.Resolve<INavigationService>();
            if (nav == null) return;

            if (profilePanel != null) profilePanel.Hide();
            var result = nav.GoTo(SceneId.Register);
            if (!result.IsSuccess && errorPresenter != null)
                errorPresenter.ShowFromResult(result);
        }

        /// <summary>ホームでの端末バックは終了確認（既定=いいえ）で誤操作を防ぐ（Q3=A / BR-15）。</summary>
        public override void OnBackPressed()
        {
            if (profilePanel != null && profilePanel.IsOpen)
            {
                profilePanel.Hide();
                return;
            }

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
