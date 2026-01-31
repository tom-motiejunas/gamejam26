using UnityEngine;
using UnityEngine.SceneManagement;

public class WinUI : MonoBehaviour
{
    [Tooltip("The name of the Main Menu scene.")]
    public string mainMenuSceneName = "title_screen";

    public void LoadMainMenu()
    {
        // Check if the scene exists to avoid errors, or catch the error log
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("WinUI: Main Menu scene name is empty!");
            // Fallback to index 0 which is usually the Main Menu
            SceneManager.LoadScene(0);
        }
    }
}
