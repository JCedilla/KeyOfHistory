using UnityEngine;
using TMPro;

namespace KeyOfHistory.Manager
{
    public class TutorialManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject TutorialPanel;
        [SerializeField] private TextMeshProUGUI TutorialText_WASD;
        [SerializeField] private TextMeshProUGUI TutorialText_Shift;
        [SerializeField] private TextMeshProUGUI TutorialText_Space;
        [SerializeField] private TextMeshProUGUI TutorialText_Ctrl;
        
        [Header("Settings")]
        [SerializeField] private Color CompletedColor = Color.green;
        [SerializeField] private Color IncompleteColor = Color.white;
        
        private bool _hasMovedWASD = false;
        private bool _hasRun = false;
        private bool _hasJumped = false;
        private bool _hasCrouched = false;
        private bool _tutorialActive = false;
        
        private void Start()
        {
            TutorialPanel.SetActive(false);
        }
        
        public void StartTutorial()
        {
            _tutorialActive = true;
            TutorialPanel.SetActive(true);
            
            // Set all to incomplete color
            TutorialText_WASD.color = IncompleteColor;
            TutorialText_Shift.color = IncompleteColor;
            TutorialText_Space.color = IncompleteColor;
            TutorialText_Ctrl.color = IncompleteColor;
        }
        
        private void Update()
        {
            if (!_tutorialActive) return;
            
            // Check WASD movement
            if (!_hasMovedWASD)
            {
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || 
                    Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
                {
                    _hasMovedWASD = true;
                    TutorialText_WASD.color = CompletedColor;
                }
            }
            
            // Check Shift (run)
            if (!_hasRun && Input.GetKey(KeyCode.LeftShift))
            {
                _hasRun = true;
                TutorialText_Shift.color = CompletedColor;
            }
            
            // Check Space (jump)
            if (!_hasJumped && Input.GetKeyDown(KeyCode.Space))
            {
                _hasJumped = true;
                TutorialText_Space.color = CompletedColor;
            }
            
            // Check Ctrl (crouch)
            if (!_hasCrouched && Input.GetKey(KeyCode.LeftControl))
            {
                _hasCrouched = true;
                TutorialText_Ctrl.color = CompletedColor;
            }
            
            // Check if all completed
            if (_hasMovedWASD && _hasRun && _hasJumped && _hasCrouched)
            {
                CompleteTutorial();
            }
        }
        
        private void CompleteTutorial()
        {
            _tutorialActive = false;
            // Optional: Fade out tutorial panel instead of instant hide
            Invoke(nameof(HideTutorialPanel), 2f);
        }
        
        private void HideTutorialPanel()
        {
            TutorialPanel.SetActive(false);
        }
        
        // Public method to manually end tutorial
        public void EndTutorial()
        {
            _tutorialActive = false;
            TutorialPanel.SetActive(false);
        }
    }
}