using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class Character
    {
        [SerializeField] CharacterType type;
        public CharacterType Type => type;

        [SerializeField] string name;
        public string Name => name;

        [SerializeField] int requiredLevel;
        public int RequiredLevel => requiredLevel;

        [SerializeField] Sprite lockedSprite;
        public Sprite LockedSprite => lockedSprite;

        [SerializeField] CharacterStageData[] stages;
        public CharacterStageData[] Stages => stages;

        [SerializeField] CharacterUpgrade[] upgrades;
        public CharacterUpgrade[] Upgrades => upgrades;

        private CharacterSave save;
        public CharacterSave Save => save;

        public void Initialise()
        {
            save = SaveController.GetSaveObject<CharacterSave>("Character" + type.ToString());

#if UNITY_EDITOR
            if (stages.IsNullOrEmpty())
                Debug.LogError(string.Format("[Character]: Character with type {0} has no stages!", type));
#endif
        }

        public CharacterStageData GetCurrentStage()
        {
            for (int i = save.UpgradeLevel; i >= 0; i--)
            {
                if (upgrades[i].ChangeStage)
                    return stages[upgrades[i].StageIndex];
            }

            return stages[0];
        }

        public int GetCurrentStageIndex()
        {
            for (int i = save.UpgradeLevel; i >= 0; i--)
            {
                if (upgrades[i].ChangeStage)
                    return i;
            }

            return 0;
        }

        public CharacterUpgrade GetCurrentUpgrade()
        {
            return upgrades[save.UpgradeLevel];
        }

        public int GetCurrentUpgradeIndex()
        {
            return save.UpgradeLevel;
        }

        public bool IsMaxUpgrade()
        {
            return !upgrades.IsInRange(save.UpgradeLevel + 1);
        }

        public void UpgradeCharacter()
        {
            if (upgrades.IsInRange(save.UpgradeLevel + 1))
            {
                save.UpgradeLevel += 1;

                CharactersController.OnCharacterUpgraded(this);
            }
        }

        public bool IsSelected()
        {
            return CharactersController.SelectedCharacter.type == type;
        }

        public bool IsUnlocked()
        {
            return ExperienceController.CurrentLevel >= requiredLevel;
        }
    }
}