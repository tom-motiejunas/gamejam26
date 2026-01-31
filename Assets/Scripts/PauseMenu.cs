using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject pausePanel;

    [Header("Settings")]
    public string mainMenuSceneName = "title_screen";

    private bool _isPaused = false;

    void Start()
    {
        // Ensure pause menu is hidden at start
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        
        Time.timeScale = 1f; // Ensure time is normal
        _isPaused = false;
    }

    void Update()
    {
        // Toggle pause on Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        _isPaused = true;
        Time.timeScale = 0f; // Freeze time
        if (pausePanel != null) pausePanel.SetActive(true);
        
        // Ensure cursor is visible/unlocked for menu interaction
        // If you are using a custom cursor system, you might need to force it here to be "Normal"
        // drawingManager might confuse things if it's locking cursor, but usually standard UI works fine
    }

    public void ResumeGame()
    {
        _isPaused = false;
        Time.timeScale = 1f; // Resume time
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Always resume time before leaving
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
