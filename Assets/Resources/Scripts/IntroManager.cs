using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KeyOfHistory.Manager
{
    public class IntroManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image FadeOverlay_Black;
        [SerializeField] private GameObject CaptionPanel;
        [SerializeField] private TextMeshProUGUI CaptionText;
        
        [Header("Audio")]
        [SerializeField] private AudioSource VoiceSource;
        [SerializeField] private AudioClip IntroVoiceLine;
        
        [Header("Settings")]
        [SerializeField] private float FadeDuration = 2f;
        [SerializeField] private float CaptionDisplayTime = 3f;
        
        [Header("Player Control")]
        [SerializeField] private GameObject PlayerObject;
        
        private void Start()
        {
            StartCoroutine(PlayIntroSequence());
        }
        
        private IEnumerator PlayIntroSequence()
        {
            // Disable player controls during intro
            if (PlayerObject != null)
            {
                var playerController = PlayerObject.GetComponent<KeyOfHistory.PlayerControl.PlayerController>();
                var inputManager = PlayerObject.GetComponent<KeyOfHistory.Manager.InputManager>();
                
                if (playerController != null) playerController.enabled = false;
                if (inputManager != null) inputManager.enabled = false;
            }
            
            // Ensure black screen is fully opaque
            SetFadeAlpha(FadeOverlay_Black, 1f);
            CaptionPanel.SetActive(false);
            
            // Wait a moment
            yield return new WaitForSeconds(0.5f);
            
            // Fade from black
            yield return StartCoroutine(FadeFromBlack());
            
            // Play voice line and show caption
            PlayVoiceLine(IntroVoiceLine, "Hmm.. Which book was I supposed to find again?");
            
            // Wait for caption to display
            yield return new WaitForSeconds(CaptionDisplayTime);
            
            // Hide caption
            CaptionPanel.SetActive(false);
            
            // Re-enable player controls
            if (PlayerObject != null)
            {
                var playerController = PlayerObject.GetComponent<KeyOfHistory.PlayerControl.PlayerController>();
                var inputManager = PlayerObject.GetComponent<KeyOfHistory.Manager.InputManager>();
                
                if (playerController != null) playerController.enabled = true;
                if (inputManager != null) inputManager.enabled = true;
            }
            
            // Notify tutorial manager to start
            TutorialManager tutorialManager = FindFirstObjectByType<TutorialManager>();
            if (tutorialManager != null)
            {
                tutorialManager.StartTutorial();
            }
        }
        
        private IEnumerator FadeFromBlack()
        {
            float elapsed = 0f;
            
            while (elapsed < FadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / FadeDuration);
                SetFadeAlpha(FadeOverlay_Black, alpha);
                yield return null;
            }
            
            SetFadeAlpha(FadeOverlay_Black, 0f);
        }
        
        private void SetFadeAlpha(Image image, float alpha)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
        
        public void PlayVoiceLine(AudioClip clip, string captionText)
        {
            if (clip != null && VoiceSource != null)
            {
                VoiceSource.clip = clip;
                VoiceSource.Play();
            }
            
            ShowCaption(captionText);
        }
        
        public void ShowCaption(string text)
        {
            CaptionText.text = text;
            CaptionPanel.SetActive(true);
        }
        
        public void HideCaption()
        {
            CaptionPanel.SetActive(false);
        }
    }
}