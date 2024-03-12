using System.Collections.Generic;
using UnityEngine;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class MiniGunBehavior : BaseGunBehavior
    {
        [LineSpacer]
        [SerializeField] GameObject graphicsObject;

        [SerializeField] Transform shootPoint;
        [SerializeField] Transform barrelTransform;
        [SerializeField] ParticleSystem shootParticleSystem;

        [SerializeField] LayerMask targetLayers;
        [SerializeField] float bulletDisableTime;

        [Space]
        [SerializeField] float fireRotationSpeed;

        [Space]
        [SerializeField] List<float> bulletStreamAngles;

        private float spread;
        private float attackDelay;
        private DuoFloat bulletSpeed;

        private float nextShootTime;

        private Pool bulletPool;

        private Vector3 shootDirection;

        private MinigunUpgrade upgrade;

        private TweenCase shootTweenCase;

        public override void Initialise(CharacterBehaviour characterBehaviour, WeaponData data)
        {
            base.Initialise(characterBehaviour, data);

            upgrade = UpgradesController.GetUpgrade<MinigunUpgrade>(data.UpgradeType);

            GameObject bulletObj = (upgrade.CurrentStage as BaseWeaponUpgradeStage).BulletPrefab;
            bulletPool = new Pool(new PoolSettings(bulletObj.name, bulletObj, 5, true));

            RecalculateDamage();
        }

        public override void OnLevelLoaded()
        {
            RecalculateDamage();
        }

        public override void RecalculateDamage()
        {
            var stage = upgrade.GetCurrentStage();

            damage = stage.Damage;
            attackDelay = 1f / stage.FireRate;
            spread = stage.Spread;
            bulletSpeed = stage.BulletSpeed;
        }

        public override void GunUpdate()
        {
            if (!characterBehaviour.IsCloseEnemyFound)
                return;

            barrelTransform.Rotate(Vector3.forward * fireRotationSpeed);

            if (nextShootTime >= Time.timeSinceLevelLoad)
                return;

            shootDirection = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;

            if (Physics.Raycast(transform.position, shootDirection, out var hitInfo, 300f, targetLayers) && hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
            {
                if (Vector3.Angle(shootDirection, transform.forward.SetY(0f)) < 40f)
                {
                    if (shootTweenCase != null && !shootTweenCase.isCompleted)
                        shootTweenCase.Kill();

                    shootTweenCase = transform.DOLocalMoveZ(-0.55f, attackDelay * 0.3f).OnComplete(delegate
                    {
                        shootTweenCase = transform.DOLocalMoveZ(0, attackDelay * 0.6f);
                    });

                    characterBehaviour.SetTargetActive();

                    shootParticleSystem.Play();

                    nextShootTime = Time.timeSinceLevelLoad + attackDelay;

                    if (bulletStreamAngles.IsNullOrEmpty())
                    {
                        bulletStreamAngles = new List<float> { 0 };
                    }

                    for (int i = 0; i < bulletStreamAngles.Count; i++)
                    {
                        var streamAngle = bulletStreamAngles[i];

                        BaseBulletBehavior bullet = bulletPool
                            .GetPooledObject(new PooledObjectSettings()
                            .SetPosition(shootPoint.position)
                            .SetEulerRotation(characterBehaviour.transform.eulerAngles + Vector3.up * (Random.Range((float)-spread, spread) + streamAngle)))
                            .GetComponent<BaseBulletBehavior>();
                        bullet.Initialise(damage.Random() * characterBehaviour.Stats.BulletDamageMultiplier, bulletSpeed.Random(), characterBehaviour.ClosestEnemyBehaviour, bulletDisableTime);
                    }

                    characterBehaviour.OnGunShooted();

                    AudioController.PlaySound(AudioController.Sounds.shotMinigun);
                }
            }
            else
            {
                characterBehaviour.SetTargetUnreachable();
            }
        }

        public override void OnGunUnloaded()
        {
            // Destroy bullets pool
            if (bulletPool != null)
            {
                bulletPool.Clear();
                bulletPool = null;
            }
        }

        public override void PlaceGun(BaseCharacterGraphics characterGraphics)
        {
            transform.SetParent(characterGraphics.MinigunHolderTransform);
            transform.ResetLocal();
        }

        public override void SetGraphicsState(bool state)
        {
            graphicsObject.SetActive(state);
        }

        public override void Reload()
        {
            bulletPool.ReturnToPoolEverything();
        }
    }
}