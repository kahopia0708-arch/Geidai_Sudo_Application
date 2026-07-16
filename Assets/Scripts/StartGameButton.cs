using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Geidai.Common.Models;
using Geidai.Common.Results;
using Geidai.Services;
using Geidai.Services.Navigation;

/// <summary>
/// 既存ゲーム選択 UI（game_Home）からゲームへ遷移するブラウンフィールド橋（U6 MCP フォローアップ）。
/// 既定は <see cref="INavigationService.GoTo"/>(Game1)。未登録時のみ sceneName へフォールバック。
/// </summary>
public class StartGameButton : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [Tooltip("NavigationService 未登録時のフォールバック（通常は空でよい）。")]
    [SerializeField] private string sceneName;
    [SerializeField] private SceneId targetScene = SceneId.Game1;

    private void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(LoadGameScene);

        // シーンに AppManager が無い場合でも遷移できるよう最低限のナビを確保。
        if (!ServiceRegistry.IsRegistered<INavigationService>())
            ServiceRegistry.Register<INavigationService>(new NavigationService());
    }

    private void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(LoadGameScene);
    }

    private void LoadGameScene()
    {
        var nav = ServiceRegistry.Resolve<INavigationService>();
        if (nav != null)
        {
            Result result = nav.GoTo(targetScene);
            if (result.IsSuccess) return;
            Debug.LogWarning($"[StartGameButton] GoTo({targetScene}) failed: {result.Code} {result.Message}");
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("移動先のScene名が設定されていません。");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
