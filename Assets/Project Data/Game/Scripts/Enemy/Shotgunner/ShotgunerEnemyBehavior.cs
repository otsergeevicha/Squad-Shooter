using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class ShotgunerEnemyBehavior : BaseEnemyBehavior
    {
        [Space]
        [SerializeField] Transform shootPoint;
        [SerializeField] ParticleSystem shootParticle;
        [SerializeField] ParticleSystem reloadParticle;

        [Space]
        [SerializeField] GameObject bulletPrefab;
        [SerializeField] float bulletSpeed;
        [SerializeField] float spreadAngle;
        [SerializeField] int bulletsCount;

        private Pool bulletPool;

        protected override void Awake()
        {
            base.Awake();

            bulletPool = new Pool(new PoolSettings(bulletPrefab.name, bulletPrefab, 3, true));
        }

        public override void Attack()
        {
            animatorRef.SetTrigger("Shoot");
        }

        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {
            switch (enemyCallbackType)
            {
                case EnemyCallbackType.Hit:
                    for (int i = 0; i < bulletsCount; i++)
                    {
                        var bullet = bulletPool.GetPooledObject(new PooledObjectSettings(false).SetPosition(shootPoint.position).SetEulerRotation(shootPoint.eulerAngles)).GetComponent<EnemyBaseBulletBehavior>();
                        bullet.transform.LookAt(target.position.SetY(shootPoint.position.y));
                        bullet.Initialise(GetCurrentDamage(), bulletSpeed, Stats.AttackDistance + 10f);
                        bullet.transform.Rotate(new Vector3(0f, i == 0 ? 0f : Random.Range(spreadAngle * -0.5f, spreadAngle * 0.5f), 0f));
                    }

                    shootParticle.Play();
                    AudioController.PlaySound(AudioController.Sounds.shotShotgun);

                    break;
                case EnemyCallbackType.HitFinish:
                    InvokeOnAttackFinished();
                    break;

                case EnemyCallbackType.ReloadFinished:
                    reloadParticle.Play();
                    break;
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            healthbarBehaviour.FollowUpdate();
        }
    }
}