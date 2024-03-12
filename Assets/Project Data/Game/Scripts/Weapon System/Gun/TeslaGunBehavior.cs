using UnityEngine;
using Watermelon;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class TeslaGunBehavior : BaseGunBehavior
    {
        [LineSpacer]
        [SerializeField] GameObject graphicsObject;
        [SerializeField] Transform shootPoint;
        [SerializeField] ParticleSystem shootParticleSystem;
        [SerializeField] GameObject lightningLoopParticle;

        [SerializeField] LayerMask targetLayers;
        [SerializeField] float chargeDuration;
        private DuoFloat bulletSpeed;
        [SerializeField] DuoInt targetsHitGoal;
        [SerializeField] float stunDuration = 0.2f;

        private Pool bulletPool;

        private TweenCase shootTweenCase;
        private Vector3 shootDirection;

        private bool isCharging;
        private bool isCharged;
        private bool isChargeParticleActivated;
        private float fullChargeTime;

        private TeslaGunUpgrade upgrade;

        public override void Initialise(CharacterBehaviour characterBehaviour, WeaponData data)
        {
            base.Initialise(characterBehaviour, data);

            upgrade = UpgradesController.GetUpgrade<TeslaGunUpgrade>(data.UpgradeType);
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
            bulletSpeed = stage.BulletSpeed;
        }

        public override void GunUpdate()
        {
            // if no enemy - cancel charge
            if (!characterBehaviour.IsCloseEnemyFound)
            {
                if (isCharging || isCharged)
                {
                    CancelCharge();
                }

                return;
            }

            // if not charging - start
            if (!isCharging && !isCharged)
            {
                isCharging = true;
                isChargeParticleActivated = false;
                fullChargeTime = Time.timeSinceLevelLoad + chargeDuration;
            }

            // wait for full charge
            if (fullChargeTime >= Time.timeSinceLevelLoad)
            {
                // start charge particle 0.5 sec before charge complete
                if (!isChargeParticleActivated && fullChargeTime - Time.timeSinceLevelLoad <= 0.5f)
                {
                    isChargeParticleActivated = true;
                    shootParticleSystem.Play();
                }

                if (IsEnemyVisible())
                {
                    characterBehaviour.SetTargetActive();
                }
                else
                {
                    characterBehaviour.SetTargetUnreachable();
                }

                return;
            }
            // activate loop particle once charged
            else if (!isCharged)
            {
                isCharged = true;
                lightningLoopParticle.SetActive(true);
            }

            if (IsEnemyVisible())
            {
                characterBehaviour.SetTargetActive();

                if (shootTweenCase != null && !shootTweenCase.isCompleted)
                    shootTweenCase.Kill();

                shootTweenCase = transform.DOLocalMoveZ(-1f, chargeDuration * 0.3f).OnComplete(delegate
                {
                    shootTweenCase = transform.DOLocalMoveZ(0, chargeDuration * 0.6f);
                });

                TeslaBulletBehavior bullet = bulletPool.GetPooledObject(new PooledObjectSettings().SetPosition(shootPoint.position).SetEulerRotation(characterBehaviour.transform.eulerAngles)).GetComponent<TeslaBulletBehavior>();
                bullet.Initialise(damage.Random() * characterBehaviour.Stats.BulletDamageMultiplier, bulletSpeed.Random(), characterBehaviour.ClosestEnemyBehaviour, 5f, false, stunDuration);
                bullet.SetTargetsHitGoal(targetsHitGoal.Random());

                characterBehaviour.OnGunShooted();
                characterBehaviour.MainCameraCase.Shake(0.04f, 0.04f, 0.3f, 0.8f);

                CancelCharge();

                AudioController.PlaySound(AudioController.Sounds.shotTesla, volumePercentage: 0.8f);
            }
            else
            {
                characterBehaviour.SetTargetUnreachable();
            }
        }

        public bool IsEnemyVisible()
        {
            shootDirection = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;

            RaycastHit hitInfo;
            if (Physics.Raycast(shootPoint.position - shootDirection.normalized * 10f, shootDirection, out hitInfo, 300f, targetLayers) ||
                Physics.Raycast(shootPoint.position, shootDirection, out hitInfo, 300f, targetLayers)
            )
            {
                if (hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
                {
                    if (Vector3.Angle(shootDirection, transform.forward) < 40f)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void CancelCharge()
        {
            isCharging = false;
            isCharged = false;
            isChargeParticleActivated = false;
            lightningLoopParticle.SetActive(false);
            shootParticleSystem.Stop();
        }

        private void OnDrawGizmos()
        {
            if (characterBehaviour == null)
                return;

            if (characterBehaviour.ClosestEnemyBehaviour == null)
                return;

            Color defCol = Gizmos.color;
            Gizmos.color = Color.red;

            Vector3 shootDirection = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;

            Gizmos.DrawLine(shootPoint.position - shootDirection.normalized * 10f, characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y));

            Gizmos.color = defCol;
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
            transform.SetParent(characterGraphics.TeslaHolderTransform);
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