using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class EnemyAnimationCallback : MonoBehaviour
    {
        private BaseEnemyBehavior baseEnemyBehavior;

        public void Initialise(BaseEnemyBehavior baseEnemyBehavior)
        {
            this.baseEnemyBehavior = baseEnemyBehavior;
        }

        public void OnCallbackInvoked(EnemyCallbackType enemyCallbackType)
        {
            baseEnemyBehavior.OnAnimatorCallback(enemyCallbackType);
        }

        public void OnHitCallback()
        {
            OnCallbackInvoked(EnemyCallbackType.Hit);
        }

        public void OnLeftHitCallback()
        {
            OnCallbackInvoked(EnemyCallbackType.LeftHit);
        }

        public void OnRightHitCallback()
        {
            OnCallbackInvoked(EnemyCallbackType.RightHit);
        }

        public void OnHitFinishCallback()
        {
            OnCallbackInvoked(EnemyCallbackType.HitFinish);
        }

        public void OnBossLeftStepCallback()
        {
            OnCallbackInvoked(EnemyCallbackType.BossLeftStep);
        }

        public void OnBossRightStepCallback()
        {
            OnCallbackInvoked(EnemyCallbackType.BossRightStep);
        }

        public void OnBossDeathFallCallback()
        {
            OnCallbackInvoked(EnemyCallbackType.BossDeathFall);
        }

        public void OnBossEnterFallCallback()
        {
            OnCallbackInvoked(EnemyCallbackType.BossEnterFall);
        }

        public void OnBossKickCallback()
        {
            OnCallbackInvoked(EnemyCallbackType.BossKick);
        }

        public void OnBossEnterFallFinishedCallback()
        {
            OnCallbackInvoked(EnemyCallbackType.BossEnterFallFinished);
        }

        public void OnReloadFinishedCallback()
        {
            OnCallbackInvoked(EnemyCallbackType.ReloadFinished);
        }
    }

    public enum EnemyCallbackType
    {
        Hit = 0,
        HitFinish = 1,
        BossLeftStep = 2,
        BossRightStep = 3,
        BossDeathFall = 4,
        BossEnterFall = 5,
        BossKick = 6,
        BossEnterFallFinished = 7,
        ReloadFinished = 8,
        LeftHit = 9,
        RightHit = 10,
    }
}
