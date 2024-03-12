using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    [CreateAssetMenu(fileName = "Level Tutorial Behaviour", menuName = "Content/New Level/Behaviours/Tutorial")]
    public sealed class TutorialLevelSpecialBehaviour : LevelSpecialBehaviour, ITutorial
    {
        [SerializeField] TutorialID tutorialId;
        public TutorialID TutorialID => tutorialId;

        [System.NonSerialized]
        private bool isInitialised;
        public bool IsInitialised => isInitialised;

        private Transform finishPointTransform;
        private TutorialLabelBehaviour tutorialLabelBehaviour;

        public bool IsActive => saveData.isActive;
        public bool IsFinished => saveData.isFinished;
        public int Progress => saveData.progress;

        private TutorialBaseSave saveData;

        private LineNavigationArrowCase arrowCase;

        private CharacterBehaviour characterBehaviour;
        private BaseEnemyBehavior enemyBehavior;
        
        public void Initialise()
        {
            isInitialised = true;

            // Load save file
            saveData = SaveController.GetSaveObject<TutorialBaseSave>(string.Format(ITutorial.SAVE_IDENTIFIER, tutorialId.ToString()));
        }

        public void StartTutorial()
        {
            if (saveData.isFinished) return;

            // Activate tutorial
            saveData.isActive = true;

            characterBehaviour = CharacterBehaviour.GetBehaviour();

            // Force game start
            LevelController.OnGameStarted(immediately: true);
        }

        private void OnEnemyDied(BaseEnemyBehavior enemy)
        {
            if (enemy == enemyBehavior)
            {
                BaseEnemyBehavior.OnDiedEvent -= OnEnemyDied;

                if (arrowCase != null)
                {
                    arrowCase.DisableArrow();
                    arrowCase = null;
                }

                tutorialLabelBehaviour.Disable();

                arrowCase = NavigationArrowController.RegisterLineArrow(characterBehaviour.transform, finishPointTransform.position);

                LevelController.ActivateExit();

                LevelController.OnPlayerExitLevelEvent += OnPlayerExitLevel;
            }
        }

        private void OnPlayerExitLevel()
        {
            LevelController.OnPlayerExitLevelEvent -= OnPlayerExitLevel;

            if (arrowCase != null)
            {
                arrowCase.DisableArrow();
                arrowCase = null;
            }

            FinishTutorial();
        }

        public void FinishTutorial()
        {
            saveData.isFinished = true;

            isInitialised = false;
        }

        public void Unload()
        {
            if (arrowCase != null)
                arrowCase.DisableArrow();
        }

        public override void OnLevelCompleted()
        {

        }

        public override void OnLevelFailed()
        {

        }

        public override void OnLevelInitialised()
        {
            isInitialised = false;

            TutorialController.ActivateTutorial(this);
        }

        public override void OnLevelLoaded()
        {
            finishPointTransform = ActiveRoom.ExitPointBehaviour.transform;

            if(isInitialised)
                StartTutorial();
        }

        public override void OnLevelStarted()
        {
            if (saveData.isFinished) return;

            LevelController.EnableManualExitActivation();

            enemyBehavior = ActiveRoom.Enemies[0];

            arrowCase = NavigationArrowController.RegisterLineArrow(characterBehaviour.transform, enemyBehavior.transform.position);
            arrowCase.FixArrowToTarget(enemyBehavior.transform);

            tutorialLabelBehaviour = TutorialController.CreateTutorialLabel("KILL THE ENEMY", enemyBehavior.transform, new Vector3(0, 20.0f, 0));

            BaseEnemyBehavior.OnDiedEvent += OnEnemyDied;
        }

        public override void OnLevelUnloaded()
        {

        }

        public override void OnRoomEntered()
        {

        }

        public override void OnRoomLeaved()
        {

        }
    }
}
