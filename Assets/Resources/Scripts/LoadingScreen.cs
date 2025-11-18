using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace KeyOfHistory.Manager
{
    public class LoadingScreen : MonoBehaviour
    {
        public static LoadingScreen Instance { get; private set; }
        
        [Header("UI References")]
        [SerializeField] private GameObject LoadingPanel;
        [SerializeField] private Image LoadingBarFill;
        [SerializeField] private TextMeshProUGUI LoadingText;
        [SerializeField] private TextMeshProUGUI TipText;
        
        [Header("Settings")]
        [SerializeField] private float MinimumLoadTime = 5f;
        [SerializeField] private string[] LoadingTips;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadingPanel.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }
        
        private IEnumerator LoadSceneAsync(string sceneName)
        {
            // Show loading screen
            LoadingPanel.SetActive(true);
            
            // Show random tip
            if (LoadingTips.Length > 0 && TipText != null)
            {
                TipText.text = LoadingTips[Random.Range(0, LoadingTips.Length)];
            }
            
            // Start async load
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;
            
            float elapsedTime = 0f;
            
            // Wait for loading to complete (or minimum time)
            while (!operation.isDone)
            {
                elapsedTime += Time.deltaTime;
                
                // Calculate progress (0.9 = 90% loaded, Unity holds at 90% until activation)
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                
                // Update UI
                if (LoadingBarFill != null)
                {
                    LoadingBarFill.fillAmount = progress;
                }
                
                if (LoadingText != null)
                {
                    LoadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
                }
                
                // Once loaded and minimum time passed, activate scene
                if (operation.progress >= 0.9f && elapsedTime >= MinimumLoadTime)
                {
                    LoadingText.text = "Press any key to continue";
                    
                    // Wait for player input
                    yield return new WaitUntil(() => Input.anyKeyDown);
                    
                    operation.allowSceneActivation = true;
                }
                
                yield return null;
            }
            
            // Hide loading screen
            LoadingPanel.SetActive(false);
        }
    }
}