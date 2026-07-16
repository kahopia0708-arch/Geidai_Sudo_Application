using UnityEngine;
using UnityEngine.UI;
using Geidai.Common.Content;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Common.UI;
using Geidai.Services;
using Geidai.Services.Content;
using Geidai.Services.Navigation;

namespace Geidai.Theme
{
    /// <summary>
    /// 「今週のお題」を表示する再利用部品（U5 / P5 / frontend-components）。
    /// 専用 Theme 画面にもホーム上部バナーにも配置できる（両対応の土台 / US-TECH-07）。
    /// 取得は IContentService、お題タップは ThemeContext 設定→Rec 遷移（失敗は ErrorPresenter）。
    /// 見た目/文言/レイアウトは Sさん がシーンで調整可能（ロジックは表示に非依存）。
    /// </summary>
    public class WeeklyThemeController : MonoBehaviour
    {
        [Header("お題テキスト")]
        [SerializeField] private Text themeText;
        [SerializeField] private Text readingText;
        [SerializeField] private Text hintText;

        [Header("導線・状態")]
        [SerializeField] private Button recordButton;
        [SerializeField] private GameObject emptyState;
        [SerializeField] private ErrorPresenter errorPresenter;

        [Tooltip("お題カタログを直接指定する場合に設定（未設定なら登録済み IContentService を使用）。")]
        [SerializeField] private ThemeCatalog catalog;

        private IContentService _content;
        private ThemeContext _themeContext;
        private INavigationService _nav;
        private ThemeItem _current;

        private void Start()
        {
            EnsureWired();
            if (recordButton != null) recordButton.onClick.AddListener(OnRecordPressed);
            Refresh();
        }

        private void OnDestroy()
        {
            if (recordButton != null) recordButton.onClick.RemoveListener(OnRecordPressed);
        }

        private void EnsureWired()
        {
            _content = ThemeBootstrap.EnsureContentService(catalog);
            _themeContext = ThemeBootstrap.EnsureThemeContext();
            _nav = ServiceRegistry.Resolve<INavigationService>();
        }

        /// <summary>今週のお題を取得し UI に反映する（表示時/手動更新）。</summary>
        public void Refresh()
        {
            if (_content == null) EnsureWired();

            Result<ThemeItem> result = _content != null
                ? _content.GetCurrentTheme()
                : Result<ThemeItem>.Fail(ResultCode.NotFound, "おだいが まだ ないよ");

            if (!result.IsSuccess || result.Value == null)
            {
                ShowEmpty();
                return;
            }

            _current = result.Value;
            ShowTheme(_current);
        }

        private void ShowTheme(ThemeItem item)
        {
            if (emptyState != null) emptyState.SetActive(false);

            if (themeText != null)
            {
                themeText.gameObject.SetActive(true);
                themeText.text = item.text;
            }
            if (readingText != null)
            {
                bool has = !string.IsNullOrWhiteSpace(item.reading);
                readingText.gameObject.SetActive(has);
                readingText.text = has ? item.reading : string.Empty;
            }
            if (hintText != null)
            {
                bool has = !string.IsNullOrWhiteSpace(item.hint);
                hintText.gameObject.SetActive(has);
                hintText.text = has ? item.hint : string.Empty;
            }
            if (recordButton != null) recordButton.interactable = true;
        }

        private void ShowEmpty()
        {
            _current = null;
            if (themeText != null) themeText.gameObject.SetActive(false);
            if (readingText != null) readingText.gameObject.SetActive(false);
            if (hintText != null) hintText.gameObject.SetActive(false);
            if (recordButton != null) recordButton.interactable = false;
            if (emptyState != null) emptyState.SetActive(true);
        }

        private void OnRecordPressed()
        {
            if (_current == null) return;

            // お題を受け渡してから Rec へ遷移（BR-THEME-31/32）。
            _themeContext?.Set(_current);

            if (_nav == null) _nav = ServiceRegistry.Resolve<INavigationService>();
            if (_nav == null)
            {
                if (errorPresenter != null) errorPresenter.ShowError("がめんを ひらけなかったよ");
                return;
            }

            Result result = _nav.GoTo(SceneId.Rec);
            if (!result.IsSuccess && errorPresenter != null) errorPresenter.ShowFromResult(result);
        }
    }
}
