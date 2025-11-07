using UnityEngine;

namespace KeyOfHistory.Manager
{
    public class InteractableObject : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private string PromptText = "[E] Interact";
        
        public virtual string GetPromptText()
        {
            return PromptText;
        }
        
        public virtual void Interact()
        {
            // Override this in child classes
            Debug.Log("Interacted with: " + gameObject.name);
        }
    }
}