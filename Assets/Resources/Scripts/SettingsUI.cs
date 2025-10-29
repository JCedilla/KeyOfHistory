using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KeyOfHistory.UI
{
    public class SettingsUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider MouseSensitivitySlider;
        [SerializeField] private TextMeshProUGUI MouseSensitivityText;

        [SerializeField] private Slider VolumeSlider;
        [SerializeField] private TextMeshProUGUI VolumeText;

        [SerializeField] private Slider BrightnessSlider;
        [SerializeField] private TextMeshProUGUI BrightnessText;

        [SerializeField] private Toggle FullscreenToggle;

        [SerializeField] private Button ResetButton;


        private void OnEnable()
        {
            LoadCurrentSettings();

            // Add listeners
            MouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
            VolumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            BrightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
            FullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

            if (ResetButton != null)
            {
                ResetButton.onClick.AddListener(OnResetClicked);
            }
        }

        private void OnDisable()
        {
            // Remove listeners
            MouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSensitivityChanged);
            VolumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
            BrightnessSlider.onValueChanged.RemoveListener(OnBrightnessChanged);
            FullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);

            if (ResetButton != null)
            {
                ResetButton.onClick.RemoveListener(OnResetClicked);
            }
        }

        private void LoadCurrentSettings()
        {
            if (Manager.SettingsManager.Instance == null) return;

            // Load mouse sensitivity
            float mouseSens = Manager.SettingsManager.Instance.GetMouseSensitivity();
            MouseSensitivitySlider.value = mouseSens;
            UpdateMouseSensitivityText(mouseSens);

            // Load volume
            float volume = Manager.SettingsManager.Instance.GetMasterVolume();
            VolumeSlider.value = volume;
            UpdateVolumeText(volume);

            // Load brightness
            float brightness = Manager.SettingsManager.Instance.GetBrightness();
            BrightnessSlider.value = brightness;
            UpdateBrightnessText(brightness);

            // Load fullscreen
            bool fullscreen = Manager.SettingsManager.Instance.GetFullscreen();
            FullscreenToggle.isOn = fullscreen;
        }

        private void OnMouseSensitivityChanged(float value)
        {
            Manager.SettingsManager.Instance.SetMouseSensitivity(value);
            UpdateMouseSensitivityText(value);
        }

        private void UpdateMouseSensitivityText(float value)
        {
            if (MouseSensitivityText != null)
            {
                MouseSensitivityText.text = value.ToString("F1");
            }
        }

        private void OnVolumeChanged(float value)
        {
            Manager.SettingsManager.Instance.SetMasterVolume(value);
            UpdateVolumeText(value);
        }

        private void UpdateVolumeText(float value)
        {
            if (VolumeText != null)
            {
                VolumeText.text = Mathf.RoundToInt(value * 100) + "%";
            }
        }

        private void OnBrightnessChanged(float value)
        {
            Manager.SettingsManager.Instance.SetBrightness(value);
            UpdateBrightnessText(value);
        }

        private void UpdateBrightnessText(float value)
        {
            if (BrightnessText != null)
            {
                BrightnessText.text = value.ToString("F2");
            }
        }

        private void OnFullscreenChanged(bool isFullscreen)
        {
            Manager.SettingsManager.Instance.SetFullscreen(isFullscreen);
        }

        private void OnResetClicked()
        {
            Manager.SettingsManager.Instance.ResetToDefaults();
            LoadCurrentSettings();
        }
    }
}