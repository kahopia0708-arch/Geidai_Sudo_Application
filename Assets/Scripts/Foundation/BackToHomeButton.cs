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
    /// モジュール画面（Rec/コレクション/ゲーム 等）の「もどる/ホーム」導線（BR-15）。
    /// 既存 ReturnHomeButton の後継。直接 SceneManager を呼ばず NavigationService 経由で遷移する（NFR-08）。
    /// 各モジュールシーンのボタンにアタッチして再利用する。
    /// </summary>
    public class BackToHomeButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private ErrorPresenter errorPresenter;

        private void Start()
        {
            if (button == null) button = GetComponent<Button>();
            if (button != null) button.onClick.AddListener(GoHome);
        }

        private void OnDestroy()
        {
            if (button != null) button.onClick.RemoveListener(GoHome);
        }

        public void GoHome()
        {
            var nav = ServiceRegistry.Resolve<INavigationService>();
            if (nav == null)
            {
                SafeLogger.Warn("[BackToHome] navigation service not registered.");
                return;
            }

            var result = nav.GoTo(SceneId.Home);
            if (!result.IsSuccess && errorPresenter != null) errorPresenter.ShowFromResult(result);
        }
    }
}
