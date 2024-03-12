using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    [System.Serializable]
    public class LevelData
    {
        [SerializeField] LevelType type;
        public LevelType Type => type;

        [SerializeField] RoomData[] rooms;
        public RoomData[] Rooms => rooms;

        [Space]
        [SerializeField] int xpAmount;
        public int XPAmount => xpAmount;

        [SerializeField] int requiredUpg;
        public int RequiredUpg => requiredUpg;

        [SerializeField] int enemiesLevel;
        public int EnemiesLevel => enemiesLevel;

        [SerializeField] bool hasCharacterSuggestion;
        public bool HasCharacterSuggestion => hasCharacterSuggestion;

        [SerializeField, Range(0.0f, 1.0f)] float healSpawnPercent = 0.5f;
        public float HealSpawnPercent => healSpawnPercent;

        [SerializeField] List<DropData> dropData = new List<DropData>();
        public List<DropData> DropData => dropData;

        [SerializeField] LevelSpecialBehaviour[] specialBehaviours;
        public LevelSpecialBehaviour[] SpecialBehaviours => specialBehaviours;

        private World world;
        public World World => world;

        public void Initialise(World world)
        {
            this.world = world;
        }

        #region Special Behaviours callbacks
        public void OnLevelInitialised()
        {
            for (int i = 0; i < specialBehaviours.Length; i++)
            {
                specialBehaviours[i].OnLevelInitialised();
            }
        }

        public void OnLevelLoaded()
        {
            for (int i = 0; i < specialBehaviours.Length; i++)
            {
                specialBehaviours[i].OnLevelLoaded();
            }
        }

        public void OnLevelUnloaded()
        {
            for (int i = 0; i < specialBehaviours.Length; i++)
            {
                specialBehaviours[i].OnLevelUnloaded();
            }
        }

        public void OnLevelStarted()
        {
            for (int i = 0; i < specialBehaviours.Length; i++)
            {
                specialBehaviours[i].OnLevelStarted();
            }
        }

        public void OnLevelFailed()
        {
            for (int i = 0; i < specialBehaviours.Length; i++)
            {
                specialBehaviours[i].OnLevelFailed();
            }
        }

        public void OnLevelCompleted()
        {
            for (int i = 0; i < specialBehaviours.Length; i++)
            {
                specialBehaviours[i].OnLevelCompleted();
            }
        }

        public void OnRoomEntered()
        {
            for (int i = 0; i < specialBehaviours.Length; i++)
            {
                specialBehaviours[i].OnRoomEntered();
            }
        }

        public void OnRoomLeaved()
        {
            for (int i = 0; i < specialBehaviours.Length; i++)
            {
                specialBehaviours[i].OnRoomLeaved();
            }
        }
        #endregion

        public int GetChestsAmount(bool includeRewarded = false)
        {
            int finalAmount = 0;

            for (int i = 0; i < rooms.Length; i++)
            {
                var room = rooms[i];
                if (room.ChestEntities != null)
                {
                    for (int j = 0; j < room.ChestEntities.Length; j++)
                    {
                        var chest = room.ChestEntities[j];

                        if (chest.IsInited && (includeRewarded || chest.ChestType != LevelChestType.Rewarded))
                        {
                            finalAmount++;
                        }
                    }
                }
            }

            return finalAmount;
        }

        public int GetCoinsReward()
        {
            for (int i = 0; i < dropData.Count; i++)
            {
                if (dropData[i].dropType == DropableItemType.Currency && dropData[i].currencyType == CurrencyType.Coin)
                    return dropData[i].amount;
            }

            return 0;
        }

        public List<WeaponType> GetCardsReward()
        {
            List<WeaponType> result = new List<WeaponType>();

            for (int i = 0; i < dropData.Count; i++)
            {
                if (dropData[i].dropType == DropableItemType.WeaponCard)
                {
                    bool isWeaponUnlocked = WeaponsController.IsWeaponUnlocked(dropData[i].cardType);

                    if (!isWeaponUnlocked)
                    {
                        for (int j = 0; j < dropData[i].amount; j++)
                        {
                            result.Add(dropData[i].cardType);
                        }
                    }
                }
            }

            return result;
        }
    }
}