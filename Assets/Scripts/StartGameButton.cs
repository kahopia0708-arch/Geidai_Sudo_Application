using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGameButton : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private string sceneName;

    private void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(LoadGameScene);
        }
    }

    private void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(LoadGameScene);
        }
    }

    private void LoadGameScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("移動先のScene名が設定されていません。");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}