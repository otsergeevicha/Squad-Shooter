using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.Sniper
{
    public enum State
    {
        Patrolling,
        Following,
        Attacking,
        Aiming,
        Fleeing
    }

    public class SniperAimState : EnemyStateBehavior
    {
        private SniperEnemyBehavior enemy;

        public SniperAimState(SniperEnemyBehavior enemy) : base(enemy)
        {
            this.enemy = enemy;
        }

        private bool isYellow;
        private TweenCase delayedCase;

        private float startAimingTime;

        public override void OnStart()
        {
            isYellow = true;

            enemy.EnableLaser();

            delayedCase = Tween.DelayedCall(enemy.YellowAimingDuration, () => {
                isYellow = false;
                enemy.MakeLaserRed();

                delayedCase = Tween.DelayedCall(enemy.RedAimingDuration, () => {
                    InvokeOnFinished();
                });
            });

            startAimingTime = Time.time;

            enemy.Animator.SetBool("Aim", true);

        }

        public override void OnUpdate()
        {
            if (isYellow || (!isYellow && !enemy.IsRedStatic))
            {
                enemy.Rotation = Quaternion.Lerp(enemy.Rotation, Quaternion.LookRotation((TargetPosition - Position).normalized), Time.deltaTime * 5);
            }

            if (Time.time > startAimingTime + 0.25f)
            {

                enemy.AimLaser();
            }
        }

        public override void OnEnd()
        {
            delayedCase.KillActive();

            enemy.DisableLaser();

            enemy.Animator.SetBool("Aim", false);
        }
    }
}
