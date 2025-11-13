using UnityEngine;
using UnityEngine.SceneManagement;
using KeyOfHistory.Manager;

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject confirmQuitPanel;
    public GameObject settingsPanel;     // NEW - Assign your SettingsPanel
    public GameObject playButton;
    public GameObject settingsButton;    // NEW - Assign Settings button
    public GameObject quitButton;
    public GameObject titleImage;

    private void Start()
    {
        // Make sure settings panel is hidden at start
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void PlayGame()
    {
        // Use LoadingScreen if available, otherwise fallback to direct load
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.LoadScene("GameScene");
        }
        else
        {
            StartCoroutine(LoadGameAfterDelay());
        }
    }

    private System.Collections.IEnumerator LoadGameAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("GameScene");
    }

    // NEW - Open Settings
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        playButton.SetActive(false);
        if (settingsButton != null) settingsButton.SetActive(false);
        quitButton.SetActive(false);
        titleImage.SetActive(false);
    }

    // NEW - Close Settings (called by Back button in settings)
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        playButton.SetActive(true);
        if (settingsButton != null) settingsButton.SetActive(true);
        quitButton.SetActive(true);
        titleImage.SetActive(true);
    }

    public void QuitGame()
    {
        confirmQuitPanel.SetActive(true);
        playButton.SetActive(false);
        if (settingsButton != null) settingsButton.SetActive(false);
        quitButton.SetActive(false);
        titleImage.SetActive(false);
    }

    public void ConfirmQuit()
    {
        Debug.Log("Game is quitting...");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void CancelQuit()
    {
        confirmQuitPanel.SetActive(false);
        playButton.SetActive(true);
        if (settingsButton != null) settingsButton.SetActive(true);
        quitButton.SetActive(true);
        titleImage.SetActive(true);
    }
}