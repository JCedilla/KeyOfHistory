using UnityEngine;
using KeyOfHistory.UI;

namespace KeyOfHistory.Manager
{
    public class HallwayTrigger : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioSource VoiceSource;
        [SerializeField] private AudioClip HallwayVoiceLine; // "Huh, what's that glowing thing?"
        
        [Header("UI References")]
        [SerializeField] private GameObject CaptionPanel;
        [SerializeField] private TMPro.TextMeshProUGUI CaptionText;
        
        [Header("Objective")]
        [SerializeField] private ObjectiveManager ObjectiveManager;
        
        [Header("Settings")]
        [SerializeField] private float CaptionDisplayTime = 3f;
        
        private bool _hasTriggered = false;
        
        private void OnTriggerEnter(Collider other)
        {
            // Check if player entered and hasn't triggered before
            if (_hasTriggered) return;
            
            if (other.CompareTag("Player"))
            {
                _hasTriggered = true;
                TriggerHallwayEvent();
            }
        }
        
        private void TriggerHallwayEvent()
        {
            // Play voice line
            if (HallwayVoiceLine != null && VoiceSource != null)
            {
                VoiceSource.clip = HallwayVoiceLine;
                VoiceSource.Play();
            }
            
            // Show caption
            if (CaptionPanel != null && CaptionText != null)
            {
                CaptionText.text = "Huh, what's that glowing thing?";
                CaptionPanel.SetActive(true);
                
                // Hide caption after delay
                Invoke(nameof(HideCaption), CaptionDisplayTime);
            }
            
            // Update objective
            if (ObjectiveManager != null)
            {
                ObjectiveManager.UpdateObjective("Investigate the glowing object");
            }
        }
        
        private void HideCaption()
        {
            if (CaptionPanel != null)
            {
                CaptionPanel.SetActive(false);
            }
        }
    }
}