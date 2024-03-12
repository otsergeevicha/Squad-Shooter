using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class SniperEnemyBehavior : BaseEnemyBehavior
    {
        [SerializeField] Transform weaponExit;
        public Transform WeaponExit => weaponExit;

        [SerializeField] Transform laserTransform;
        public Transform LaserTransform => laserTransform;
        [SerializeField] MeshRenderer laserRenderer;
        [SerializeField] Color alertColor;
        [SerializeField] Color redColor;

        private Pool bulletPool;

        [Header("Fighting")]
        [SerializeField] GameObject bulletPrefab;
        [SerializeField] float bulletSpeed;
        [SerializeField] float attackDistance;
        [SerializeField] float hitCooldown;
        [SerializeField] LayerMask targetLayer;

        [Space]
        [SerializeField] float yellowAimingDuration;
        [SerializeField] float redAimingDuration;

        public float YellowAimingDuration => yellowAimingDuration;
        public float RedAimingDuration => redAimingDuration;

        [SerializeField] bool isRedStatic;
        public bool IsRedStatic => isRedStatic;

        [Space]
        [SerializeField] ParticleSystem gunFireParticle;

        public void EnableLaser()
        {
            laserRenderer.gameObject.SetActive(true);
            laserRenderer.material.SetColor("_BaseColor", alertColor);
        }

        public void MakeLaserRed()
        {
            laserRenderer.material.SetColor("_BaseColor", redColor);
        }

        public void AimLaser()
        {
            var startPos = WeaponExit.position;
            var direction = Rotation * Vector3.forward;

            if (Physics.Raycast(startPos - direction * 10f, direction, out var hitInfo, 150f, LayerMask.GetMask("Obstacle")))
            {
                var hitPoint = hitInfo.point;
                var middlePoint = (startPos + hitPoint) / 2f;

                LaserTransform.position = middlePoint;
                LaserTransform.localScale = new Vector3(0.3f, 0.3f, Vector3.Distance(hitPoint, startPos));
            }
            else
            {
                var hitPoint = startPos + direction * 150;
                var middlePoint = (startPos + hitPoint) / 2f;

                LaserTransform.position = middlePoint;
                LaserTransform.localScale = new Vector3(0.3f, 0.3f, Vector3.Distance(hitPoint, startPos));
                ;
            }
        }

        public void DisableLaser()
        {
            laserRenderer.gameObject.SetActive(false);
        }

        protected override void Awake()
        {
            base.Awake();
            laserRenderer.gameObject.SetActive(false);
            bulletPool = new Pool(new PoolSettings(bulletPrefab.name, bulletPrefab, 3, true));
        }

        private void Update()
        {
            if (!LevelController.IsGameplayActive)
                return;

            healthbarBehaviour.FollowUpdate();
        }

        public override void Attack()
        {
            animatorRef.SetTrigger("Shoot");
        }

        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {
            if (enemyCallbackType == EnemyCallbackType.Hit)
            {
                EnemyBaseBulletBehavior bullet = bulletPool.GetPooledObject(new PooledObjectSettings(false).SetPosition(weaponExit.position).SetEulerRotation(weaponExit.eulerAngles)).GetComponent<EnemyBaseBulletBehavior>();
                bullet.transform.forward = transform.forward;
                bullet.Initialise(GetCurrentDamage(), bulletSpeed, 200);

                gunFireParticle.Play();

                AudioController.PlaySound(AudioController.Sounds.enemySniperShoot);
            }
            else if (enemyCallbackType == EnemyCallbackType.HitFinish)
            {
                InvokeOnAttackFinished();
            }
        }
    }
}