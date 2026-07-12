using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoToPlaceButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(GoToPlace);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(GoToPlace);
        }
    }

    private void GoToPlace()
    {
        SceneManager.LoadScene("place");
    }
}