using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public static bool GameIsPaused = false;

    private AudioSource[] allAudioSources;

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (GameIsPaused) Resume();
            else Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;

        if (allAudioSources != null)
        {
            foreach (AudioSource audio in allAudioSources)
            {
                audio.UnPause();
            }
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;

        allAudioSources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource audio in allAudioSources)
        {
            audio.Pause();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Instead of quitting the app, load Main Menu
    public void QuitToMenu()
    {
        Time.timeScale = 1f; // reset time scale
        SceneManager.LoadScene("MainMenu"); // <-- replace with your main menu scene name
    }
}
