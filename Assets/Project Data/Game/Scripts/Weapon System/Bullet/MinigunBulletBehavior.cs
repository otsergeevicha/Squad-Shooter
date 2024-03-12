using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class MinigunBulletBehavior : BaseBulletBehavior
    {
        private static readonly int PARTICLE_HIT_HASH = ParticlesController.GetHash("Minigun Hit");
        private static readonly int PARTICLE_WAll_HIT_HASH = ParticlesController.GetHash("Minigun Wall Hit");

        [SerializeField] TrailRenderer trailRenderer;

        public override void Initialise(float damage, float speed, BaseEnemyBehavior currentTarget, float autoDisableTime, bool autoDisableOnHit = true)
        {
            base.Initialise(damage, speed, currentTarget, autoDisableTime, autoDisableOnHit);

            trailRenderer.Clear();
        }

        protected override void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior)
        {
            ParticlesController.PlayParticle(PARTICLE_HIT_HASH).SetPosition(transform.position);

            trailRenderer.Clear();
        }

        protected override void OnObstacleHitted()
        {
            base.OnObstacleHitted();

            ParticlesController.PlayParticle(PARTICLE_WAll_HIT_HASH).SetPosition(transform.position);
            trailRenderer.Clear();
        }
    }
}