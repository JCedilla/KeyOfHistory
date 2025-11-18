using UnityEngine;
using TMPro;
using KeyOfHistory.Manager;

namespace KeyOfHistory.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }
        
        [Header("UI References")]
        [SerializeField] private GameObject DialoguePanel;
        [SerializeField] private TextMeshProUGUI NPCNameText;
        [SerializeField] private TextMeshProUGUI DialogueText;
        [SerializeField] private GameObject ContinuePrompt;
        
        [Header("Player Control")]
        [SerializeField] private GameObject PlayerObject;
        
        private DialogueData _currentDialogue;
        private int _currentLineIndex = 0;
        private bool _isInDialogue = false;
        private System.Action _onDialogueComplete;
        
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
        
        public void StartDialogue(DialogueData dialogue, System.Action onComplete = null)
        {
            if (_isInDialogue) return;
            
            _currentDialogue = dialogue;
            _currentLineIndex = 0;
            _isInDialogue = true;
            _onDialogueComplete = onComplete;
            
            DisablePlayerControls();
            ShowLine(_currentLineIndex);
        }
        
        private void ShowLine(int index)
        {
            if (index >= _currentDialogue.Lines.Length)
            {
                EndDialogue();
                return;
            }
            
            DialoguePanel.SetActive(true);
            NPCNameText.text = _currentDialogue.NPCName;
            DialogueText.text = _currentDialogue.Lines[index].Text;
            ContinuePrompt.SetActive(true);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopVoice(); // NEW
            }
            
            // Play voice if available
            if (_currentDialogue.Lines[index].VoiceClip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayVoice(_currentDialogue.Lines[index].VoiceClip);
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

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopVoice(); // NEW
            }
            
            
            EnablePlayerControls();
            
            // Trigger completion callback (for spawning keys, etc.)
            _onDialogueComplete?.Invoke();
            
            _currentDialogue = null;
            _onDialogueComplete = null;
        }
        
        public bool IsInDialogue()
        {
            return _isInDialogue;
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
            
            // Lock cursor during dialogue
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
}