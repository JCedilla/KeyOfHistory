using UnityEngine;
using KeyOfHistory.Manager;
using KeyOfHistory.Enemy;

namespace KeyOfHistory.PlayerControl
{
    public class LightPowerController : MonoBehaviour
    {
        [Header("Light Settings")]
        [SerializeField] private Light SpiritualLight;
        [SerializeField] private ParticleSystem LightParticles;
        [SerializeField] private float LightRange = 15f;
        [SerializeField] private float LightAngle = 45f;
        
        [Header("Audio")]
        [SerializeField] private AudioSource LightAudioSource;
        [SerializeField] private AudioClip LightActivateSound;
        [SerializeField] private AudioClip LightLoopSound;
        [SerializeField] private AudioClip LightDeactivateSound;
        
        [Header("References")]
        [SerializeField] private Transform LightOrigin; // Chest position
        [SerializeField] private LayerMask EnemyLayer;
        
        private InputManager _inputManager;
        private bool _isLightActive = false;
        
        private void Start()
        {
            _inputManager = GetComponent<InputManager>();
            
            // Make sure light starts disabled
            if (SpiritualLight != null)
                SpiritualLight.enabled = false;
            
            if (LightParticles != null)
                LightParticles.Stop();
        }
        
        private void Update()
        {
            HandleLightPowerInput();
            
            if (_isLightActive)
            {
                DetectEnemiesInLight();
            }
        }
        
        private void HandleLightPowerInput()
        {
            bool wantsLightActive = _inputManager.LightPower;
            
            if (wantsLightActive && !_isLightActive)
            {
                ActivateLight();
            }
            else if (!wantsLightActive && _isLightActive)
            {
                DeactivateLight();
            }
        }
        
        private void ActivateLight()
        {
            _isLightActive = true;
            
            // Enable light
            if (SpiritualLight != null)
                SpiritualLight.enabled = true;
            
            // Start particles
            if (LightParticles != null)
                LightParticles.Play();
            
            // Play activation sound
            if (LightAudioSource != null && LightActivateSound != null)
            {
                LightAudioSource.PlayOneShot(LightActivateSound);
                
                // Start looping sound
                if (LightLoopSound != null)
                {
                    LightAudioSource.clip = LightLoopSound;
                    LightAudioSource.loop = true;
                    LightAudioSource.Play();
                }
            }
            
            Debug.Log("Light Power ACTIVATED");
        }
        
        private void DeactivateLight()
        {
            _isLightActive = false;
            
            // Disable light
            if (SpiritualLight != null)
                SpiritualLight.enabled = false;
            
            // Stop particles
            if (LightParticles != null)
                LightParticles.Stop();
            
            // Stop loop and play deactivate sound
            if (LightAudioSource != null)
            {
                LightAudioSource.loop = false;
                LightAudioSource.Stop();
                
                if (LightDeactivateSound != null)
                    LightAudioSource.PlayOneShot(LightDeactivateSound);
            }
            
            Debug.Log("Light Power DEACTIVATED");
        }
        
        private void DetectEnemiesInLight()
        {
            // Get all enemies in range
            Collider[] enemies = Physics.OverlapSphere(LightOrigin.position, LightRange, EnemyLayer);
            
            foreach (Collider enemyCollider in enemies)
            {
                // Check if enemy is within light cone
                Vector3 directionToEnemy = (enemyCollider.transform.position - LightOrigin.position).normalized;
                float angleToEnemy = Vector3.Angle(LightOrigin.forward, directionToEnemy);
                
                if (angleToEnemy <= LightAngle / 2f)
                {
                    // Check line of sight (no walls blocking)
                    if (Physics.Raycast(LightOrigin.position, directionToEnemy, out RaycastHit hit, LightRange))
                    {
                        if (hit.collider == enemyCollider)
                        {
                            // Enemy is in light cone and visible
                            EnemyHealth enemyHealth = enemyCollider.GetComponent<EnemyHealth>();
                            if (enemyHealth != null)
                            {
                                enemyHealth.TakeLightDamage(Time.deltaTime);
                            }
                        }
                    }
                }
            }
        }
        
        // Debug visualization
        private void OnDrawGizmos()
        {
            if (LightOrigin == null) return;
            
            Gizmos.color = _isLightActive ? Color.yellow : Color.gray;
            
            // Draw light range sphere
            Gizmos.DrawWireSphere(LightOrigin.position, LightRange);
            
            // Draw light cone
            Vector3 forward = LightOrigin.forward * LightRange;
            Vector3 right = Quaternion.Euler(0, LightAngle / 2f, 0) * forward;
            Vector3 left = Quaternion.Euler(0, -LightAngle / 2f, 0) * forward;
            
            Gizmos.DrawRay(LightOrigin.position, forward);
            Gizmos.DrawRay(LightOrigin.position, right);
            Gizmos.DrawRay(LightOrigin.position, left);
        }
    }
}