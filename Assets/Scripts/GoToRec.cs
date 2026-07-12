using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoToRecButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(GoToRec);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(GoToRec);
        }
    }

    private void GoToRec()
    {
        SceneManager.LoadScene("Rec");
    }
}