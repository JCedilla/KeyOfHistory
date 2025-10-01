using UnityEngine;
using System.Collections;

public class MusicFader : MonoBehaviour
{
    public AudioSource musicSource;  // Drag your AudioSource here
    public float fadeDuration = 2f;  // Fade in duration
    public float targetVolume = 1f;  // Final music volume

    void Start()
    {
        if (musicSource != null)
        {
            musicSource.volume = 0f;  // Start silent
            musicSource.Play();       // Start playing
            StartCoroutine(FadeIn());
        }
    }

    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / fadeDuration);
            yield return null;
        }
        musicSource.volume = targetVolume; // Ensure final volume is exact
    }
}
