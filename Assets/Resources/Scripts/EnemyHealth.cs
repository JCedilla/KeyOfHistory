using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using KeyOfHistory.UI;

namespace KeyOfHistory.Enemy
{
    public class EnemyHealth : MonoBehaviour
    {
        [Header("Light Damage Settings")]
        [SerializeField] private float MaxLightExposure = 5f; // 5 seconds to defeat
        [SerializeField] private float LightExposureDrainRate = 2f; // How fast timer decreases when not in light
        
        [Header("Respawn Settings")]
        [SerializeField] private float RespawnTime = 15f;
        [SerializeField] private Vector3 SpawnPosition;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem DamageParticles;
        [SerializeField] private ParticleSystem ExplosionParticles;
        [SerializeField] private MeshRenderer EnemyRenderer;
        [SerializeField] private float ExplosionDuration = 2f; // NEW: How long explosion lasts before despawn
        
        [Header("Audio")]
        [SerializeField] private AudioSource EnemyAudioSource;
        [SerializeField] private AudioClip DamageSound;
        [SerializeField] private AudioClip ExplosionSound;
        
        private float _currentLightExposure = 0f;
        private bool _isBeingDamaged = false;
        private bool _isAlive = true;
        private Material _enemyMaterial;
        private Color _originalColor;
        private EnemyAI _enemyAI;
        private NavMeshAgent _navAgent;
        
        private void Start()
        {
            // Store spawn position
            SpawnPosition = transform.position;
            
            // Get material for visual feedback
            if (EnemyRenderer != null)
            {
                _enemyMaterial = EnemyRenderer.material;
                _originalColor = _enemyMaterial.color;
            }
            
            _enemyAI = GetComponent<EnemyAI>();
            _navAgent = GetComponent<NavMeshAgent>();
        }
        
        private void Update()
        {
            if (!_isAlive) return;
            
            // Drain light exposure when not being damaged
            if (!_isBeingDamaged && _currentLightExposure > 0f)
            {
                _currentLightExposure -= LightExposureDrainRate * Time.deltaTime;
                _currentLightExposure = Mathf.Max(_currentLightExposure, 0f);
                
                // Update UI while draining (NEW)
                if (_currentLightExposure > 0f && EnemyChaseUI.Instance != null)
                {
                    EnemyChaseUI.Instance.UpdateLightDamage(_currentLightExposure, MaxLightExposure);
                }

                // Update visual feedback
                UpdateDamageVisuals();
                
                // Stop damage particles if exposure is 0
                if (_currentLightExposure <= 0f && DamageParticles != null && DamageParticles.isPlaying)
                {
                    DamageParticles.Stop();
                    if (EnemyChaseUI.Instance != null)
                    {
                    EnemyChaseUI.Instance.ShowLightDamage(false);
                    }
                }
            }
            
            // Reset damage flag each frame (LightPowerController will set it if still in light)
            _isBeingDamaged = false;
        }
        
        public void TakeLightDamage(float damageAmount)
        {
            if (!_isAlive) return;
            
            _isBeingDamaged = true;
            _currentLightExposure += damageAmount;
            
            // DEBUG: Show timer going up
            Debug.Log($"{gameObject.name} Light Exposure: {_currentLightExposure:F2} / {MaxLightExposure}");

            if (EnemyChaseUI.Instance != null)
            {
                EnemyChaseUI.Instance.ShowLightDamage(true);
                EnemyChaseUI.Instance.UpdateLightDamage(_currentLightExposure, MaxLightExposure);
            }
            
            // Play damage particles
            if (DamageParticles != null && !DamageParticles.isPlaying)
            {
                DamageParticles.Play();
            }
            
            // Play damage sound (only once when first hit)
            if (_currentLightExposure <= damageAmount && EnemyAudioSource != null && DamageSound != null)
            {
                EnemyAudioSource.PlayOneShot(DamageSound);
            }
            
            // Update visual feedback
            UpdateDamageVisuals();
            
            // Check if defeated
            if (_currentLightExposure >= MaxLightExposure)
            {
                Die();
            }
            
            // Slow down enemy while in light
            if (_enemyAI != null)
            {
                _enemyAI.SetSlowedByLight(true);
            }
        }
        
