using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class CowboyEnemyBehavior : BaseEnemyBehavior
    {
        [SerializeField] GameObject bulletPrefab;
        [SerializeField] float bulletSpeed;

        [Header("Left side")]
        [SerializeField] Transform leftShootPoint;
        [SerializeField] ParticleSystem leftGunFireParticle;

        [Header("Right side")]
        [SerializeField] Transform rightShootPoint;
        [SerializeField] ParticleSystem rightGunFireParticle;

        private Pool bulletPool;

        protected override void Awake()
        {
            base.Awake();

            bulletPool = new Pool(new PoolSettings(bulletPrefab.name, bulletPrefab, 3, true));
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            healthbarBehaviour.FollowUpdate();
        }

        public override void Attack()
        {
            animatorRef.SetTrigger("Shoot");
        }

        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {
            EnemyBaseBulletBehavior bullet;

            switch (enemyCallbackType)
            {
                case EnemyCallbackType.LeftHit:
                    bullet = bulletPool.GetPooledObject(new PooledObjectSettings(false).SetPosition(leftShootPoint.position).SetEulerRotation(leftShootPoint.eulerAngles)).GetComponent<EnemyBaseBulletBehavior>();
                    bullet.transform.LookAt(target.position.SetY(leftShootPoint.position.y));
                    bullet.Initialise(GetCurrentDamage(), bulletSpeed, 200);

                    leftGunFireParticle.Play();
                    AudioController.PlaySound(AudioController.Sounds.enemyShot);

                    break;
                case EnemyCallbackType.RightHit:
                    bullet = bulletPool.GetPooledObject(new PooledObjectSettings(false).SetPosition(rightShootPoint.position).SetEulerRotation(rightShootPoint.eulerAngles)).GetComponent<EnemyBaseBulletBehavior>();
                    bullet.transform.LookAt(target.position.SetY(rightShootPoint.position.y));
                    bullet.Initialise(GetCurrentDamage(), bulletSpeed, 200);

                    rightGunFireParticle.Play();
                    AudioController.PlaySound(AudioController.Sounds.enemyShot);

                    break;

                case EnemyCallbackType.HitFinish:
                    InvokeOnAttackFinished();
                    break;
            }
        }
    }
}