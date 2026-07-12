using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoToMySoundLibraryButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(GoToMySoundCollection);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(GoToMySoundCollection);
        }
    }

    private void GoToMySoundCollection()
    {
        SceneManager.LoadScene("MySoundCollection");
    }
}