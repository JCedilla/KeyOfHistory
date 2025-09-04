using UnityEngine;
using UnityEngine.UI;

namespace KeyOfHistory.UI
{
    public class StaminaUIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider StaminaSlider;
        [SerializeField] private GameObject StaminaPanel;
        
        [Header("Settings")]
        [SerializeField] private bool HideWhenFull = true;
        [SerializeField] private float HideDelay = 2f; // seconds after full before hiding
        
        private float _hideTimer;
        private bool _isHidden = false;
        
        private void Start()
        {
            if (StaminaSlider == null)
            {
                Debug.LogError("StaminaSlider not assigned!");
                return;
            }
            
            // Initialize UI
            StaminaSlider.minValue = 0f;
            StaminaSlider.maxValue = 100f;
            StaminaSlider.value = 100f;
            
            if (HideWhenFull)
            {
                HideStaminaBar();
            }
        }
        
        public void UpdateStaminaUI(float currentStamina, float maxStamina)
        {
            if (StaminaSlider == null) return;
            
            // Update slider value (0-100 percentage)
            float percentage = (currentStamina / maxStamina) * 100f;
            StaminaSlider.value = percentage;
            
            // Handle visibility
            if (HideWhenFull)
            {
                if (percentage >= 100f)
                {
                    // Start hide timer when stamina is full
                    _hideTimer += Time.deltaTime;
                    if (_hideTimer >= HideDelay && !_isHidden)
                    {
                        HideStaminaBar();
                    }
                }
                else
                {
                    // Show bar when stamina is not full
                    _hideTimer = 0f;
                    if (_isHidden)
                    {
                        ShowStaminaBar();
                    }
                }
            }
        }
        
        private void ShowStaminaBar()
        {
            if (StaminaPanel != null)
                StaminaPanel.SetActive(true);
            _isHidden = false;
        }
        
        private void HideStaminaBar()
        {
            if (StaminaPanel != null)
                StaminaPanel.SetActive(false);
            _isHidden = true;
        }
        
        // Public methods for external control
        public void ForceShow()
        {
            ShowStaminaBar();
            _hideTimer = 0f;
        }
        
        public void ForceHide()
        {
            HideStaminaBar();
        }
    }
}