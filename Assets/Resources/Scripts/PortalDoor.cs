using System.Collections;
using UnityEngine;

namespace KeyOfHistory.Manager
{
    public class PortalDoor : InteractableObject
    {
        [Header("Scene Loading")]
        [SerializeField] private string NextSceneName = "PreColonialScene";
        
        [Header("Portal Effects")]
        [SerializeField] private ParticleSystem PortalVortex;
        [SerializeField] private AudioClip PortalEnterSound;
        
        [Header("Player")]
        [SerializeField] private GameObject PlayerObject;
        
        private bool _isTransitioning = false;
        
        public override string GetPromptText()
        {
            return "[E] Enter Portal";
        }
        
        public override void Interact()
        {
            if (_isTransitioning) return;
            
            _isTransitioning = true;
            StartCoroutine(EnterPortalSequence());
        }
        
        private IEnumerator EnterPortalSequence()
        {
            // Disable player controls
            DisablePlayerControls();
            
            // Play portal sound
            if (PortalEnterSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(PortalEnterSound);
            }
            
            // Intensify portal effects
            if (PortalVortex != null)
            {
                var emission = PortalVortex.emission;
                emission.rateOverTime = 100; // Increase particles
            }
            
            // Small delay for effect
            yield return new WaitForSeconds(0.5f);
            
            // Load next scene with loading screen
            LoadingScreen.Instance.LoadScene(NextSceneName);
        }
        
        private void DisablePlayerControls()
        {
            if (PlayerObject == null)
            {
                PlayerObject = GameObject.FindGameObjectWithTag("Player");
            }
            
            if (PlayerObject != null)
            {
                var playerController = PlayerObject.GetComponent<KeyOfHistory.PlayerControl.PlayerController>();
                var inputManager = PlayerObject.GetComponent<KeyOfHistory.Manager.InputManager>();
                
                if (playerController != null) playerController.enabled = false;
                if (inputManager != null) inputManager.enabled = false;
            }
        }
    }
}