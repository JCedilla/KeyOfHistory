using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject confirmQuitPanel; // The confirmation box
    public GameObject playButton;        // Assign in Inspector
    public GameObject quitButton;        // Assign in Inspector
    public GameObject titleImage;        // 👈 Assign your TitleImage here

    public void PlayGame()
    {
        StartCoroutine(LoadGameAfterDelay());
    }

    private System.Collections.IEnumerator LoadGameAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        // Show confirm panel, hide main menu elements
        confirmQuitPanel.SetActive(true);
        playButton.SetActive(false);
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
        // Hide confirm panel, restore main menu elements
        confirmQuitPanel.SetActive(false);
        playButton.SetActive(true);
        quitButton.SetActive(true);
        titleImage.SetActive(true);
    }
}
