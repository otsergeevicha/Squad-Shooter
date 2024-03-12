using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class RangeEnemyBehaviour : BaseEnemyBehavior
    {
        private readonly int ANIMATOR_ATTACK_HASH = Animator.StringToHash("Attack");

        [Header("Fighting")]
        [SerializeField] GameObject bulletPrefab;
        [SerializeField] float bulletSpeed;
        [SerializeField] float attackDistance;
        [SerializeField] float hitCooldown;
        [SerializeField] LayerMask targetLayer;

        [Header("Weapon")]
        [SerializeField] Transform weaponHolderTransform;
        [SerializeField] Transform weaponTransform;
        [SerializeField] Transform shootPointTransform;

        [Space]
        [SerializeField] ParticleSystem gunFireParticle;

        [Space]
        [SerializeField] bool canReload;
        public bool CanReload => canReload;

        private bool isHitting;

        private Pool bulletPool;

        protected override void Awake()
        {
            base.Awake();

            bulletPool = new Pool(new PoolSettings(bulletPrefab.name, bulletPrefab, 3, true));
        }

        public override void Attack()
        {
            animatorRef.SetTrigger("Shoot");
        }

        public override void Initialise()
        {
            base.Initialise();

            isDead = false;
            isHitting = false;
        }

        private void PerformHit()
        {
            if (isHitting)
                return;

            isHitting = true;

            navMeshAgent.isStopped = true;

            animatorRef.SetBool(ANIMATOR_RUN_HASH, false);
            animatorRef.SetTrigger(ANIMATOR_ATTACK_HASH);

            Tween.DelayedCall(0.2f, () =>
            {
                if (!IsDead)
                    AudioController.PlaySound(AudioController.Sounds.enemyShot, 0.6f);
            });
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!LevelController.IsGameplayActive)
                return;

            healthbarBehaviour.FollowUpdate();
        }

        public override void TakeDamage(float damage, Vector3 projectilePosition, Vector3 projectileDirection)
        {
            if (isDead)
                return;

            base.TakeDamage(damage, projectilePosition, projectileDirection);
        }

        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {
            if (enemyCallbackType == EnemyCallbackType.Hit)
            {
                EnemyBaseBulletBehavior bullet = bulletPool.GetPooledObject(new PooledObjectSettings(false).SetPosition(shootPointTransform.position).SetEulerRotation(shootPointTransform.eulerAngles)).GetComponent<EnemyBaseBulletBehavior>();
                bullet.transform.LookAt(target.position.SetY(shootPointTransform.position.y));
                bullet.Initialise(GetCurrentDamage(), bulletSpeed, 200);

                gunFireParticle.Play();

                AudioController.PlaySound(AudioController.Sounds.enemyShot);
            }
            else if (enemyCallbackType == EnemyCallbackType.HitFinish)
            {
                isHitting = false;
                InvokeOnAttackFinished();
            }
            else if (enemyCallbackType == EnemyCallbackType.ReloadFinished)
            {
                InvokeOnReloadFinished();
            }
        }
    }
}