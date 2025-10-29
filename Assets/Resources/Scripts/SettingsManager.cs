using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

namespace KeyOfHistory.Manager
{
    public class SettingsManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AudioMixer MainAudioMixer;
        [SerializeField] private Volume PostProcessingVolume;
        
        [Header("Default Values")]
        [SerializeField] private float DefaultMouseSensitivity = 21.9f;
        [SerializeField] private float DefaultVolume = 0.75f;
        [SerializeField] private float DefaultBrightness = 0f;

        // Singleton
        public static SettingsManager Instance { get; private set; }

        // Current settings values
        private float _mouseSensitivity;
        private float _masterVolume;
        private float _brightness;
        private bool _isFullscreen;

        // PlayerPrefs keys
        private const string MOUSE_SENSITIVITY_KEY = "MouseSensitivity";
        private const string MASTER_VOLUME_KEY = "MasterVolume";
        private const string BRIGHTNESS_KEY = "Brightness";
        private const string FULLSCREEN_KEY = "Fullscreen";

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadSettings();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            ApplyAllSettings();
        }

        // Load settings from PlayerPrefs
        private void LoadSettings()
        {
            _mouseSensitivity = PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_KEY, DefaultMouseSensitivity);
            _masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, DefaultVolume);
            _brightness = PlayerPrefs.GetFloat(BRIGHTNESS_KEY, DefaultBrightness);
            _isFullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1; // 1 = true, 0 = false
        }

        // Save settings to PlayerPrefs
        private void SaveSettings()
        {
            PlayerPrefs.SetFloat(MOUSE_SENSITIVITY_KEY, _mouseSensitivity);
            PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, _masterVolume);
            PlayerPrefs.SetFloat(BRIGHTNESS_KEY, _brightness);
            PlayerPrefs.SetInt(FULLSCREEN_KEY, _isFullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }

        // Apply all settings
        private void ApplyAllSettings()
        {
            ApplyMouseSensitivity(_mouseSensitivity);
            ApplyVolume(_masterVolume);
            ApplyBrightness(_brightness);
            ApplyFullscreen(_isFullscreen);
        }

        // === MOUSE SENSITIVITY ===
        public void SetMouseSensitivity(float value)
        {
            _mouseSensitivity = value;
            ApplyMouseSensitivity(value);
            SaveSettings();
        }

        private void ApplyMouseSensitivity(float value)
        {
            // This will be accessed by PlayerController
            _mouseSensitivity = value;
        }

        public float GetMouseSensitivity()
        {
            return _mouseSensitivity;
        }

        // === MASTER VOLUME ===
        public void SetMasterVolume(float value)
        {
            _masterVolume = value;
            ApplyVolume(value);
            SaveSettings();
        }

        private void ApplyVolume(float value)
        {
            if (MainAudioMixer != null)
            {
                // Convert 0-1 range to decibels (-80 to 0)
                float dB = value > 0 ? Mathf.Log10(value) * 20 : -80f;
                MainAudioMixer.SetFloat("MasterVolume", dB);
            }
        }

        public float GetMasterVolume()
        {
            return _masterVolume;
        }

        // === BRIGHTNESS ===
        public void SetBrightness(float value)
        {
            _brightness = value;
            ApplyBrightness(value);
            SaveSettings();
        }

        private void ApplyBrightness(float value)
        {
            if (PostProcessingVolume != null && PostProcessingVolume.profile != null)
            {
                // Try to get Color Adjustments (URP) or Color Grading (HDRP/Built-in)
                if (PostProcessingVolume.profile.TryGet(out UnityEngine.Rendering.Universal.ColorAdjustments colorAdjustments))
                {
                    colorAdjustments.postExposure.value = value;
                }
            }
        }

        public float GetBrightness()
        {
            return _brightness;
        }

        // === FULLSCREEN ===
        public void SetFullscreen(bool isFullscreen)
        {
            _isFullscreen = isFullscreen;
            ApplyFullscreen(isFullscreen);
            SaveSettings();
        }

        private void ApplyFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }

        public bool GetFullscreen()
        {
            return _isFullscreen;
        }

        // === RESET TO DEFAULTS ===
        public void ResetToDefaults()
        {
            SetMouseSensitivity(DefaultMouseSensitivity);
            SetMasterVolume(DefaultVolume);
            SetBrightness(DefaultBrightness);
            SetFullscreen(true);
        }
    }
}