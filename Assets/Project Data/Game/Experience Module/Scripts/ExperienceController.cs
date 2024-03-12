using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class ExperienceController : MonoBehaviour
    {
        private static readonly int FLOATING_TEXT_HASH = FloatingTextController.GetHash("Stars");
        private static readonly int SAVE_HASH = "Experience".GetHashCode();

        [SerializeField] ExperienceDatabase database;

        private static ExperienceController instance;

        private ExperienceSave save;
        private ExperienceUIController expUI;

        public static int CurrentLevel
        {
            get => instance.save.CurrentLevel;
            private set => instance.save.CurrentLevel = value;
        }

        public static int ExperiencePoints
        {
            get => instance.save.CurrentExperiencePoints;
            private set => instance.save.CurrentExperiencePoints = value;
        }

        public ExperienceLevelData CurrentLevelData => database.GetDataForLevel(CurrentLevel);
        public ExperienceLevelData NextLevelData => database.GetDataForLevel(CurrentLevel + 1);

        public static event SimpleCallback OnExperienceGained;
        public static event SimpleCallback OnLevelIncreased;

        private void Awake()
        {
            instance = this;
        }

        public void Initialise()
        {
            save = SaveController.GetSaveObject<ExperienceSave>(SAVE_HASH);

            database.Init();

            expUI = UIController.GetPage<UIMainMenu>().ExperienceUIController;
            expUI.Init(this);
        }

        public static void GainXPPoints(int amount)
        {
            instance.GainExperience(amount);
        }

        public void GainExperience(int amount)
        {
            ExperiencePoints = ExperiencePoints + amount;

            FloatingTextController.SpawnFloatingText(FLOATING_TEXT_HASH, string.Format("+{0}", amount), CharacterBehaviour.Transform.position + new Vector3(3, 6, 0), Quaternion.identity, 1f);

            expUI.PlayXpGainedAnimation(amount, CharacterBehaviour.Transform.position, () =>
            {
                expUI.UpdateUI(false);
            });

            // new level reached
            if (ExperiencePoints >= NextLevelData.ExperienceRequired)
            {
                // new level reached
                CurrentLevel++;

                OnLevelIncreased?.Invoke();
            };

            OnExperienceGained?.Invoke();
        }

        public static int GetXpPointsRequiredForLevel(int level)
        {
            return instance.database.GetDataForLevel(level).ExperienceRequired;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("[Experience controller] Exp Amount: " + ExperiencePoints + "  level: " + CurrentLevel);
            }
        }

        #region Development

        public static void SetLevelDev(int level)
        {
            CurrentLevel = level;
            ExperiencePoints = instance.database.GetDataForLevel(level).ExperienceRequired;
            //instance.expUI.UpdateUI(true);
            OnLevelIncreased?.Invoke();
        }

        #endregion
    }
}
