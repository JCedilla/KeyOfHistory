using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KeyOfHistory.Manager
{
    public class GlowingBook : InteractableObject
    {
        [Header("Teleport Settings")]
        [SerializeField] private Transform TeleportDestination;
        [SerializeField] private GameObject PlayerObject;
        
        [Header("Fade Settings")]
        [SerializeField] private Image FadeOverlay_White;
        [SerializeField] private float FadeDuration = 2f;
        
        [Header("Audio")]
        [SerializeField] private AudioClip InteractSound;
        
        private bool _hasInteracted = false;
        private bool _isTeleporting = false;
        
        public override string GetPromptText()
        {
            return "[E] Examine Book";
        }
        
        public override void Interact()
        {
            if (_hasInteracted || _isTeleporting) return;
            
            _hasInteracted = true;
            _isTeleporting = true;
            StartCoroutine(TeleportSequence());
        }
        
        private IEnumerator TeleportSequence()
        {
            // Get references to player components
            KeyOfHistory.PlayerControl.PlayerController playerController = null;
            KeyOfHistory.Manager.InputManager inputManager = null;
            Rigidbody playerRigidbody = null;
            Animator playerAnimator = null;
            
            if (PlayerObject != null)
            {
                playerController = PlayerObject.GetComponent<KeyOfHistory.PlayerControl.PlayerController>();
                inputManager = PlayerObject.GetComponent<KeyOfHistory.Manager.InputManager>();
                playerRigidbody = PlayerObject.GetComponent<Rigidbody>();
                playerAnimator = PlayerObject.GetComponentInChildren<Animator>();
            }
            
            // STEP 1: Stop ALL player movement and audio
            if (playerController != null) 
            {
                playerController.enabled = false;
            }
            
            if (inputManager != null) 
            {
                inputManager.enabled = false;
            }
            
            // STEP 2: Stop footstep audio immediately
            AudioSource[] audioSources = PlayerObject.GetComponentsInChildren<AudioSource>();
            foreach (AudioSource source in audioSources)
            {
                if (source.isPlaying && source.loop)
                {
                    source.Stop();
                }
            }
            
            // STEP 3: Reset Rigidbody velocity to stop movement
            if (playerRigidbody != null)
            {
                playerRigidbody.linearVelocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
            }
            
            // STEP 4: Reset animator to idle
            if (playerAnimator != null)
            {
                playerAnimator.SetFloat("X_Velocity", 0f);
                playerAnimator.SetFloat("Y_Velocity", 0f);
            }
            
            // Wait a tiny bit for everything to settle
            yield return new WaitForFixedUpdate();
            
            // STEP 5: Play sound effect
            if (InteractSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(InteractSound);
            }
            
            // STEP 6: Fade to white
            yield return StartCoroutine(FadeToWhite());
            
            // STEP 7: Teleport player
            if (PlayerObject != null && TeleportDestination != null)
            {
                if (playerRigidbody != null)
                {
                    // Use Rigidbody.position for physics-based movement
                    playerRigidbody.position = TeleportDestination.position;
                    playerRigidbody.rotation = TeleportDestination.rotation;
                    
                    // Ensure velocity stays zero after teleport
                    playerRigidbody.linearVelocity = Vector3.zero;
                    playerRigidbody.angularVelocity = Vector3.zero;
                }
                else
                {
                    // Fallback to transform
                    PlayerObject.transform.position = TeleportDestination.position;
                    PlayerObject.transform.rotation = TeleportDestination.rotation;
                }
                
                // RESET CAMERA LOOK DIRECTION (using existing variable)
                if (playerController != null)
                {
                    playerController.ResetCameraRotation();
                }
                
                // RESET ANIMATOR COMPLETELY
                if (playerAnimator != null)
                {
                    playerAnimator.SetFloat("X_Velocity", 0f);
                    playerAnimator.SetFloat("Y_Velocity", 0f);
                    playerAnimator.SetBool("Grounded", true);
                    playerAnimator.SetBool("Falling", false);
                    playerAnimator.SetBool("Crouching", false);
                }
            }
            
            // Wait a moment at destination
            yield return new WaitForSeconds(0.5f);
            
            // STEP 8: Fade from white
            yield return StartCoroutine(FadeFromWhite());
            
            // STEP 9: Re-enable player controls
            if (playerController != null) 
            {
                playerController.enabled = true;
            }
            
            if (inputManager != null) 
            {
                inputManager.enabled = true;
            }
            
            _isTeleporting = false;
        }
        
        private IEnumerator FadeToWhite()
        {
            float elapsed = 0f;
            
            while (elapsed < FadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / FadeDuration);
                SetFadeAlpha(alpha);
                yield return null;
            }
            
            SetFadeAlpha(1f);
        }
        
        private IEnumerator FadeFromWhite()
        {
            float elapsed = 0f;
            
            while (elapsed < FadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / FadeDuration);
                SetFadeAlpha(alpha);
                yield return null;
            }
            
            SetFadeAlpha(0f);
        }
        
        private void SetFadeAlpha(float alpha)
        {
            if (FadeOverlay_White != null)
            {
                Color color = FadeOverlay_White.color;
                color.a = alpha;
                FadeOverlay_White.color = color;
            }
        }
    }
}