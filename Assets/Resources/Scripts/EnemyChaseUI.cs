using UnityEngine;
using TMPro;

namespace KeyOfHistory.UI
{
    public class EnemyChaseUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject ChaseWarningUI;
        [SerializeField] private GameObject LightDamageUI;
        [SerializeField] private TextMeshProUGUI CountdownText;
        
        [Header("Animation")]
        [SerializeField] private float PulseSpeed = 2f;
        
        private bool _isChasing = false;
        private bool _isDamagingEnemy = false;
        private float _currentExposure = 0f;
        private float _maxExposure = 5f;
        private CanvasGroup _chaseWarningCanvasGroup;
        
        // Singleton
        public static EnemyChaseUI Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // Hide UI initially
            if (ChaseWarningUI != null)
            {
                ChaseWarningUI.SetActive(false);
                
                // Add CanvasGroup for fade effect
                _chaseWarningCanvasGroup = ChaseWarningUI.GetComponent<CanvasGroup>();
                if (_chaseWarningCanvasGroup == null)
                {
                    _chaseWarningCanvasGroup = ChaseWarningUI.AddComponent<CanvasGroup>();
                }
            }
            
            if (LightDamageUI != null)
            {
                LightDamageUI.SetActive(false);
            }
        }
        
        private void Update()
        {
            // Pulse eye icon when chasing
            if (_isChasing && _chaseWarningCanvasGroup != null)
            {
                float pulse = Mathf.PingPong(Time.time * PulseSpeed, 1f);
                _chaseWarningCanvasGroup.alpha = Mathf.Lerp(0.5f, 1f, pulse);
            }
        }
        
        public void ShowChaseWarning(bool show)
        {
            _isChasing = show;
            
            if (ChaseWarningUI != null)
            {
                ChaseWarningUI.SetActive(show);
            }
        }
        
        public void ShowLightDamage(bool show)
        {
            _isDamagingEnemy = show;
            
            if (LightDamageUI != null)
            {
                LightDamageUI.SetActive(show);
            }
        }
        
        public void UpdateLightDamage(float currentExposure, float maxExposure)
        {
            _currentExposure = currentExposure;
            _maxExposure = maxExposure;
            
            if (CountdownText != null)
            {
                CountdownText.text = $"{currentExposure:F1} / {maxExposure:F1}";
                
                // Change color based on progress
                float percent = currentExposure / maxExposure;
                if (percent < 0.5f)
                {
                    CountdownText.color = Color.yellow;
                }
                else if (percent < 0.8f)
                {
                    CountdownText.color = Color.Lerp(Color.yellow, Color.red, (percent - 0.5f) / 0.3f);
                }
                else
                {
                    CountdownText.color = Color.red;
                }
            }
        }
    }
}