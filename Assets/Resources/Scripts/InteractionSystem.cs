using UnityEngine;
using TMPro;

namespace KeyOfHistory.Manager
{
    public class InteractionSystem : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private Camera PlayerCamera;
        [SerializeField] private float InteractionDistance = 3f;
        [SerializeField] private LayerMask InteractableLayer;
        
        [Header("UI References")]
        [SerializeField] private GameObject InteractionPrompt;
        [SerializeField] private TextMeshProUGUI PromptText;
        
        [Header("Input")]
        [SerializeField] private KeyCode InteractKey = KeyCode.E;
        
        private InteractableObject _currentInteractable;
        
        private void Start()
        {
            if (PlayerCamera == null)
            {
                PlayerCamera = Camera.main;
            }
            
            InteractionPrompt.SetActive(false);
        }
        
        private void Update()
        {
            CheckForInteractable();
            HandleInteraction();
        }
        
        private void CheckForInteractable()
        {
            Ray ray = PlayerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, InteractionDistance, InteractableLayer))
            {
                InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
                
                if (interactable != null)
                {
                    // Found interactable object
                    _currentInteractable = interactable;
                    ShowPrompt(interactable.GetPromptText());
                    return;
                }
            }
            
            // No interactable found
            if (_currentInteractable != null)
            {
                _currentInteractable = null;
                HidePrompt();
            }
        }
        
        private void HandleInteraction()
        {
            if (_currentInteractable != null && Input.GetKeyDown(InteractKey))
            {
                _currentInteractable.Interact();
                HidePrompt();
            }
        }
        
        private void ShowPrompt(string text)
        {
            if (PromptText != null)
            {
                PromptText.text = text;
                InteractionPrompt.SetActive(true);
            }
        }
        
        private void HidePrompt()
        {
            InteractionPrompt.SetActive(false);
        }
        
        // Debug visualization
        private void OnDrawGizmos()
        {
            if (PlayerCamera != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(PlayerCamera.transform.position, PlayerCamera.transform.forward * InteractionDistance);
            }
        }
    }
}