using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class RifleEnemyBehavior : BaseEnemyBehavior
    {
        [SerializeField] Transform shootingPoint;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            healthbarBehaviour.FollowUpdate();
        }

        public override void Attack()
        {

        }

        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {

        }
    }
}