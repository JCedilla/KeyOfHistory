using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pauseMenuUI;
    public GameObject confirmMenuPanel;
    public GameObject settingsPanel;

    public static bool GameIsPaused = false;

    private AudioSource[] allAudioSources;

    [Header("Canvas References")]
[SerializeField] private GameObject GameplayCanvas;

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (settingsPanel.activeSelf)
            {
                // If settings panel is open, go back to pause menu
                BackToPauseMenu();
            }
            else if (confirmMenuPanel.activeSelf)
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
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;

        if (allAudioSources != null)
        {
            foreach (AudioSource audio in allAudioSources)
                audio.UnPause();
        }

         if (GameplayCanvas != null)
        GameplayCanvas.SetActive(true);

    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        confirmMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        Time.timeScale = 0f;
        GameIsPaused = true;

        allAudioSources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource audio in allAudioSources)
            audio.Pause();

         if (GameplayCanvas != null)
        GameplayCanvas.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Called when player clicks "Settings" button in Pause Menu
    public void ShowSettingsPanel()
    {
        pauseMenuUI.SetActive(false);
        confirmMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // Called when player clicks "Back" button in Settings Panel
    public void BackToPauseMenu()
    {
        settingsPanel.SetActive(false);
        confirmMenuPanel.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    // Called when player clicks "Return to Menu" in Pause Menu
    public void ShowConfirmMenu()
    {
        pauseMenuUI.SetActive(false);
        settingsPanel.SetActive(false);
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
        settingsPanel.SetActive(false);
        pauseMenuUI.SetActive(true);
    }
}