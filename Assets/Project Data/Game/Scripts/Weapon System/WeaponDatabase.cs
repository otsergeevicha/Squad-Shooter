using UnityEngine;

namespace Watermelon.SquadShooter
{
    [CreateAssetMenu(fileName = "Weapon Database", menuName = "Content/Weapon Database")]
    public class WeaponDatabase : ScriptableObject
    {
        [SerializeField] WeaponData[] weapons;
        public WeaponData[] Weapons => weapons;

        [SerializeField] RarityData[] raritySettings;
        public RarityData[] RaritySettings => raritySettings;

        public WeaponData GetWeapon(WeaponType type)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i].Type.Equals(type))
                    return weapons[i];
            }

            Debug.LogError("Weapon data of type: " + type + " is not found");
            return weapons[0];
        }

        public WeaponData GetWeaponByIndex(int index)
        {
            return weapons[index % weapons.Length];
        }

        public RarityData GetRarityData(Rarity rarity)
        {
            for (int i = 0; i < raritySettings.Length; i++)
            {
                if (raritySettings[i].Rarity.Equals(rarity))
                    return raritySettings[i];
            }

            Debug.LogError("Rarity data of type: " + rarity + " is not found");
            return raritySettings[0];
        }
    }
}