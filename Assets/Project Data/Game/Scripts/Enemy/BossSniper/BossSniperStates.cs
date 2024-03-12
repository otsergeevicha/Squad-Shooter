using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.BossSniper
{
    public enum State
    {
        ChangingPosition,
        Aiming,
        Shooting
    }

    public class BossSniperChangingPositionState : EnemyStateBehavior
    {
        private int positionId = 0;
        private Vector3 nextPosition;

        protected readonly int ANIMATOR_SPEED_HASH = Animator.StringToHash("Movement Speed");

        public BossSniperChangingPositionState(BossSniperBehavior enemy) : base(enemy)
        {

        }

        public override void OnStart()
        {
            positionId++;
            if (positionId >= Enemy.PatrollingPoints.Length) positionId = 0;

            nextPosition = Enemy.PatrollingPoints[positionId];
            Enemy.MoveToPoint(nextPosition);
        }

        public override void OnUpdate()
        {
            if(Vector3.Distance(Enemy.Position, nextPosition) < 1f)
            {
                InvokeOnFinished();
            }

            Enemy.Animator.SetFloat(ANIMATOR_SPEED_HASH, Enemy.NavMeshAgent.velocity.magnitude / Enemy.Stats.MoveSpeed);
        }

        public override void OnEnd()
        {
            Enemy.StopMoving();
        }
    }

    public class BossSniperAimState: EnemyStateBehavior
    {
        private BossSniperBehavior boss;

        public BossSniperAimState(BossSniperBehavior enemy) : base(enemy)
        {
            this.boss = enemy;
        }

        private bool isYellow;
        private TweenCase delayedCase;

        private float startAimingTime;

        public override void OnStart()
        {
            isYellow = true;

            boss.MakeLaserYellow();

            delayedCase = Tween.DelayedCall(boss.YellowAinimgDuration, () => {
                isYellow = false;
                boss.MakeLaserRed();

                delayedCase = Tween.DelayedCall(boss.RedAimingDuration, () => {
                    InvokeOnFinished();
                });
            });

            startAimingTime = Time.time;

            boss.Animator.SetBool("Aim", true);

        }

        public override void OnUpdate()
        {
            if (isYellow || (!isYellow && !boss.IsRedStatic))
            {
                boss.Rotation = Quaternion.Lerp(boss.Rotation, Quaternion.LookRotation((TargetPosition - Position).normalized), Time.deltaTime * 5);
            }

            if (Time.time > startAimingTime + 0.25f)
            {

                boss.AimLaser();
            }
        }

        public override void OnEnd()
        {
            delayedCase.KillActive();

            boss.DisableLaser();

            boss.Animator.SetBool("Aim", false);
        }
    }

    public class BossSniperAttackState : EnemyStateBehavior
    {
        public BossSniperAttackState(BossSniperBehavior enemy) : base(enemy)
        {

        }

        public override void OnStart()
        {
            Enemy.Attack();
            Enemy.OnAttackFinished += OnAttackEnded;
        }

        private void OnAttackEnded()
        {
            InvokeOnFinished();
        }

        public override void OnEnd()
        {
            Enemy.OnAttackFinished -= OnAttackEnded;
        }
    }
}