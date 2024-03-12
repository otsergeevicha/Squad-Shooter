using UnityEngine;

namespace Watermelon.Upgrades
{
    [System.Serializable]
    public class BaseWeaponUpgrade : Upgrade<BaseWeaponUpgradeStage>
    {
        public override void Initialise()
        {

        }
    }

    [System.Serializable]
    public class BaseWeaponUpgradeStage : BaseUpgradeStage
    {
        [Header("Prefabs")]
        [SerializeField] GameObject weaponPrefab;
        public GameObject WeaponPrefab => weaponPrefab;

        [SerializeField] GameObject bulletPrefab;
        public GameObject BulletPrefab => bulletPrefab;

        [Header("Data")]
        [SerializeField] DuoInt damage;
        public DuoInt Damage => damage;

        [SerializeField] float rangeRadius;
        public float RangeRadius => rangeRadius;

        [SerializeField, Tooltip("Shots Per Second")] float fireRate;
        public float FireRate => fireRate;

        [SerializeField] float spread;
        public float Spread => spread;

        [SerializeField] int power;
        public int Power => power;

        [SerializeField] DuoInt bulletsPerShot = new DuoInt(1,1);
        public DuoInt BulletsPerShot => bulletsPerShot;

        [SerializeField] DuoFloat bulletSpeed;
        public DuoFloat BulletSpeed => bulletSpeed;

        // key upgrade - "ideal" way to play the game, based on this upgrades sequence is built economy
        [SerializeField] int keyUpgradeNumber = -1;
        public int KeyUpgradeNumber => keyUpgradeNumber;
    }
}