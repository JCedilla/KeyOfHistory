using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        // Start a coroutine that waits before loading
        StartCoroutine(LoadGameAfterDelay());
    }

    private System.Collections.IEnumerator LoadGameAfterDelay()
    {
        yield return new WaitForSeconds(1f); // ⏳ wait 1 second
        SceneManager.LoadScene("GameScene"); // replace with your scene name
    }

    public void QuitGame()
    {
        Debug.Log("Game is quitting..."); // Works in Editor
        Application.Quit(); // Works in a build
        Debug.Log("I have been pressed!");
    }
}
