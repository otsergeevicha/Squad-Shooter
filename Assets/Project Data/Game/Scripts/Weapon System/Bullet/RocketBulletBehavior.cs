using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class RocketBulletBehavior : BaseBulletBehavior
    {
        private static readonly int PARTICLE_HIT_HASH = ParticlesController.GetHash("Rocket Hit");
        private static readonly int PARTICLE_WALL_HIT_HASH = ParticlesController.GetHash("Rocket Wall Hit");

        [SerializeField] float angularSmoothMin = 0.2f;
        [SerializeField] float angularSmoothMax = 0.7f;
        [SerializeField] float smothGrowSpeed = 0.5f;

        [Space]
        [SerializeField] float effectKillDealay = 0f;

        [Space]
        [SerializeField] GameObject visuals;
        [SerializeField] TrailRenderer trailRenderer;

        private float currentAngSmooth;

        public override void Initialise(float damage, float speed, BaseEnemyBehavior currentTarget, float autoDisableTime, bool autoDisableOnHit = true)
        {
            base.Initialise(damage, speed, currentTarget, autoDisableTime, autoDisableOnHit);

            visuals.SetActive(true);
            currentAngSmooth = angularSmoothMin;
        }

        protected override void FixedUpdate()
        {
            Vector3 targetDirection = currentTarget.transform.position.SetY(6.5f) - transform.position;
            Vector3 rotationDirection = Vector3.RotateTowards(transform.forward, targetDirection, 360, 0f);
            Quaternion targetRotation = Quaternion.LookRotation(rotationDirection);

            if (currentAngSmooth < angularSmoothMax)
            {
                currentAngSmooth += smothGrowSpeed * Time.fixedDeltaTime;
            }

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, currentAngSmooth);

            transform.position += transform.forward * speed * Time.fixedDeltaTime;

            if (currentTarget.IsDead)
            {
                KillBullet();
            }
        }

        protected override void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior)
        {
            ParticlesController.PlayParticle(PARTICLE_HIT_HASH).SetPosition(transform.position);

            visuals.SetActive(false);
            speed = 0;

            Tween.DelayedCall(effectKillDealay, () =>
            {
                KillBullet();
            });
        }

        private void KillBullet()
        {
            trailRenderer.Clear();
            gameObject.SetActive(false);
        }

        protected override void OnObstacleHitted()
        {
            base.OnObstacleHitted();
            ParticlesController.PlayParticle(PARTICLE_WALL_HIT_HASH).SetPosition(transform.position);
            KillBullet();
        }
    }
}