using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class MeleeEnemyBehaviour : BaseEnemyBehavior
    {
        private static readonly int HIT_PARTICLE_HASH = ParticlesController.GetHash("Enemy Melee Hit");

        private readonly int ANIMATOR_ATTACK_HASH = Animator.StringToHash("Attack");

        [Header("Fighting")]
        [SerializeField] float hitRadius;
        [SerializeField] DuoFloat slowDownDuration;
        [SerializeField] float slowDownSpeedMult;

        [Space]
        [SerializeField] Transform hitParticlePosition;

        private float slowRunningTimer;

        private bool isHitting;
        private bool isSlowRunning;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                Attack();
            }
        }

        public override void Attack()
        {
            if (isHitting)
                return;

            isHitting = true;
            ApplySlowDown();

            AudioController.PlaySound(AudioController.Sounds.enemyMeleeHit, 0.5f);

            animatorRef.SetTrigger(ANIMATOR_ATTACK_HASH);
        }

        private void ApplySlowDown()
        {
            isSlowRunning = true;
            IsWalking = true;
            slowRunningTimer = slowDownDuration.Random();

            navMeshAgent.speed = Stats.MoveSpeed * slowDownSpeedMult;
        }

        private void DisableSlowDown()
        {
            isSlowRunning = false;
            IsWalking = false;

            navMeshAgent.speed = Stats.MoveSpeed;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isDead)
                return;

            if (!LevelController.IsGameplayActive)
                return;

            healthbarBehaviour.FollowUpdate();

            if (isSlowRunning)
            {
                slowRunningTimer -= Time.deltaTime;

                if (slowRunningTimer <= 0)
                    DisableSlowDown();
            }
        }

        public override void TakeDamage(float damage, Vector3 projectilePosition, Vector3 projectileDirection)
        {
            if (isDead)
                return;

            base.TakeDamage(damage, projectilePosition, projectileDirection);

            if (hitAnimationTime < Time.time)
                HitAnimation(Random.Range(0, 2));
        }

        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {
            if (enemyCallbackType == EnemyCallbackType.Hit)
            {
                if (Vector3.Distance(transform.position, target.position) <= hitRadius)
                {
                    characterBehaviour.TakeDamage(GetCurrentDamage());

                    ParticlesController.PlayParticle(HIT_PARTICLE_HASH).SetPosition(hitParticlePosition.position);
                }
            }
            else if (enemyCallbackType == EnemyCallbackType.HitFinish)
            {
                isHitting = false;
                InvokeOnAttackFinished();
            }
        }
    }
}