using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class ShotgunBulletBehavior : BaseBulletBehavior
    {
        private static readonly int PARTICLE_HIT_HASH = ParticlesController.GetHash("Shotgun Hit");
        private static readonly int PARTICLE_WALL_HIT_HASH = ParticlesController.GetHash("Shotgun Wall Hit");

        [SerializeField] TrailRenderer trailRenderer;
        [SerializeField] Transform graphicsTransform;

        public override void Initialise(float damage, float speed, BaseEnemyBehavior currentTarget, float autoDisableTime, bool autoDisableOnHit = true)
        {
            base.Initialise(damage, speed, currentTarget, autoDisableTime, autoDisableOnHit);

            trailRenderer.Clear();

            transform.localScale = Vector3.one * 0.1f;
            transform.DOScale(1.0f, 0.25f).SetEasing(Ease.Type.CubicIn);
        }

        protected override void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior)
        {
            ParticlesController.PlayParticle(PARTICLE_HIT_HASH).SetPosition(transform.position);

            baseEnemyBehavior.Stun(0.1f);

            trailRenderer.Clear();
        }

        protected override void OnObstacleHitted()
        {
            base.OnObstacleHitted();

            ParticlesController.PlayParticle(PARTICLE_WALL_HIT_HASH).SetPosition(transform.position);
            trailRenderer.Clear();
        }
    }
}