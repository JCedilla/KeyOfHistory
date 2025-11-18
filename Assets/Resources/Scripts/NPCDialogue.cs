using UnityEngine;
using System.Collections;

namespace KeyOfHistory.Manager
{
    public class NPCDialogue : InteractableObject
    {
        [Header("NPC Settings")]
        [SerializeField] private string NPCName = "Mysterious Figure";
        
        [Header("Dialogue Lines")]
        [SerializeField] private DialogueLine[] DialogueLines;
        
        [Header("UI References")]
        [SerializeField] private GameObject DialoguePanel;
        [SerializeField] private TMPro.TextMeshProUGUI NPCNameText;
        [SerializeField] private TMPro.TextMeshProUGUI DialogueText;
        [SerializeField] private GameObject ContinuePrompt;
        
        [Header("Key Spawn")]
        [SerializeField] private GameObject KeyPrefab;
        [SerializeField] private Transform KeySpawnPoint;
        
        [Header("Player Control")]
        [SerializeField] private GameObject PlayerObject;
        
        [Header("Dialogue Settings")]
        [SerializeField] private bool CanRepeatDialogue = false; // NEW: Allow talking multiple times
        
        private int _currentLineIndex = 0;
        private bool _isInDialogue = false;
        private bool _hasCompletedDialogue = false;
        
        [System.Serializable]
        public class DialogueLine
        {
            [TextArea(2, 4)]
            public string Text;
            public AudioClip VoiceClip;
        }
        
        private void Start()
        {
            DialoguePanel.SetActive(false);
            
            // Auto-find player if not assigned
            if (PlayerObject == null)
            {
                PlayerObject = GameObject.FindGameObjectWithTag("Player");
            }
        }
        
        private void Update()
        {
            // Press Space or E to continue during dialogue
            if (_isInDialogue && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E)))
            {
                NextLine();
            }
        }
        
        public override string GetPromptText()
        {
            // Hide prompt if dialogue is ongoing
            if (_isInDialogue)
                return "";
            
            // If can repeat dialogue, always show prompt
            if (CanRepeatDialogue)
                return "[E] Talk to " + NPCName;
            
            // Otherwise, hide prompt after completion
            if (_hasCompletedDialogue)
                return "";
            else
                return "[E] Talk to " + NPCName;
        }
        
        public override void Interact()
        {
            // Don't interact if already in dialogue
            if (_isInDialogue)
                return;
            
            // If can't repeat and already completed, don't start again
            if (!CanRepeatDialogue && _hasCompletedDialogue)
                return;
            
            StartDialogue();
        }
        
        private void StartDialogue()
        {
            _isInDialogue = true;
            _currentLineIndex = 0;
            
            // Disable player controls
            DisablePlayerControls();
            
            // Show first line
            ShowLine(_currentLineIndex);
        }
        
        private void ShowLine(int index)
        {
            if (index >= DialogueLines.Length)
            {
                EndDialogue();
                return;
            }
            
            DialoguePanel.SetActive(true);
            NPCNameText.text = NPCName;
            DialogueText.text = DialogueLines[index].Text;
            ContinuePrompt.SetActive(true);
            
            // Stop previous voice before playing new one (FIXED)
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopVoice();
            }
            
            // Play voice if available
            if (DialogueLines[index].VoiceClip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayVoice(DialogueLines[index].VoiceClip);
            }
        }
        
        private void NextLine()
        {
            _currentLineIndex++;
            ShowLine(_currentLineIndex);
        }
        
        private void EndDialogue()
        {
            _isInDialogue = false;
            DialoguePanel.SetActive(false);
            
            // Stop voice when dialogue ends (FIXED)
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopVoice();
            }
            
            // Re-enable player controls
            EnablePlayerControls();
            
            // Only spawn key on first completion (not if repeating)
            if (!_hasCompletedDialogue)
            {
                _hasCompletedDialogue = true;
                SpawnKey();
            }
        }
        
        private void SpawnKey()
        {
            if (KeyPrefab != null && KeySpawnPoint != null)
            {
                GameObject key = Instantiate(KeyPrefab, KeySpawnPoint.position, KeySpawnPoint.rotation);
                
                // Add floating animation to key
                FloatingObject floatScript = key.GetComponent<FloatingObject>();
                if (floatScript == null)
                {
                    floatScript = key.AddComponent<FloatingObject>();
                }
                
                Debug.Log("Key spawned!");
            }
        }
        
        private void DisablePlayerControls()
        {
            if (PlayerObject != null)
            {
                var playerController = PlayerObject.GetComponent<KeyOfHistory.PlayerControl.PlayerController>();
                var inputManager = PlayerObject.GetComponent<KeyOfHistory.Manager.InputManager>();

                if (playerController != null) playerController.enabled = false;
                if (inputManager != null) inputManager.enabled = false;
            }
            
            InteractionSystem interactionSystem = FindFirstObjectByType<InteractionSystem>();
            if (interactionSystem != null)
            {
                interactionSystem.enabled = false;
            }
            
            // Show cursor during dialogue
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        
        private void EnablePlayerControls()
        {
            if (PlayerObject != null)
            {
                var playerController = PlayerObject.GetComponent<KeyOfHistory.PlayerControl.PlayerController>();
                var inputManager = PlayerObject.GetComponent<KeyOfHistory.Manager.InputManager>();

                if (playerController != null) playerController.enabled = true;
                if (inputManager != null) inputManager.enabled = true;
            }
            
            InteractionSystem interactionSystem = FindFirstObjectByType<InteractionSystem>();
            if (interactionSystem != null)
            {
                interactionSystem.enabled = true;
            }
            
            // Hide cursor after dialogue
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    
    // Simple floating animation for spawned objects
    public class FloatingObject : MonoBehaviour
    {
        [SerializeField] private float BobSpeed = 1f;
        [SerializeField] private float BobHeight = 0.5f;
        [SerializeField] private float RotationSpeed = 50f;
        
        private Vector3 _startPos;
        
        void Start()
        {
            _startPos = transform.position;
        }
        
        void Update()
        {
            // Bob up and down
            float newY = _startPos.y + Mathf.Sin(Time.time * BobSpeed) * BobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            
            // Rotate
            transform.Rotate(Vector3.up, RotationSpeed * Time.deltaTime);
        }
    }
}