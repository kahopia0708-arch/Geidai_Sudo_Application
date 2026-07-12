using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ReturnHomeButton : MonoBehaviour
{
    [SerializeField] private Button returnButton;
    [SerializeField] private string homeSceneName = "Home";

    private void Start()
    {
        if (returnButton != null)
        {
            returnButton.onClick.AddListener(ReturnHome);
        }
    }

    private void OnDestroy()
    {
        if (returnButton != null)
        {
            returnButton.onClick.RemoveListener(ReturnHome);
        }
    }

    private void ReturnHome()
    {
        if (string.IsNullOrEmpty(homeSceneName))
        {
            Debug.LogWarning("Home");
            return;
        }

        SceneManager.LoadScene(homeSceneName);
    }
}