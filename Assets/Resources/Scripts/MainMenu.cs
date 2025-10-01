using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        // Loads your GameScene by name
        SceneManager.LoadScene("GameScene");
        Debug.Log("I have been pressed!");
    }

    public void QuitGame()
    {
        Debug.Log("Game is quitting..."); // Works in Editor
        Application.Quit(); // Works in a build
        Debug.Log("I have been pressed!");
    }
}
