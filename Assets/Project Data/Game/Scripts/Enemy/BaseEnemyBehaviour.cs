using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Watermelon;
using Watermelon.Enemy;
using Watermelon.LevelSystem;
using Random = UnityEngine.Random;

namespace Watermelon.SquadShooter
{
    public abstract class BaseEnemyBehavior : MonoBehaviour, IHealth, INavMeshAgent
    {
        private static readonly Color HIT_OVERLAY_COLOR = new Color(0.6f, 0.6f, 0.6f, 1.0f);

        protected readonly int ANIMATOR_RUN_HASH = Animator.StringToHash("Running");
        protected readonly int ANIMATOR_SPEED_HASH = Animator.StringToHash("Movement Speed");

        public static readonly int ANIMATOR_HIT_HASH = Animator.StringToHash("Hit");
        public static readonly int ANIMATOR_HIT_INDEX_HASH = Animator.StringToHash("Hit Index");
        public static readonly float ANIMATOR_HIT_COOLDOWN = 0.08f;

        private static readonly int SHADER_HIT_SHINE_COLOR_HASH = Shader.PropertyToID("_EmissionColor");

        private static readonly int MASK_PERCENT_HASH = Shader.PropertyToID("_MaskPercent");

        [SerializeField] EnemyType type;
        public EnemyType Type => type;

        [SerializeField] EnemyTier tier = EnemyTier.Regular;
        public EnemyTier Tier { get => tier; private set => tier = value; }

        protected EnemyStats stats;
        public EnemyStats Stats => stats;

        [SerializeField]
        protected Animator animatorRef;
        public Animator Animator => animatorRef;
        [SerializeField] Transform hipsTransform;
        public Transform HipsTransform => hipsTransform;

        [SerializeField]
        protected HealthbarBehaviour healthbarBehaviour;
        [SerializeField] EnemyAnimationCallback enemyAnimationCallback;

        [Space]
        [SerializeField]
        protected SkinnedMeshRenderer meshRenderer;
        public bool IsVisible => meshRenderer.isVisible;

        [Space]
        [SerializeField] EnemyRagdollBehavior ragdoll;
        public EnemyRagdollBehavior Ragdoll => ragdoll;

        [SerializeField]
        protected Collider baseHitCollider;

        protected IStateMachine StateMachine { get; private set; }

        [Space]
        [SerializeField] EliteCase eliteCase;

        [Header("Weapon")]
        [SerializeField] List<WeaponCase> weaponCases;
        public List<WeaponCase> WeaponCases => weaponCases;

        public bool CanPursue { get; set; }
        public bool CanMove { get; set; }
        public bool IsAttacking { get; set; }

        // Health
        protected float currentHealth;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => stats.Hp * (tier == EnemyTier.Elite ? stats.EliteHealthMult : 1f);

        protected bool isDead;
        public bool IsDead => isDead;

        protected bool isStuned;
        public bool IsStuned => isStuned;

        protected bool chaseMode; // activates after first hit - never stop chasing target

        // movement
        protected NavMeshAgent navMeshAgent;
        public NavMeshAgent NavMeshAgent => navMeshAgent;

        public float RunningSpeed { get; protected set; }
        public float WalkingSpeed { get; protected set; }
        public bool IsWalking { get; set; }

        public Vector3 Position => transform.position;
        public Quaternion Rotation { get => transform.rotation; set => transform.rotation = value; }

        public Vector3 TargetPosition => Target.position;
        public float VisionRange => visionRange;

        public bool IsTargetInVisionRange => Vector3.Distance(transform.position, Target.position) <= visionRange;
        public bool IsTargetInAttackRange => Vector3.Distance(transform.position, Target.position) <= stats.AttackDistance;
        public bool IsTargetInFleeRange => Vector3.Distance(transform.position, Target.position) <= stats.FleeDistance;

        public bool HasTakenDamage { get; private set; }

        protected Transform target;
        public Transform Target => target;

        protected CharacterBehaviour characterBehaviour;

        protected CapsuleCollider enemyCollider;
        protected Rigidbody enemyRigidbody;

        public static OnEnemyDiedDelegate OnDiedEvent;

        private MaterialPropertyBlock hitShinePropertyBlock;
        private TweenCase hitShineTweenCase;

        private float lastFlotingTextTime;
        private float lastDamagedTime;
        private float lastHitShineTime;

        protected float hitAnimationTime;

