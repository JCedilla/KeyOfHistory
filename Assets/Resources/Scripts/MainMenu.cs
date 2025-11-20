using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using KeyOfHistory.Manager;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject playPanel;
    public GameObject optionsPanel;
    public GameObject confirmQuitPanel;
    public GameObject settingsPanel;
    
    [Header("UI Elements")]
    public TextMeshProUGUI gameTitle;
    
    [Header("Transition Settings")]
    public float fadeDuration = 0.5f;

    // ---------- AUDIO ----------
    [Header("Audio")]
    public AudioSource musicSource;      // background music
    public AudioSource sfxSource;        // button + transition SFX

    public AudioClip mainTheme;          // looped menu music
    public AudioClip buttonClickSfx;     // short click sound
    public AudioClip transitionSfx;      // whoosh / fade sound

    public float musicFadeDuration = 1f; // fade when starting game
    // ---------------------------

    private CanvasGroup mainPanelGroup;
    private CanvasGroup playPanelGroup;
    private CanvasGroup optionsPanelGroup;
    private CanvasGroup settingsPanelGroup;
    private CanvasGroup confirmQuitPanelGroup;
    private CanvasGroup titleGroup;
    
    void Start()
    {
        // Add CanvasGroups if they don't exist
        mainPanelGroup = GetOrAddCanvasGroup(mainPanel);
        playPanelGroup = GetOrAddCanvasGroup(playPanel);
        optionsPanelGroup = GetOrAddCanvasGroup(optionsPanel);
        settingsPanelGroup = GetOrAddCanvasGroup(settingsPanel);
        confirmQuitPanelGroup = GetOrAddCanvasGroup(confirmQuitPanel);
        titleGroup = GetOrAddCanvasGroup(gameTitle.gameObject);
        
        // Make sure only main panel is visible at start
        ShowMainMenu();

        // Start menu music
        StartMenuMusic();
    }
    
    CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        CanvasGroup group = obj.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = obj.AddComponent<CanvasGroup>();
        }
        return group;
    }

    // ---------- AUDIO HELPERS ----------
    void StartMenuMusic()
    {
        if (musicSource != null && mainTheme != null)
        {
            musicSource.clip = mainTheme;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    void PlayClickSfx()
    {
        if (sfxSource != null && buttonClickSfx != null)
        {
            sfxSource.PlayOneShot(buttonClickSfx);
        }
    }

    void PlayTransitionSfx()
    {
        if (sfxSource != null && transitionSfx != null)
        {
            sfxSource.PlayOneShot(transitionSfx);
        }
    }

    System.Collections.IEnumerator FadeMusic(float targetVolume)
    {
        if (musicSource == null) yield break;

        float startVol = musicSource.volume;
        float t = 0f;

        while (t < musicFadeDuration)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVol, targetVolume, t / musicFadeDuration);
            yield return null;
        }

        musicSource.volume = targetVolume;

        if (Mathf.Approximately(targetVolume, 0f))
            musicSource.Stop();
    }
    // ----------------------------------
    
    // === MAIN MENU BUTTONS ===
    public void OnPlayButtonClicked()
    {
        PlayClickSfx();
        StartCoroutine(TransitionToPanel(playPanel, playPanelGroup));
    }
    
    public void OnOptionsButtonClicked()
    {
        PlayClickSfx();
        StartCoroutine(TransitionToPanel(optionsPanel, optionsPanelGroup));
    }
    
    public void OnQuitButtonClicked()
    {
        PlayClickSfx();
        StartCoroutine(ShowConfirmQuitPanel());
    }

    public void OnConfirmQuit()
    {
        PlayClickSfx();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void OnCancelQuit()
    {
        PlayClickSfx();
        StartCoroutine(HideConfirmQuitPanel());
    }
    
    // === PLAY PANEL BUTTONS ===
    public void OnNewGameClicked()
    {
        PlayClickSfx();
        StartCoroutine(NewGameRoutine());
    }

    private System.Collections.IEnumerator NewGameRoutine()
    {
        // fade music out first
        yield return StartCoroutine(FadeMusic(0f));

        // then start loading via LoadingScreen (async with UI)
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.LoadScene("GameScene");
        }
        else
        {
            // fallback if something is wrong
            SceneManager.LoadScene("GameScene");
        }
    }
    
    public void OnContinueClicked()
    {
        PlayClickSfx();
        Debug.Log("Continue - Not implemented yet");
    }
    
    // === OPTIONS PANEL BUTTONS ===
    public void OnSettingsClicked()
    {
        PlayClickSfx();
        StartCoroutine(TransitionFromOptionsPanelToSettings());
    }
    
    public void OnCreditsClicked()
    {
        PlayClickSfx();
        Debug.Log("Credits - Not implemented yet");
    }
    
    // === RETURN BUTTONS ===
    public void OnReturnToMainMenu()
    {
        PlayClickSfx();
        StartCoroutine(ReturnToMainMenu());
    }

    public void OnReturnFromSettings()
    {
        PlayClickSfx();
        StartCoroutine(TransitionFromSettingsToOptions());
    }
    
    // === TRANSITION LOGIC ===
    System.Collections.IEnumerator TransitionToPanel(GameObject targetPanel, CanvasGroup targetGroup)
    {
        PlayTransitionSfx();

        // Fade out main panel and title
        yield return StartCoroutine(FadeOut(mainPanelGroup));
        yield return StartCoroutine(FadeOut(titleGroup));
        
        mainPanel.SetActive(false);
        gameTitle.gameObject.SetActive(false);
        
        // Fade in target panel
        targetPanel.SetActive(true);
        yield return StartCoroutine(FadeIn(targetGroup));
    }

    System.Collections.IEnumerator TransitionFromOptionsPanelToSettings()
    {
        PlayTransitionSfx();

        // Fade out options panel
        yield return StartCoroutine(FadeOut(optionsPanelGroup));
        optionsPanel.SetActive(false);
        
        // Fade in settings panel
        settingsPanel.SetActive(true);
        yield return StartCoroutine(FadeIn(settingsPanelGroup));
    }

    System.Collections.IEnumerator TransitionFromSettingsToOptions()
    {
        PlayTransitionSfx();

        // Fade out settings panel
        yield return StartCoroutine(FadeOut(settingsPanelGroup));
        settingsPanel.SetActive(false);
        
        // Fade in options panel
        optionsPanel.SetActive(true);
        yield return StartCoroutine(FadeIn(optionsPanelGroup));
    }

    System.Collections.IEnumerator ShowConfirmQuitPanel()
    {
        PlayTransitionSfx();

        // Fade out main panel and title
        yield return StartCoroutine(FadeOut(mainPanelGroup));
        yield return StartCoroutine(FadeOut(titleGroup));
        
        mainPanel.SetActive(false);
        gameTitle.gameObject.SetActive(false);
        
        // Fade in confirm quit panel
        confirmQuitPanel.SetActive(true);
        yield return StartCoroutine(FadeIn(confirmQuitPanelGroup));
    }

    System.Collections.IEnumerator HideConfirmQuitPanel()
    {
        PlayTransitionSfx();

        // Fade out confirm quit panel
        yield return StartCoroutine(FadeOut(confirmQuitPanelGroup));
        confirmQuitPanel.SetActive(false);
        
        // Fade in main panel and title
        mainPanel.SetActive(true);
        gameTitle.gameObject.SetActive(true);
        
        yield return StartCoroutine(FadeIn(mainPanelGroup));
        yield return StartCoroutine(FadeIn(titleGroup));
    }
    
    System.Collections.IEnumerator ReturnToMainMenu()
    {
        PlayTransitionSfx();

        // Fade out current panel
        if (playPanel.activeSelf)
        {
            yield return StartCoroutine(FadeOut(playPanelGroup));
            playPanel.SetActive(false);
        }
        
        if (optionsPanel.activeSelf)
        {
            yield return StartCoroutine(FadeOut(optionsPanelGroup));
            optionsPanel.SetActive(false);
        }

        if (settingsPanel.activeSelf)
        {
            yield return StartCoroutine(FadeOut(settingsPanelGroup));
            settingsPanel.SetActive(false);
        }

        if (confirmQuitPanel.activeSelf)
        {
            yield return StartCoroutine(FadeOut(confirmQuitPanelGroup));
            confirmQuitPanel.SetActive(false);
        }
        
        // Fade in main panel and title
        mainPanel.SetActive(true);
        gameTitle.gameObject.SetActive(true);
        
        yield return StartCoroutine(FadeIn(mainPanelGroup));
        yield return StartCoroutine(FadeIn(titleGroup));
    }
    
    void ShowMainMenu()
    {
        mainPanel.SetActive(true);
        playPanel.SetActive(false);
        optionsPanel.SetActive(false);
        settingsPanel.SetActive(false);
        confirmQuitPanel.SetActive(false);
        gameTitle.gameObject.SetActive(true);
        
        mainPanelGroup.alpha = 1;
        titleGroup.alpha = 1;
    }
    
    // === FADE EFFECTS ===
    System.Collections.IEnumerator FadeOut(CanvasGroup group)
    {
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration);
            yield return null;
        }
        group.alpha = 0;
    }
    
    System.Collections.IEnumerator FadeIn(CanvasGroup group)
    {
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(0, 1, elapsed / fadeDuration);
            yield return null;
        }
        group.alpha = 1;
    }
}
