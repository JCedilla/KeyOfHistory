using UnityEngine;
using System.Collections;

namespace KeyOfHistory.Manager
{
    public class NPCDialogue : MonoBehaviour
    {
        [Header("NPC Settings")]
        [SerializeField] private string NPCName = "Mysterious Figure";
        [SerializeField] private float AutoStartDelay = 2.5f;
        
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
        
        private int _currentLineIndex = 0;
        private bool _isInDialogue = false;
        
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
            StartCoroutine(AutoStartDialogue());
        }
        
        private void Update()
        {
            // Press Space or E to continue
            if (_isInDialogue && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E)))
            {
                NextLine();
            }
        }
        
        private IEnumerator AutoStartDialogue()
        {
            yield return new WaitForSeconds(AutoStartDelay);
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
            
            // Re-enable player controls
            EnablePlayerControls();
            
            // Spawn key
            SpawnKey();
        }
        
        private void SpawnKey()
        {
            if (KeyPrefab != null && KeySpawnPoint != null)
            {
                Instantiate(KeyPrefab, KeySpawnPoint.position, KeySpawnPoint.rotation);
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
        }
    }
}