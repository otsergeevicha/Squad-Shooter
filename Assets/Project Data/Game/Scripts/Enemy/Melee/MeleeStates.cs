using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.Melee
{
    public class MeleeFollowAttackState: EnemyStateBehavior
    {
        public MeleeFollowAttackState(MeleeEnemyBehaviour melee): base(melee)
        {

        }

        protected readonly int ANIMATOR_SPEED_HASH = Animator.StringToHash("Movement Speed");

        private Vector3 cachedTargetPos;
        private bool isSlowed = false;

        private bool isAttacking = false;

        public override void OnStart()
        {
            cachedTargetPos = Enemy.Target.position;

            isSlowed = Enemy.IsWalking;
            if (isSlowed)
            {
                Enemy.NavMeshAgent.speed = Enemy.Stats.PatrollingSpeed;
            }
            else
            {
                Enemy.NavMeshAgent.speed = Enemy.Stats.MoveSpeed;
            }

            Enemy.MoveToPoint(cachedTargetPos);

            isAttacking = false;
        }

        public override void OnUpdate()
        {
            if (Vector3.Distance(Enemy.Target.position, cachedTargetPos) > 0.5f)
            {
                cachedTargetPos = Enemy.Target.position;
                Enemy.MoveToPoint(cachedTargetPos);
            }

            if (isSlowed && !Enemy.IsWalking)
            {
                Enemy.NavMeshAgent.speed = Enemy.Stats.MoveSpeed;
            }
            else if (!isSlowed && Enemy.IsWalking)
            {
                Enemy.NavMeshAgent.speed = Enemy.Stats.PatrollingSpeed;
            }

            Enemy.Animator.SetFloat(ANIMATOR_SPEED_HASH, Enemy.NavMeshAgent.velocity.magnitude / Enemy.NavMeshAgent.speed * (isSlowed ? Enemy.Stats.PatrollingMutliplier : 1f));

            if (Enemy.IsTargetInAttackRange && !isAttacking)
            {
                isAttacking = true;
                Enemy.Attack();
                Enemy.OnAttackFinished += OnAttackFinished;
            }
        }

        private void OnAttackFinished()
        {
            Enemy.OnAttackFinished -= OnAttackFinished;
            isAttacking = false;
        }

        public override void OnEnd()
        {
            Enemy.OnAttackFinished -= OnAttackFinished;
            Enemy.StopMoving();
        }
    }

    public enum State
    {
        Patrolling,
        Attacking,
    }
}
