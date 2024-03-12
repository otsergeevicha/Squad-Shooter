using UnityEngine;
using UnityEngine.AI;
using Watermelon;
using Watermelon.LevelSystem;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class CharacterBehaviour : MonoBehaviour, IEnemyDetector, IHealth, INavMeshAgent
    {
        private static readonly int SHADER_HIT_SHINE_COLOR_HASH = Shader.PropertyToID("_EmissionColor");

        private static CharacterBehaviour characterBehaviour;

        [SerializeField] NavMeshAgent agent;
        [SerializeField] EnemyDetector enemyDetector;

        [Header("Health")]
        [SerializeField] HealthbarBehaviour healthbarBehaviour;
        public HealthbarBehaviour HealthbarBehaviour => healthbarBehaviour;

        [SerializeField] ParticleSystem healingParticle;

        [Header("Target")]
        [SerializeField] GameObject targetRingPrefab;
        [SerializeField] Color targetRingActiveColor;
        [SerializeField] Color targetRingDisabledColor;
        [SerializeField] Color targetRingSpecialColor;

        [Space(5)]
        [SerializeField] AimRingBehavior aimRingBehavior;

        // Character Graphics
        private BaseCharacterGraphics graphics;
        public BaseCharacterGraphics Graphics => graphics;

        private GameObject graphicsPrefab;
        private SkinnedMeshRenderer characterMeshRenderer;

        private MaterialPropertyBlock hitShinePropertyBlock;
        private TweenCase hitShineTweenCase;

        private CharacterStats stats;
        public CharacterStats Stats => stats;

        // Gun
        private BaseGunBehavior gunBehaviour;
        public BaseGunBehavior Weapon => gunBehaviour;

        private GameObject gunPrefabGraphics;

        // Health
        private float currentHealth;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => stats.Health;
        public bool FullHealth => currentHealth == stats.Health;

        public bool IsActive => isActive;
        private bool isActive;

        private Joystick joystick;

        public static Transform Transform => characterBehaviour.transform;

        // Movement
        private MovementSettings movementSettings;
        private MovementSettings movementAimingSettings;

        private MovementSettings activeMovementSettings;
        public MovementSettings MovementSettings => activeMovementSettings;

        private bool isMoving;
        private float speed = 0;

        private Vector3 movementVelocity;
        public Vector3 MovementVelocity => movementVelocity;

        public EnemyDetector EnemyDetector => enemyDetector;

        private bool isCloseEnemyFound;
        public bool IsCloseEnemyFound => isCloseEnemyFound;

        private BaseEnemyBehavior closestEnemyBehaviour;
        public BaseEnemyBehavior ClosestEnemyBehaviour => closestEnemyBehaviour;

        private Transform playerTarget;
        private GameObject targetRing;
        private Renderer targetRingRenderer;
        private TweenCase ringTweenCase;

        private VirtualCameraCase mainCameraCase;
        public VirtualCameraCase MainCameraCase => mainCameraCase;

        private bool isMovementActive = false;
        public bool IsMovementActive => isMovementActive;

        public static bool NoDamage { get; private set; } = false;

        public static SimpleCallback OnDied;

        private void Awake()
        {
            agent.enabled = false;
        }

        public void Initialise()
        {
            characterBehaviour = this;

            hitShinePropertyBlock = new MaterialPropertyBlock();

            isActive = false;
            enabled = false;

            // Create target
            GameObject tempTarget = new GameObject("[TARGET]");
            tempTarget.transform.position = transform.position;
            tempTarget.SetActive(true);

            playerTarget = tempTarget.transform;

            // Get camera case
            mainCameraCase = CameraController.GetCamera(CameraType.Main);

            // Initialise enemy detector
            enemyDetector.Initialise(this);

            // Set health
            currentHealth = MaxHealth;

            // Initialise healthbar
            healthbarBehaviour.Initialise(transform, this, true, CharactersController.SelectedCharacter.GetCurrentStage().HealthBarOffset);

            aimRingBehavior.Init(transform);

            // Get UI components
            UIGame mainUI = UIController.GetPage<UIGame>();

            // Link joystick
            joystick = mainUI.Joystick;

            CameraController.SetMainTarget(transform);

            targetRing = Instantiate(targetRingPrefab, new Vector3(0f, 0f, -999f), Quaternion.identity);
            targetRingRenderer = targetRing.GetComponent<Renderer>();

            aimRingBehavior.Hide();
        }

        public void Reload(bool resetHealth = true)
        {
            // Set health
            if (resetHealth)
            {
                currentHealth = MaxHealth;
            }

            healthbarBehaviour.EnableBar();
            healthbarBehaviour.RedrawHealth();

            enemyDetector.Reload();

            enemyDetector.gameObject.SetActive(false);

            graphics.Reload();
            gunBehaviour.Reload();

            gameObject.SetActive(true);
        }

        public void ResetDetector()
        {
            var radius = enemyDetector.DetectorRadius;
            enemyDetector.SetRadius(0);
            Tween.NextFrame(() => enemyDetector.SetRadius(radius), framesOffset: 2, updateMethod: TweenType.FixedUpdate);
        }

        public void Unload()
        {
            if (graphics != null)
                graphics.Unload();

            if (playerTarget != null)
                Destroy(playerTarget.gameObject);

            if (aimRingBehavior != null)
                Destroy(aimRingBehavior.gameObject);

            if (healthbarBehaviour != null)
                healthbarBehaviour.Destroy();
        }

        public void OnLevelLoaded()
        {
            if (gunBehaviour != null)
                gunBehaviour.OnLevelLoaded();
        }

        public void OnNavMeshUpdated()
        {
            if (agent.isOnNavMesh)
            {
                agent.enabled = true;
                agent.isStopped = false;
            }
        }

        public void ActivateAgent()
        {
            agent.enabled = true;
            agent.isStopped = false;
        }

        public static void DisableNavmeshAgent()
        {
            characterBehaviour.agent.enabled = false;
        }

        public virtual void TakeDamage(float damage)
        {
            if (currentHealth <= 0)
                return;

            currentHealth = Mathf.Clamp(currentHealth - damage, 0, MaxHealth);

            healthbarBehaviour.OnHealthChanged();

            mainCameraCase.Shake(0.04f, 0.04f, 0.3f, 1.4f);

            if (currentHealth <= 0)
            {
                healthbarBehaviour.DisableBar();
                OnCloseEnemyChanged(null);

                isActive = false;
                enabled = false;

                enemyDetector.gameObject.SetActive(false);
                aimRingBehavior.Hide();

                OnDeath();

                OnDied?.Invoke();

                Vibration.Vibrate(AudioController.Vibrations.longVibration);
            }

            HitEffect();

            AudioController.PlaySound(AudioController.Sounds.characterHit.GetRandomItem());
            Vibration.Vibrate(AudioController.Vibrations.shortVibration);

            FloatingTextController.SpawnFloatingText("PlayerHit", "-" + damage.ToString("F0"), transform.position + new Vector3(Random.Range(-2.0f, 2.0f), 24, Random.Range(-0.5f, 0.5f)), Quaternion.identity, 1f);
        }

        [Button]
        public void OnDeath()
        {
            graphics.OnDeath();

            Tween.DelayedCall(0.5f, LevelController.OnPlayerDied);
        }

        public void SetPosition(Vector3 position)
        {
            playerTarget.position = position.AddToZ(10f);
            transform.position = position;
            transform.rotation = Quaternion.identity;

            if(agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.Warp(position);
            }
        }

        protected void HitEffect()
        {
            if (hitShineTweenCase != null && !hitShineTweenCase.isCompleted)
                hitShineTweenCase.Kill();

            characterMeshRenderer.GetPropertyBlock(hitShinePropertyBlock);
            hitShinePropertyBlock.SetColor(SHADER_HIT_SHINE_COLOR_HASH, Color.white);
            characterMeshRenderer.SetPropertyBlock(hitShinePropertyBlock);

            hitShineTweenCase = characterMeshRenderer.DOPropertyBlockColor(SHADER_HIT_SHINE_COLOR_HASH, hitShinePropertyBlock, Color.black, 0.32f);

            graphics.PlayHitAnimation();
        }

        #region Gun
        public void SetGun(WeaponData weaponData, bool playBounceAnimation = false, bool playAnimation = false, bool playParticle = false)
        {
            var gunUpgrade = UpgradesController.GetUpgrade<BaseWeaponUpgrade>(weaponData.UpgradeType);
            var currentStage = gunUpgrade.GetCurrentStage();

            // Check if graphics isn't exist already
            if (gunPrefabGraphics != currentStage.WeaponPrefab)
            {
                // Store prefab link
                gunPrefabGraphics = currentStage.WeaponPrefab;

                if (gunBehaviour != null)
                {
                    gunBehaviour.OnGunUnloaded();

                    Destroy(gunBehaviour.gameObject);
                }

                if (gunPrefabGraphics != null)
                {
                    GameObject gunObject = Instantiate(gunPrefabGraphics);
                    gunObject.SetActive(true);

                    gunBehaviour = gunObject.GetComponent<BaseGunBehavior>();

                    if (graphics != null)
                    {
                        gunBehaviour.InitialiseCharacter(graphics);
                        gunBehaviour.PlaceGun(graphics);
                        gunBehaviour.SetGraphicsState(true);

                        graphics.SetShootingAnimation(gunBehaviour.GetShootAnimationClip());

                        gunBehaviour.UpdateHandRig();
                    }
                }
            }

            if (gunBehaviour != null)
            {
                gunBehaviour.Initialise(this, weaponData);

                Vector3 defaultScale = gunBehaviour.transform.localScale;

                if (playAnimation)
                {
                    gunBehaviour.transform.localScale = defaultScale * 0.8f;
                    gunBehaviour.transform.DOScale(defaultScale, 0.15f).SetEasing(Ease.Type.BackOut);
                }

                if (playBounceAnimation)
                    gunBehaviour.PlayBounceAnimation();

                if (playParticle)
                    gunBehaviour.PlayUpgradeParticle();
            }

            enemyDetector.SetRadius(currentStage.RangeRadius);
            aimRingBehavior.SetRadius(currentStage.RangeRadius);
        }

        public void OnGunShooted()
        {
            graphics.OnShoot();
        }
        #endregion

        #region Graphics
        public void SetStats(CharacterStats stats)
        {
            this.stats = stats;

            currentHealth = stats.Health;

            if (healthbarBehaviour != null)
                healthbarBehaviour.OnHealthChanged();
        }

        public void SetGraphics(GameObject newGraphicsPrefab, bool playParticle, bool playAnimation)
        {
            // Check if graphics isn't exist already
            if (graphicsPrefab != newGraphicsPrefab)
            {
                // Store prefab link
                graphicsPrefab = newGraphicsPrefab;

                if (graphics != null)
                {
                    if (gunBehaviour != null)
                        gunBehaviour.transform.SetParent(null);

                    graphics.Unload();

                    Destroy(graphics.gameObject);
                }

                GameObject graphicObject = Instantiate(newGraphicsPrefab);
                graphicObject.transform.SetParent(transform);
                graphicObject.transform.ResetLocal();
                graphicObject.SetActive(true);

                graphics = graphicObject.GetComponent<BaseCharacterGraphics>();
                graphics.Initialise(this);

                movementSettings = graphics.MovementSettings;
                movementAimingSettings = graphics.MovementAimingSettings;

                activeMovementSettings = movementSettings;

                characterMeshRenderer = graphics.MeshRenderer;

                if (gunBehaviour != null)
                {
                    gunBehaviour.InitialiseCharacter(graphics);
                    gunBehaviour.PlaceGun(graphics);
                    gunBehaviour.SetGraphicsState(true);

                    graphics.SetShootingAnimation(gunBehaviour.GetShootAnimationClip());

                    gunBehaviour.UpdateHandRig();

                    Jump();
                }
                else
                {
                    Tween.NextFrame(Jump, 0, false, TweenType.LateUpdate);
                }

                if (playParticle)
                    graphics.PlayUpgradeParticle();

                if (playAnimation)
                    graphics.PlayBounceAnimation();
            }
        }
        #endregion

        public void Activate(bool check = true)
        {
            if (check && isActive)
                return;

            isActive = true;
            enabled = true;

            enemyDetector.gameObject.SetActive(true);

            aimRingBehavior.Show();

            graphics.Activate();

            NavMeshController.InvokeOrSubscribe(this);
        }

        public void Disable()
        {
            if (!isActive)
                return;

            isActive = false;
            enabled = false;

            agent.enabled = false;

            aimRingBehavior.Hide();

            graphics.Disable();

            if (isMoving)
            {
                isMoving = false;

                speed = 0;
            }
        }

        public void MoveForwardAndDisable(float duration)
        {
            agent.enabled = false;

            transform.DOMove(transform.position + Vector3.forward * activeMovementSettings.MoveSpeed * duration, duration).OnComplete(() =>
            {
                Disable();
            });
        }

        public void DisableAgent()
        {
            agent.enabled = false;
        }

        public void ActivateMovement()
        {
            isMovementActive = true;

            aimRingBehavior.Show();
        }

        private void Update()
        {
            if (gunBehaviour != null)
                gunBehaviour.UpdateHandRig();

            if (!isActive)
                return;

            if (joystick.IsJoysticTouched && joystick.Input.sqrMagnitude > 0.1f)
            {
                if (!isMoving)
                {
                    isMoving = true;

                    speed = 0;

                    graphics.OnMovingStarted();
                }

                float maxAlowedSpeed = joystick.FormatInput.magnitude * activeMovementSettings.MoveSpeed;

                if (speed > maxAlowedSpeed)
                {
                    speed -= activeMovementSettings.Acceleration * Time.deltaTime;
                    if (speed < maxAlowedSpeed)
                    {
                        speed = maxAlowedSpeed;
                    }
                }
                else
                {
                    speed += activeMovementSettings.Acceleration * Time.deltaTime;
                    if (speed > maxAlowedSpeed)
                    {
                        speed = maxAlowedSpeed;
                    }
                }

                movementVelocity = transform.forward * speed;

                transform.position += joystick.FormatInput * Time.deltaTime * speed;

                graphics.OnMoving(Mathf.InverseLerp(0, activeMovementSettings.MoveSpeed, speed), joystick.FormatInput, isCloseEnemyFound);

                if (!isCloseEnemyFound)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(joystick.FormatInput.normalized), Time.deltaTime * activeMovementSettings.RotationSpeed);
                }
            }
            else
            {
                if (isMoving)
                {
                    isMoving = false;

                    movementVelocity = Vector3.zero;

                    graphics.OnMovingStoped();

                    speed = 0;
                }
            }

            if (isCloseEnemyFound)
            {
                playerTarget.position = Vector3.Lerp(playerTarget.position, new Vector3(closestEnemyBehaviour.transform.position.x, transform.position.y, closestEnemyBehaviour.transform.position.z), Time.deltaTime * activeMovementSettings.RotationSpeed);

                transform.LookAt(new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z));
            }

            targetRing.transform.rotation = Quaternion.identity;

            healthbarBehaviour.FollowUpdate();
            aimRingBehavior.UpdatePosition();
        }

        private void FixedUpdate()
        {
            graphics.CustomFixedUpdate();

            if (gunBehaviour != null)
                gunBehaviour.GunUpdate();
        }

        public void OnCloseEnemyChanged(BaseEnemyBehavior enemyBehavior)
        {
            if (enemyBehavior != null)
            {
                if (closestEnemyBehaviour == null)
                {
                    playerTarget.position = transform.position + transform.forward * 5;
                }

                activeMovementSettings = movementAimingSettings;

                isCloseEnemyFound = true;
                closestEnemyBehaviour = enemyBehavior;

                targetRing.SetActive(true);
                targetRing.transform.rotation = Quaternion.identity;

                if (ringTweenCase != null && !ringTweenCase.isCompleted)
                    ringTweenCase.Kill();

                targetRing.transform.SetParent(enemyBehavior.transform);
                targetRing.transform.localScale = Vector3.one * enemyBehavior.Stats.TargetRingSize * 1.4f;
                targetRing.transform.localPosition = Vector3.zero;

                ringTweenCase = targetRing.transform.DOScale(Vector3.one * enemyBehavior.Stats.TargetRingSize, 0.2f).SetEasing(Ease.Type.BackIn);

                CameraController.SetEnemyTarget(enemyBehavior);

                SetTargetActive();

                return;
            }

            activeMovementSettings = movementSettings;

            isCloseEnemyFound = false;
            closestEnemyBehaviour = null;
            targetRing.SetActive(false);
            targetRing.transform.SetParent(null);

            CameraController.SetEnemyTarget(null);
        }

        public static BaseEnemyBehavior GetClosestEnemy()
        {
            return characterBehaviour.enemyDetector.ClosestEnemy;
        }

        public static CharacterBehaviour GetBehaviour()
        {
            return characterBehaviour;
        }

        public void TryAddClosestEnemy(BaseEnemyBehavior enemy)
        {
            EnemyDetector.TryAddClosestEnemy(enemy);
        }

        public void SetTargetActive()
        {
            if (closestEnemyBehaviour != null && closestEnemyBehaviour.Tier == EnemyTier.Elite)
            {
                targetRingRenderer.material.color = targetRingSpecialColor;
            }
            else
            {
                targetRingRenderer.material.color = targetRingActiveColor;
            }
        }

        public void SetTargetUnreachable()
        {
            targetRingRenderer.material.color = targetRingDisabledColor;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_ITEM))
            {
                IDropableItem item = other.GetComponent<IDropableItem>();
                if (item.IsPickable(this))
                {
                    OnItemPicked(item);
                    item.Pick();

                }
            }
            else if (other.CompareTag(PhysicsHelper.TAG_CHEST))
            {
                other.GetComponent<AbstractChestBehavior>().ChestApproached();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_CHEST))
            {
                other.GetComponent<AbstractChestBehavior>().ChestLeft();
            }
        }

        public void OnItemPicked(IDropableItem item)
        {
            if (item.DropType == DropableItemType.Currency)
            {
                if (item.DropData.currencyType == CurrencyType.Coin)
                {
                    if (item.IsRewarded)
                    {
                        LevelController.OnRewardedCoinPicked(item.DropAmount);
                    }
                    else
                    {
                        LevelController.OnCoinPicked(item.DropAmount);
                    }
                }
                else
                {
                    CurrenciesController.Add(item.DropData.currencyType, item.DropAmount);
                }
            }
            else if (item.DropType == DropableItemType.Heal)
            {
                currentHealth = Mathf.Clamp(currentHealth + item.DropAmount, 0, MaxHealth);
                healthbarBehaviour.OnHealthChanged();
                healingParticle.Play();
            }
        }

        [Button]
        public void Jump()
        {
            graphics.Jump();
            gunBehaviour.transform.localScale = Vector3.zero;
            gunBehaviour.gameObject.SetActive(false);
        }

        public void SpawnWeapon()
        {
            graphics.EnableRig();
            gunBehaviour.gameObject.SetActive(true);
            gunBehaviour.DOScale(1, 0.2f).SetCustomEasing(Ease.GetCustomEasingFunction("BackOutLight"));
        }

        private void OnDestroy()
        {
            if (healthbarBehaviour.HealthBarTransform != null)
                Destroy(healthbarBehaviour.HealthBarTransform.gameObject);

            if (aimRingBehavior != null)
                aimRingBehavior.OnPlayerDestroyed();
        }
    }
}