        private void UpdateDamageVisuals()
        {
            if (_enemyMaterial == null) return;
            
            // Lerp color from original to white based on exposure
            float exposurePercent = _currentLightExposure / MaxLightExposure;
            _enemyMaterial.color = Color.Lerp(_originalColor, Color.white, exposurePercent);
            
            // Make material glow more as exposure increases
            _enemyMaterial.SetColor("_EmissionColor", Color.white * exposurePercent * 2f);
        }
        
        private void Die()
        {
            if (!_isAlive) return;
            
            _isAlive = false;
            
            Debug.Log($"{gameObject.name} defeated by light!");

            // HIDE UI
            if (EnemyChaseUI.Instance != null)
            {
                EnemyChaseUI.Instance.ShowLightDamage(false);
            }
            
            // Disable enemy AI immediately
            if (_enemyAI != null)
            {
                _enemyAI.enabled = false;
            }
            
            // Disable NavMeshAgent immediately
            if (_navAgent != null)
            {
                _navAgent.enabled = false;
            }
            
            // Play explosion effect
            GameObject explosionInstance = null;
            if (ExplosionParticles != null)
            {
                explosionInstance = Instantiate(ExplosionParticles.gameObject, transform.position, Quaternion.identity);
                
                // Auto-destroy explosion after it finishes
                Destroy(explosionInstance, ExplosionDuration);
            }
            
            // Play explosion sound
            if (EnemyAudioSource != null && ExplosionSound != null)
            {
                AudioSource.PlayClipAtPoint(ExplosionSound, transform.position);
            }
            
            // Start despawn sequence
            StartCoroutine(DespawnAfterEffects());
        }
        
        private IEnumerator DespawnAfterEffects()
        {
            // Hide enemy immediately (but keep GameObject active for particles/sounds)
            if (EnemyRenderer != null)
            {
                EnemyRenderer.enabled = false;
            }
            
            // Disable collider so player can't interact
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }
            
            // Wait for explosion effects to finish
            Debug.Log($"Waiting {ExplosionDuration} seconds for death effects...");
            yield return new WaitForSeconds(ExplosionDuration);
            
            // Despawn (hide) the enemy
            Debug.Log($"{gameObject.name} despawning...");
            gameObject.SetActive(false);
            
            // Wait for respawn time
            Debug.Log($"Respawning in {RespawnTime} seconds...");
            yield return new WaitForSeconds(RespawnTime);
            
            // Respawn
            Respawn();
        }
        
        private void Respawn()
        {
            Debug.Log($"{gameObject.name} respawning NOW!");
            
            // Reactivate GameObject
            gameObject.SetActive(true);
            
            // Reset position
            transform.position = SpawnPosition;
            
            // Reset health
            _currentLightExposure = 0f;
            _isAlive = true;
            
            // Reset visual
            if (_enemyMaterial != null)
            {
                _enemyMaterial.color = _originalColor;
                _enemyMaterial.SetColor("_EmissionColor", Color.black);
            }
            
            // Re-enable renderer
            if (EnemyRenderer != null)
            {
                EnemyRenderer.enabled = true;
            }
            
            // Re-enable collider
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = true;
            }
            
            // Re-enable NavMeshAgent FIRST
            if (_navAgent != null)
            {
                _navAgent.enabled = true;
                _navAgent.Warp(SpawnPosition); // Teleport agent to spawn position
            }
            
            // Re-enable AI AFTER NavMeshAgent
            if (_enemyAI != null)
            {
                _enemyAI.enabled = true;
                _enemyAI.ResetToPatrol();
            }
            
            Debug.Log($"{gameObject.name} respawn complete!");
        }
        
        // Called by EnemyAI when light is no longer hitting enemy
        public void OnLightStopped()
        {
            if (_enemyAI != null)
            {
                _enemyAI.SetSlowedByLight(false);
            }
        }
    }
}