        public HealthbarBehaviour HealthbarBehaviour => healthbarBehaviour;

        private Vector3[] patrollingPoints;
        public Vector3[] PatrollingPoints => patrollingPoints;

        private List<DropData> dropData;

        protected EnemyData enemyData;

        protected float visionRange;
        protected TweenCase ragdollCase;

        private float hitOffsetMult;

        protected virtual void Awake()
        {
            isDead = true;
            navMeshAgent = GetComponent<NavMeshAgent>();
            enemyCollider = GetComponent<CapsuleCollider>();
            enemyRigidbody = GetComponent<Rigidbody>();

            hitShinePropertyBlock = new MaterialPropertyBlock();

            enemyAnimationCallback.Initialise(this);

            navMeshAgent.enabled = false;

            RunningSpeed = navMeshAgent.speed;
            WalkingSpeed = RunningSpeed / 2f;

            weaponCases.ForEach((wCase) => wCase.Init());

            if (baseHitCollider)
                baseHitCollider.enabled = true;

            StateMachine = GetComponent<IStateMachine>();
        }

        [Button("Hit Animation")]
        public void HitAnimation()
        {
            HitAnimation(Random.Range(0, 2));
        }

        public void SetEnemyData(EnemyData enemyData, bool isElite)
        {
            if (isElite)
            {
                Tier = EnemyTier.Elite;
                eliteCase.SetElite();
            }
            else if (Tier != EnemyTier.Boss)
            {
                Tier = EnemyTier.Regular;
                eliteCase.SetRegular();
            }

            this.enemyData = enemyData;
            stats = enemyData.Stats;

            visionRange = enemyData.Stats.VisionRange;
        }

        public virtual void Initialise()
        {
            characterBehaviour = CharacterBehaviour.GetBehaviour();
            target = characterBehaviour.transform;
            hitOffsetMult = 1f;

            transform.localScale = Vector3.one;

            // Disable rigidbody
            enemyRigidbody.isKinematic = true;
            enemyRigidbody.useGravity = false;


            enemyCollider.isTrigger = true;

            animatorRef.gameObject.SetActive(true);

            // health
            currentHealth = MaxHealth;

            // Initialise healthbar
            healthbarBehaviour.Initialise(transform, this, true, healthbarBehaviour.HealthbarOffset, LevelController.CurrentLevelData.EnemiesLevel, Tier == EnemyTier.Elite);

            isDead = false;
            chaseMode = false;

            NavMeshController.InvokeOrSubscribe(this);

            LevelController.OnPlayerDiedEvent += OnRoomDone;
            LevelController.OnPlayerExitLevelEvent += OnRoomDone;

            baseHitCollider.enabled = true;

            HasTakenDamage = false;

            StateMachine.StartMachine();
        }

        public virtual void OnRoomDone()
        {
            LevelController.OnPlayerDiedEvent -= OnRoomDone;
            LevelController.OnPlayerExitLevelEvent -= OnRoomDone;

            weaponCases.ForEach((wCase) => wCase.Reset());
            ragdollCase.KillActive();
        }

        public void OnNavMeshUpdated()
        {
            navMeshAgent.enabled = true;
            navMeshAgent.stoppingDistance = stats.PreferedDistanceToPlayer;
            navMeshAgent.speed = stats.MoveSpeed;
            navMeshAgent.angularSpeed = stats.AngularSpeed;
        }
        #region Combat

        public int GetCurrentDamage()
        {
            return (int)(stats.Damage.Random() * (tier == EnemyTier.Elite ? stats.EliteDamageMult : 1f));
        }

        public void StartChasing()
        {
            visionRange = 9999;
        }

        public event SimpleCallback OnTakenDamage;
        public event SimpleCallback OnReloadFinished;
        protected Vector3 lastProjectilePosition;

        protected void InvokeOnReloadFinished()
        {
            OnReloadFinished?.Invoke();
        }

