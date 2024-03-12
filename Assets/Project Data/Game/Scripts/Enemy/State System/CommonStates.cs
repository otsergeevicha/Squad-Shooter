using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy
{
    public class PatrollingState: EnemyStateBehavior
    {
        protected readonly int ANIMATOR_SPEED_HASH = Animator.StringToHash("Movement Speed");

        private bool isStationary = false;

        private Vector3 FirstPoint => Enemy.PatrollingPoints[0];
        private int pointId = 0;

        private TweenCase idleCase;

        public PatrollingState(BaseEnemyBehavior enemy): base(enemy)
        {

        }

        public override void OnStart()
        {
            idleCase.KillActive();

            if (Enemy.PatrollingPoints.Length == 0 || (Enemy.PatrollingPoints.Length == 1 && Vector3.Distance(Position, FirstPoint) < 1))
            {
                isStationary = true;
            }
            else
            {
                Enemy.NavMeshAgent.speed = Enemy.Stats.PatrollingSpeed;

                pointId = 0;
                GoToPoint();
            }
        }

        private void GoToPoint()
        {
            var point = Enemy.PatrollingPoints[pointId];

            Enemy.MoveToPoint(point);

            isStationary = false;
        }

        public override void OnUpdate()
        {
            if (!isStationary && Vector3.Distance(Position, Enemy.PatrollingPoints[pointId]) < 1)
            {
                if (Enemy.PatrollingPoints.Length == 1)
                {
                    isStationary = true;
                    Enemy.NavMeshAgent.isStopped = true;
                }
                else
                {
                    pointId++;
                    if (pointId == Enemy.PatrollingPoints.Length) pointId = 0;

                    idleCase.KillActive();
                    idleCase = Tween.DelayedCall(Enemy.Stats.PatrollingIdleDuration, GoToPoint);
                }
            }

            Enemy.Animator.SetFloat(ANIMATOR_SPEED_HASH, Enemy.NavMeshAgent.velocity.magnitude / Enemy.NavMeshAgent.speed * Enemy.Stats.PatrollingMutliplier);
        }

        public override void OnEnd()
        {
            Enemy.StopMoving();
            idleCase.KillActive();
        }
    }

    public class FollowingState: EnemyStateBehavior
    {
        public FollowingState(BaseEnemyBehavior enemy) : base(enemy)
        {

        }

        protected readonly int ANIMATOR_SPEED_HASH = Animator.StringToHash("Movement Speed");

        private Vector3 cachedTargetPos;
        private bool isSlowed = false;

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
        }

        public override void OnEnd()
        {
            Enemy.StopMoving();
        }
    }

    public class FleeingState: EnemyStateBehavior
    {
        public FleeingState(BaseEnemyBehavior enemy) : base(enemy)
        {

        }

        protected readonly int ANIMATOR_SPEED_HASH = Animator.StringToHash("Movement Speed");

        private Vector3 fleePoint;

        public override void OnStart()
        {
            Enemy.NavMeshAgent.speed = Enemy.Stats.MoveSpeed;
            fleePoint = GetRandomPointOnLevel();

            Enemy.MoveToPoint(fleePoint);
        }

        public override void OnUpdate()
        {
            if (Vector3.Distance(Enemy.transform.position, fleePoint) < 5f || Vector3.Distance(Enemy.TargetPosition, fleePoint) < Enemy.Stats.FleeDistance)
            {
                fleePoint = GetRandomPointOnLevel();
                Enemy.MoveToPoint(fleePoint);
            }

            Enemy.Animator.SetFloat(ANIMATOR_SPEED_HASH, Enemy.NavMeshAgent.velocity.magnitude / Enemy.Stats.MoveSpeed);
        }

        public override void OnEnd()
        {
            Enemy.StopMoving();
        }

        public Vector3 GetRandomPointOnLevel()
        {
            int counter = 0;
            while (true)
            {
                counter++;

                var testPoint = Enemy.Position + Random.onUnitSphere.SetY(0) * Random.Range(10, 100);

                if (UnityEngine.AI.NavMesh.SamplePosition(testPoint, out var hit, 0.5f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    if (Vector3.Distance(Enemy.Target.position, testPoint) > Enemy.Stats.AttackDistance)
                    {
                        return testPoint;
                    }
                }

                if (counter > 1000)
                {
                    return Enemy.Position;

                }
            }
        }
    }

    public class AttackingState: EnemyStateBehavior
    {
        public AttackingState(BaseEnemyBehavior enemy) : base(enemy)
        {

        }

        public bool IsFinished { get; private set; }

        public override void OnStart()
        {
            IsFinished = false;

            Enemy.OnAttackFinished += OnAttackFinished;

            Enemy.Attack();
        }

        public override void OnUpdate()
        {
            Enemy.transform.rotation = Quaternion.LookRotation((Enemy.TargetPosition - Enemy.Position).SetY(0).normalized);
        }

        public override void OnEnd()
        {
            Enemy.OnAttackFinished -= OnAttackFinished;
        }

        private void OnAttackFinished()
        {
            IsFinished = true;
        }
    }

    public class AimAndAttackState : EnemyStateBehavior
    {
        public AimAndAttackState(BaseEnemyBehavior enemy) : base(enemy)
        {
            rangeEnemy = enemy as RangeEnemyBehaviour;
        }

        private RangeEnemyBehaviour rangeEnemy;

        public bool IsFinished { get; private set; }

        private bool AimHash { set => Enemy.Animator.SetBool("Aim", value); }

        private float nextAttackTime = 0;
        private bool hasAttacked = false;

        public override void OnStart()
        {
            AimHash = true;
            IsFinished = false;

            Enemy.CanMove = false;
            nextAttackTime = Time.time + Enemy.Stats.AimDuration;

            if(rangeEnemy != null && rangeEnemy.CanReload)
            {
                Enemy.OnReloadFinished += OnAttackFinished;
            } else
            {
                Enemy.OnAttackFinished += OnAttackFinished;
            }
            

            hasAttacked = false;
        }

        public override void OnUpdate()
        {
            Enemy.transform.rotation = Quaternion.Lerp(Enemy.transform.rotation, Quaternion.LookRotation((TargetPosition - Position).normalized), Time.deltaTime * 5);

            if (!hasAttacked && Time.time > nextAttackTime)
            {
                Enemy.Attack();
                hasAttacked = true;
            }
        }

        public override void OnEnd()
        {
            AimHash = false;

            if (rangeEnemy != null && rangeEnemy.CanReload)
            {
                Enemy.OnReloadFinished -= OnAttackFinished;
            }
            else
            {
                Enemy.OnAttackFinished -= OnAttackFinished;
            }
        }

        private void OnAttackFinished()
        {
            IsFinished = true;
        }
    }
}