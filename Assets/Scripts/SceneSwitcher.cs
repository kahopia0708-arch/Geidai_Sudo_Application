using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [Header("移動先のシーン名")]
    [SerializeField] private string sceneName = "Home";

    [Header("ホーム画面のシーン名")]
    [SerializeField] private string homeSceneName = "Home";

    /// <summary>
    /// ボタンから呼び出すメソッド
    /// </summary>
    public void SwitchScene()
    {
        Debug.Log("SwitchScene called. SceneName: " + sceneName);
        
        if (!string.IsNullOrEmpty(sceneName))
        {
            Debug.Log("Loading scene: " + sceneName);
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("sceneName is empty!");
        }
    }

    /// <summary>
    /// ホーム画面へ戻る
    /// </summary>
    public void ReturnHome()
    {
        Debug.Log("ReturnHome called. HomeSceneName: " + homeSceneName);

        if (!string.IsNullOrEmpty(homeSceneName))
        {
            Debug.Log("Loading home scene: " + homeSceneName);
            SceneManager.LoadScene(homeSceneName);
        }
        else
        {
            Debug.LogWarning("homeSceneName is empty!");
        }
    }

    /// <summary>
    /// インデックスでシーン移動
    /// </summary>
    public void SwitchSceneByIndex(int sceneIndex)
    {
        Debug.Log("SwitchSceneByIndex called. Index: " + sceneIndex);
        SceneManager.LoadScene(sceneIndex);
    }
}