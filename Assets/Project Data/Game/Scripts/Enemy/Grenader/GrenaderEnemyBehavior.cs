using System.Collections.Generic;
using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class GrenaderEnemyBehavior : BaseEnemyBehavior
    {
        [SerializeField] GameObject grenadePrefab;
        [SerializeField] GameObject eliteGrenadePrefab;

        [Space]
        [SerializeField] Transform grenadeStartPosition;

        private static PoolGeneric<GrenadeBehavior> grenadePool;
        protected override void Awake()
        {
            base.Awake();

            CanMove = true;

            if (PoolManager.PoolExists("Enemy Grenades Pool"))
            {
                grenadePool = PoolManager.GetPoolByName<GrenadeBehavior>("Enemy Grenades Pool");
            }
            else
            {
                grenadePool = new PoolGeneric<GrenadeBehavior>(new PoolSettings()
                {
                    name = "Enemy Grenades Pool",
                    multiPoolPrefabsList = new List<Pool.MultiPoolPrefab>
                {
                    new Pool.MultiPoolPrefab
                    {
                        prefab = grenadePrefab,
                        weight = 50,
                    },
                    new Pool.MultiPoolPrefab
                    {
                        prefab = eliteGrenadePrefab,
                        weight = 50,
                    },
                },
                    size = 5,
                    autoSizeIncrement = true,
                    type = Pool.PoolType.Multi,
                    objectsContainer = null
                });
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!LevelController.IsGameplayActive)
                return;

            healthbarBehaviour.FollowUpdate();
        }

        public override void Attack()
        {
            animatorRef.SetBool("Is Shooting", true);
        }

        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {
            switch (enemyCallbackType)
            {
                case EnemyCallbackType.Hit:
                    var grenade = grenadePool.GetMultiPooledObjectByIndex(Tier == EnemyTier.Elite ? 1 : 0, new PooledObjectSettings()).GetComponent<GrenadeBehavior>();

                    grenade.Throw(grenadeStartPosition.position, TargetPosition, GetCurrentDamage());

                    break;

                case EnemyCallbackType.HitFinish:
                    animatorRef.SetBool("Is Shooting", false);
                    InvokeOnAttackFinished();
                    break;
            }
        }
    }
}