        public virtual void TakeDamage(float damage, Vector3 projectilePosition, Vector3 projectileDirection)
        {
            if (damage <= 0)
                return;

            currentHealth = Mathf.Clamp(currentHealth - damage, 0, MaxHealth);

            healthbarBehaviour.OnHealthChanged();

            lastProjectilePosition = projectilePosition - projectileDirection;

            if (currentHealth <= 0)
                OnDeath();

            transform.position += (transform.position - target.position).normalized * hitOffsetMult;
            hitOffsetMult *= 0.8f;

            HitEffect();

            if (lastFlotingTextTime + 0.18f <= Time.realtimeSinceStartup)
            {
                lastFlotingTextTime = Time.realtimeSinceStartup;

                FloatingTextController.SpawnFloatingText("Hit", "-" + damage.ToString("F0"), transform.position + transform.forward * stats.HitTextOffsetForward + new Vector3(Random.Range(-2.0f, 2.0f), stats.HitTextOffsetY, Random.Range(-0.5f, 0.5f)), Quaternion.identity, 1f);
            }

            lastDamagedTime = Time.realtimeSinceStartup;

            OnTakenDamage?.Invoke();

            HasTakenDamage = true;
        }

        protected virtual void FixedUpdate()
        {
            if (hitOffsetMult < 1 && lastDamagedTime + 0.5f < Time.realtimeSinceStartup)
            {
                hitOffsetMult += Time.fixedDeltaTime;
            }
        }

        protected virtual void OnDeath()
        {
            isDead = true;

            navMeshAgent.enabled = false;

            // Disable healthbar
            healthbarBehaviour.DisableBar();

            enemyCollider.isTrigger = false;
            animatorRef.enabled = false;

            ShowDeathFallAnimation();

            if (baseHitCollider)
                baseHitCollider.enabled = false;

            EnableRagdoll(deathExplosionForce, lastProjectilePosition);
            ragdollCase = Tween.DelayedCall(2.0f, () =>
            {
                ragdoll?.Disable();
                weaponCases.ForEach((wCase) => wCase.Activate());
            });

            DropResources();

            AudioController.PlaySound(AudioController.Sounds.enemyScreems.GetRandomItem());

            LevelController.OnEnemyKilled(this);

            OnDiedEvent?.Invoke(this);

            HasTakenDamage = false;

            StateMachine.StopMachine();
        }

        public void DisableWeapons()
        {
            weaponCases.ForEach((wCase) => wCase.Activate());
        }

        private void EnableRagdoll(float force, Vector3 point)
        {
            ragdoll?.ActivateWithForce(point, force, deathExplosionRadius);
        }

        private void ShowDeathFallAnimation()
        {
            // Enable rigidbody
            enemyRigidbody.isKinematic = true;
            enemyRigidbody.useGravity = false;

            enemyRigidbody.AddExplosionForce(deathExplosionForce, transform.position + (transform.forward * 0.5f).SetY(15f) + transform.right * Random.Range(-0.5f, 0.5f), deathExplosionRadius);
        }

        public float deathExplosionForce = 7000, deathExplosionRadius = 100;

        protected void HitEffect()
        {
            if (lastHitShineTime + 0.11f > Time.realtimeSinceStartup)
                return;

            lastHitShineTime = Time.realtimeSinceStartup;

            if (hitShineTweenCase != null && !hitShineTweenCase.isCompleted)
                hitShineTweenCase.Kill();

            meshRenderer.GetPropertyBlock(hitShinePropertyBlock);
            hitShinePropertyBlock.SetColor(SHADER_HIT_SHINE_COLOR_HASH, HIT_OVERLAY_COLOR);
            meshRenderer.SetPropertyBlock(hitShinePropertyBlock);

            hitShineTweenCase = meshRenderer.DOPropertyBlockColor(SHADER_HIT_SHINE_COLOR_HASH, hitShinePropertyBlock, Color.black, 0.32f);
        }

        protected void SetMaskPercent(float percent)
        {
            meshRenderer.GetPropertyBlock(hitShinePropertyBlock);
            hitShinePropertyBlock.SetFloat(MASK_PERCENT_HASH, percent);
            meshRenderer.SetPropertyBlock(hitShinePropertyBlock);
        }

        protected void HitAnimation(int animationIndex)
        {
            animatorRef.SetTrigger(ANIMATOR_HIT_HASH);
            animatorRef.SetInteger(ANIMATOR_HIT_INDEX_HASH, animationIndex);

            hitAnimationTime = Time.time + ANIMATOR_HIT_COOLDOWN;
        }
        #endregion

        #region Target
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
        #endregion

        public void OnDestroy()
        {
            if (healthbarBehaviour != null && healthbarBehaviour.HealthBarTransform != null)
            {
                Destroy(healthbarBehaviour.HealthBarTransform.gameObject);
            }
        }

        public abstract void OnAnimatorCallback(EnemyCallbackType enemyCallbackType);

