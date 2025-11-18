using UnityEngine;
using UnityEngine.AI;
using KeyOfHistory.Manager;
using KeyOfHistory.UI; // NEW

namespace KeyOfHistory.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("Patrol Settings")]
        [SerializeField] private Transform[] PatrolWaypoints;
        [SerializeField] private float WaypointWaitTime = 2f;
        [SerializeField] private float PatrolSpeed = 3f;
        
        [Header("Chase Settings")]
        [SerializeField] private float ChaseSpeed = 5f;
        [SerializeField] private float ChaseSlowedSpeed = 4f;
        [SerializeField] private Transform Player;
        [SerializeField] private float DetectionRange = 10f;
        [SerializeField] private float DetectionAngle = 60f;
        [SerializeField] private LayerMask ObstacleLayer;
        
        [Header("Attack Settings")]
        [SerializeField] private float AttackRange = 2f;
        [SerializeField] private float AttackCooldown = 2f;
        
        [Header("Audio")] // NEW
        [SerializeField] private AudioClip ChaseMusic;
        [SerializeField] private AudioSource ChaseAudioSource;
        
        private NavMeshAgent _agent;
        private int _currentWaypointIndex = 0;
        private float _waypointWaitTimer = 0f;
        private float _attackCooldownTimer = 0f;
        private bool _isSlowedByLight = false;
        private bool _isChaseMusicPlaying = false; // NEW
        
        public enum EnemyState { Patrol, Chase, Attack }
        public EnemyState CurrentState { get; private set; } = EnemyState.Patrol;
        
        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = PatrolSpeed;
            
            // Find player if not assigned
            if (Player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    Player = playerObj.transform;
                }
            }
            
            // Setup chase audio source if not assigned
            if (ChaseAudioSource == null)
            {
                ChaseAudioSource = gameObject.AddComponent<AudioSource>();
                ChaseAudioSource.loop = true;
                ChaseAudioSource.playOnAwake = false;
                ChaseAudioSource.spatialBlend = 0f; // 2D sound
            }
            
            // Start patrol
            if (PatrolWaypoints.Length > 0)
            {
                GoToNextWaypoint();
            }
        }
        
        private void Update()
        {
            _attackCooldownTimer -= Time.deltaTime;
            
            switch (CurrentState)
            {
                case EnemyState.Patrol:
                    UpdatePatrol();
                    CheckForPlayer();
                    break;
                    
                case EnemyState.Chase:
                    UpdateChase();
                    break;
                    
                case EnemyState.Attack:
                    UpdateAttack();
                    break;
            }
        }
        
        private void UpdatePatrol()
        {
            if (PatrolWaypoints.Length == 0) return;
            
            // Check if reached waypoint
            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
            {
                _waypointWaitTimer += Time.deltaTime;
                
                if (_waypointWaitTimer >= WaypointWaitTime)
                {
                    _waypointWaitTimer = 0f;
                    GoToNextWaypoint();
                }
            }
        }
        
        private void GoToNextWaypoint()
        {
            if (PatrolWaypoints.Length == 0) return;
            
            _currentWaypointIndex = (_currentWaypointIndex + 1) % PatrolWaypoints.Length;
            _agent.SetDestination(PatrolWaypoints[_currentWaypointIndex].position);
        }
        
        private void CheckForPlayer()
        {
            if (Player == null) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
            
            if (distanceToPlayer <= DetectionRange)
            {
                Vector3 directionToPlayer = (Player.position - transform.position).normalized;
                float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
                
                if (angleToPlayer <= DetectionAngle / 2f)
                {
                    Vector3 rayStart = transform.position + Vector3.up * 1f;
                    Vector3 rayDirection = directionToPlayer;
                    
                    if (Physics.Raycast(rayStart, rayDirection, out RaycastHit hit, DetectionRange))
                    {
                        if (hit.collider.CompareTag("Player"))
                        {
                            EnterChaseState();
                        }
                    }
                }
            }
        }
        
        private void EnterChaseState()
        {
            CurrentState = EnemyState.Chase;
            _agent.speed = _isSlowedByLight ? ChaseSlowedSpeed : ChaseSpeed;
            
            // Play chase music
            PlayChaseMusic();
            
            // Show chase warning UI
            if (EnemyChaseUI.Instance != null)
            {
                EnemyChaseUI.Instance.ShowChaseWarning(true);
            }
            
            Debug.Log($"{gameObject.name} is chasing player!");
        }
        
        private void UpdateChase()
        {
            if (Player == null)
            {
                ResetToPatrol();
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
            
            // Check if in attack range
            if (distanceToPlayer <= AttackRange)
            {
                CurrentState = EnemyState.Attack;
                _agent.isStopped = true;
                return;
            }
            
            // Chase player
            _agent.SetDestination(Player.position);
            
            // If player is too far, return to patrol
            if (distanceToPlayer > DetectionRange * 2f)
            {
                ResetToPatrol();
            }
        }
        
        private void UpdateAttack()
        {
            if (Player == null)
            {
                ResetToPatrol();
                return;
            }
            
            // Face player
            Vector3 directionToPlayer = (Player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0f, directionToPlayer.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);
            
            float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
            
            // If player moves away, chase again
            if (distanceToPlayer > AttackRange * 1.5f)
            {
                _agent.isStopped = false;
                CurrentState = EnemyState.Chase;
                return;
            }
            
            // Attack
            if (_attackCooldownTimer <= 0f)
            {
                PerformAttack();
                _attackCooldownTimer = AttackCooldown;
            }
        }
        
        private void PerformAttack()
        {
            Debug.Log($"{gameObject.name} attacks player!");
            // TODO: Deal damage to player when health system is ready
        }
        
        public void ResetToPatrol()
        {
            CurrentState = EnemyState.Patrol;
            _agent.speed = PatrolSpeed;
            _agent.isStopped = false;
            
            // Stop chase music
            StopChaseMusic();
            
            // Hide chase warning UI
            if (EnemyChaseUI.Instance != null)
            {
                EnemyChaseUI.Instance.ShowChaseWarning(false);
            }
            
            if (PatrolWaypoints.Length > 0)
            {
                _agent.SetDestination(PatrolWaypoints[_currentWaypointIndex].position);
            }
            
            Debug.Log($"{gameObject.name} returning to patrol");
        }
        
        public void SetSlowedByLight(bool slowed)
        {
            _isSlowedByLight = slowed;
            
            if (CurrentState == EnemyState.Chase)
            {
                _agent.speed = slowed ? ChaseSlowedSpeed : ChaseSpeed;
            }
        }
        
        // NEW: Chase music methods
        private void PlayChaseMusic()
        {
            if (ChaseAudioSource != null && ChaseMusic != null && !_isChaseMusicPlaying)
            {
                ChaseAudioSource.clip = ChaseMusic;
                ChaseAudioSource.Play();
                _isChaseMusicPlaying = true;
            }
        }
        
        private void StopChaseMusic()
        {
            if (ChaseAudioSource != null && _isChaseMusicPlaying)
            {
                ChaseAudioSource.Stop();
                _isChaseMusicPlaying = false;
            }
        }
        
        private void OnDisable()
        {
            // Stop chase music when enemy is disabled (e.g., defeated)
            StopChaseMusic();
            
            // Hide UI
            if (EnemyChaseUI.Instance != null)
            {
                EnemyChaseUI.Instance.ShowChaseWarning(false);
            }
        }
        
        // Debug visualization
        private void OnDrawGizmos()
        {
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, DetectionRange);
            
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange);
            
            // Draw detection cone
            Vector3 forward = transform.forward * DetectionRange;
            Vector3 right = Quaternion.Euler(0, DetectionAngle / 2f, 0) * forward;
            Vector3 left = Quaternion.Euler(0, -DetectionAngle / 2f, 0) * forward;
            
            Gizmos.color = CurrentState == EnemyState.Chase ? Color.red : Color.blue;
            Gizmos.DrawRay(transform.position, forward);
            Gizmos.DrawRay(transform.position, right);
            Gizmos.DrawRay(transform.position, left);
            
            // Draw line to player if detected
            if (Player != null && CurrentState == EnemyState.Chase)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, Player.position);
            }
        }
    }
}