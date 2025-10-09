using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pauseMenuUI;
    public GameObject confirmMenuPanel;

    public static bool GameIsPaused = false;

    private AudioSource[] allAudioSources;

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (confirmMenuPanel.activeSelf)
            {
                // If confirm panel is open, cancel it and go back to pause menu
                CancelReturnToMenu();
            }
            else if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        confirmMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;

        if (allAudioSources != null)
        {
            foreach (AudioSource audio in allAudioSources)
                audio.UnPause();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        confirmMenuPanel.SetActive(false);
        Time.timeScale = 0f;
        GameIsPaused = true;

        allAudioSources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource audio in allAudioSources)
            audio.Pause();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Called when player clicks "Return to Menu" in Pause Menu
    public void ShowConfirmMenu()
    {
        pauseMenuUI.SetActive(false);
        confirmMenuPanel.SetActive(true);
    }

    public void ConfirmReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Change to your menu scene name
    }

    public void CancelReturnToMenu()
    {
        confirmMenuPanel.SetActive(false);
        pauseMenuUI.SetActive(true);
    }
}
