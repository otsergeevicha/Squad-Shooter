using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.Boss
{
    public enum State
    {
        Hidden,
        Entering,
        Idle,
        Chasing,
        Shooting,
        Hitting,
    }

    public abstract class BossState : EnemyStateBehavior
    {
        protected BossBomberBehaviour boss;

        public BossState(BossBomberBehaviour enemy) : base(enemy)
        {
            boss = enemy;
        }
    }

    public class EnteringState: BossState
    {
        public EnteringState(BossBomberBehaviour enemy) : base(enemy){}

        public override void OnStart()
        {
            boss.OnEntered += InvokeOnFinished;
        }

        public override void OnEnd()
        {
            boss.OnEntered -= InvokeOnFinished;
        }
    }

    public class IdleState : BossState
    {
        public IdleState(BossBomberBehaviour enemy) : base(enemy) { }

        public override void OnStart()
        {
            boss.Idle();
        }
    }

    public class KikkingState : BossState
    {
        private float kickEndTime;

        public KikkingState(BossBomberBehaviour enemy) : base(enemy) { }

        public override void OnStart()
        {
            boss.PerformKick();

            kickEndTime = Time.time + boss.HitCooldown;
        }

        public override void OnUpdate()
        {
            if(Time.time >= kickEndTime)
            {
                InvokeOnFinished();
            }
        }
    }

    public class ShootingState : BossState
    {
        private float shootingEndTime;

        public ShootingState(BossBomberBehaviour enemy) : base(enemy) { }

        public override void OnStart()
        {
            boss.PerformHit();

            shootingEndTime = Time.time + boss.HitCooldown;
        }

        public override void OnUpdate()
        {
            if (Time.time >= shootingEndTime)
            {
                InvokeOnFinished();
            }
        }
    }

    public class ChasingState : BossState
    {
        public ChasingState(BossBomberBehaviour enemy) : base(enemy) { }

        public override void OnStart()
        {
            boss.ChasePlayer();
        }
    }
}