        public delegate void OnEnemyDiedDelegate(BaseEnemyBehavior enemy);

        public event SimpleBoolCallback OnStunned;

        public void Stun(float duration)
        {
            isStuned = true;

            OnStunned?.Invoke(true);

            Tween.DelayedCall(duration, () =>
            {
                isStuned = false;
                OnStunned?.Invoke(false);
            });
        }

        public abstract void Attack();
        public event SimpleCallback OnAttackFinished;

        protected void InvokeOnAttackFinished()
        {
            OnAttackFinished?.Invoke();
        }

        public void ResetDrop()
        {
            dropData = new List<DropData>();
        }

        public void AddDrop(DropData drop)
        {
            dropData.Add(drop);
        }

        protected void DropResources()
        {
            if (!LevelController.IsGameplayActive)
                return;

            if (!dropData.IsNullOrEmpty())
            {
                for (int i = 0; i < dropData.Count; i++)
                {
                    if (dropData[i].dropType == DropableItemType.Currency)
                    {
                        int itemsAmount = Mathf.Clamp(Tier == EnemyTier.Elite ? Random.Range(7, 11) : Random.Range(3, 6), 1, dropData[i].amount);

                        List<int> itemValues = LevelController.SplitIntEqually(dropData[i].amount, itemsAmount);

                        for (int j = 0; j < itemValues.Count; j++)
                        {
                            DropData itemDropData = new DropData() { dropType = dropData[i].dropType, currencyType = dropData[i].currencyType, amount = itemValues[j] };

                            Tween.DelayedCall(i * 0.05f, () =>
                            {
                                Drop.DropItem(itemDropData, transform.position, Vector3.zero.SetY(Random.Range(0f, 360f)), DropFallingStyle.Coin, 0.5f);
                            });
                        }

                        AudioController.PlaySound(AudioController.Sounds.coinAppear, 0.6f);
                    }
                    else if (dropData[i].dropType == DropableItemType.WeaponCard)
                    {
                        for (int j = 0; j < dropData[i].amount; j++)
                        {
                            WeaponCardDropBehaviour card = Drop.DropItem(new DropData() { dropType = dropData[i].dropType, cardType = dropData[i].cardType, amount = 1 }, transform.position, Vector3.zero, DropFallingStyle.Default, 0.6f).GetComponent<WeaponCardDropBehaviour>();
                            card.SetCardData(dropData[i].cardType);
                        }
                    }
                    else
                    {
                        for (int j = 0; j < dropData[i].amount; j++)
                        {
                            Drop.DropItem(new DropData() { dropType = dropData[i].dropType, amount = 1 }, transform.position, Vector3.zero.SetY(Random.Range(0f, 360f)), DropFallingStyle.Default, 0.6f);
                        }
                    }
                }
            }

            if (Random.Range(0.0f, 1.0f) <= ActiveRoom.LevelData.HealSpawnPercent)
            {
                int health = Mathf.RoundToInt(Stats.HpForPlayer.Random());

                Drop.DropItem(new DropData() { dropType = DropableItemType.Heal, amount = health }, transform.position, Vector3.zero.SetY(Random.Range(0f, 360f)), DropFallingStyle.Coin, 0.3f, -1);
            }
        }

        public void MoveToPoint(Vector3 pos)
        {
            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(pos);
            }
        }

        public void StopMoving()
        {
            if (gameObject.activeInHierarchy && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
                navMeshAgent.isStopped = true;
        }

        public void SetAnimMovementMultiplier(float speedMultiplier)
        {
            animatorRef.SetFloat(ANIMATOR_SPEED_HASH, navMeshAgent.velocity.magnitude / RunningSpeed * speedMultiplier);
        }

        public void SetPatrollingPoints(Vector3[] points)
        {
            patrollingPoints = points;
        }

        public virtual void Unload()
        {
            healthbarBehaviour.Destroy();

            if (navMeshAgent.isActiveAndEnabled)
                navMeshAgent.isStopped = true;
        }

        public bool IsTargetInSight()
        {
            var origin = transform.position.SetY(2);
            var ray = new Ray(origin, (Target.position.SetY(2) - origin).normalized);
            if (Physics.Raycast(ray, out var hit, Stats.AttackDistance, 328))
            {
                if (hit.collider.gameObject == Target.gameObject)
                    return true;
            }

            return false;
        }
    }
}