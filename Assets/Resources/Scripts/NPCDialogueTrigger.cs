using UnityEngine;
using KeyOfHistory.Manager;

namespace KeyOfHistory.Dialogue
{
    public class NPCDialogueTrigger : InteractableObject
    {
        [Header("Dialogue Settings")]
        [SerializeField] private DialogueData DialogueData;
        
        [Header("One-Time Dialogue")]
        [SerializeField] private bool CanOnlyTalkOnce = false;
        [SerializeField] private bool HasTalked = false;
        
        [Header("Post-Dialogue Events")]
        [SerializeField] private bool SpawnObjectAfterDialogue = false;
        [SerializeField] private GameObject ObjectToSpawn;
        [SerializeField] private Transform SpawnPoint;
        
        public override string GetPromptText()
        {
            // If DialogueManager is in dialogue, hide prompt
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue())
                return "";
            
            // If already talked and can only talk once, hide prompt
            if (CanOnlyTalkOnce && HasTalked)
                return "";
            
            return $"[E] Talk to {DialogueData.NPCName}";
        }
        
        public override void Interact()
        {
            // Don't interact if already in dialogue
            if (DialogueManager.Instance == null || DialogueManager.Instance.IsInDialogue())
                return;
            
            // Don't interact if already talked (one-time only)
            if (CanOnlyTalkOnce && HasTalked)
                return;
            
            // Start dialogue with optional completion callback
            DialogueManager.Instance.StartDialogue(DialogueData, OnDialogueComplete);
            
            // Mark as talked
            if (CanOnlyTalkOnce)
                HasTalked = true;
        }
        
        private void OnDialogueComplete()
        {
            // Spawn object if configured
            if (SpawnObjectAfterDialogue && ObjectToSpawn != null && SpawnPoint != null)
            {
                GameObject spawnedObject = Instantiate(ObjectToSpawn, SpawnPoint.position, SpawnPoint.rotation);
                
                // Add floating animation if it's a pickup
                FloatingObject floatScript = spawnedObject.GetComponent<FloatingObject>();
                if (floatScript == null)
                {
                    floatScript = spawnedObject.AddComponent<FloatingObject>();
                }
                
                Debug.Log($"Spawned {ObjectToSpawn.name} after dialogue with {DialogueData.NPCName}");
            }
        }
    }
    
    // Keep your floating animation
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
