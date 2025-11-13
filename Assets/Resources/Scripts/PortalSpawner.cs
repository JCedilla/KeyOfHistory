using System.Collections;
using UnityEngine;

namespace KeyOfHistory.Manager
{
    public class PortalSpawner : MonoBehaviour
    {
        [Header("Portal Object")]
        [SerializeField] private GameObject PortalDoorPrefab;
        [SerializeField] private Transform PortalSpawnPoint;
        
        [Header("Earthquake Settings")]
        [SerializeField] private float RumbleDuration = 3f;
        [SerializeField] private float RiseSpeed = 2f;
        [SerializeField] private float FinalHeight = 5f;
        
        [Header("Effects")]
        [SerializeField] private ParticleSystem RumbleParticles;
        [SerializeField] private AudioClip RumbleSound;
        [SerializeField] private AudioClip PortalEmergenceSound;
        
        [Header("Camera Shake")]
        [SerializeField] private float ShakeIntensity = 0.5f;
        [SerializeField] private float ShakeFrequency = 2f;
        
        private GameObject _spawnedPortal;
        private bool _hasSpawned = false;
        
        public void SpawnPortal()
        {
            if (_hasSpawned) return;
            _hasSpawned = true;
            
            StartCoroutine(PortalEmergenceSequence());
        }
        
        private IEnumerator PortalEmergenceSequence()
        {
            // PHASE 1: Rumble Start
            if (RumbleSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(RumbleSound);
            }
            
            if (RumbleParticles != null)
            {
                RumbleParticles.Play();
            }
            
            StartCameraShake();
            
            yield return new WaitForSeconds(1f);
            
            // PHASE 2: Portal Spawns Underground
            Vector3 spawnPos = PortalSpawnPoint.position;
            spawnPos.y -= FinalHeight; // Start below ground
            
            _spawnedPortal = Instantiate(PortalDoorPrefab, spawnPos, PortalSpawnPoint.rotation);
            _spawnedPortal.SetActive(true);
            
            // Disable interaction initially
            var portalDoor = _spawnedPortal.GetComponent<PortalDoor>();
            if (portalDoor != null)
            {
                portalDoor.enabled = false;
            }
            
            // PHASE 3: Rise from Ground
            if (PortalEmergenceSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(PortalEmergenceSound);
            }
            
            Vector3 targetPos = PortalSpawnPoint.position;
            float elapsed = 0f;
            float duration = FinalHeight / RiseSpeed;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                _spawnedPortal.transform.position = Vector3.Lerp(spawnPos, targetPos, t);
                
                yield return null;
            }
            
            _spawnedPortal.transform.position = targetPos;
            
            // PHASE 4: Stabilize
            StopCameraShake();
            
            if (RumbleParticles != null)
            {
                RumbleParticles.Stop();
            }
            
            yield return new WaitForSeconds(0.5f);
            
            // PHASE 5: Enable Interaction
            if (portalDoor != null)
            {
                portalDoor.enabled = true;
            }
            
            Debug.Log("Portal fully emerged and ready!");
        }
        
        private void StartCameraShake()
        {
            // Simple camera shake implementation
            StartCoroutine(CameraShakeCoroutine());
        }
        
        private void StopCameraShake()
        {
            StopCoroutine(nameof(CameraShakeCoroutine));
        }
        
        private IEnumerator CameraShakeCoroutine()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null) yield break;
            
            Vector3 originalPos = mainCam.transform.localPosition;
            
            while (true)
            {
                float x = Random.Range(-1f, 1f) * ShakeIntensity;
                float y = Random.Range(-1f, 1f) * ShakeIntensity;
                
                mainCam.transform.localPosition = originalPos + new Vector3(x, y, 0);
                
                yield return new WaitForSeconds(1f / ShakeFrequency);
            }
        }
    }
}