using UnityEngine;
using Watermelon.LevelSystem;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public sealed class FirstLevelTutorial : ITutorial
    {
        private TutorialID tutorialId;
        public TutorialID TutorialID => tutorialId;

        private bool isInitialised;
        public bool IsInitialised => isInitialised;

        [SerializeField] Transform finishPointTransform;
        [SerializeField] TutorialLabelBehaviour tutorialLabelBehaviour;

        public bool IsActive => saveData.isActive;
        public bool IsFinished => saveData.isFinished;
        public int Progress => saveData.progress;

        private TutorialBaseSave saveData;

        private LineNavigationArrowCase arrowCase;

        private CharacterBehaviour characterBehaviour;
        private BaseEnemyBehavior enemyBehavior;

        private bool isCompleted;

        public void Initialise()
        {
            if (isInitialised)
                return;

            isInitialised = true;

            // Load save file
            saveData = SaveController.GetSaveObject<TutorialBaseSave>(string.Format(ITutorial.SAVE_IDENTIFIER, tutorialId.ToString()));
        }

        public void ForceGameStart()
        {
            // Force game start
            LevelController.OnGameStarted(immediately: true);
        }

        public void StartTutorial()
        {
            // Activate tutorial
            saveData.isActive = true;

            characterBehaviour = CharacterBehaviour.GetBehaviour();

            if(!isCompleted)
            {
                LevelController.EnableManualExitActivation();

                enemyBehavior = ActiveRoom.Enemies[0];

                arrowCase = NavigationArrowController.RegisterLineArrow(characterBehaviour.transform, enemyBehavior.transform.position);
                arrowCase.FixArrowToTarget(enemyBehavior.transform);

                tutorialLabelBehaviour.Activate("KILL THE ENEMY", enemyBehavior.transform, new Vector3(0, 20.0f, 0));

                BaseEnemyBehavior.OnDiedEvent += OnEnemyDied;
            }
        }

        private void OnEnemyDied(BaseEnemyBehavior enemy)
        {
            if (enemy == enemyBehavior)
            {
                BaseEnemyBehavior.OnDiedEvent -= OnEnemyDied;

                tutorialLabelBehaviour.Disable();

                if (arrowCase != null)
                {
                    arrowCase.DisableArrow();
                    arrowCase = null;
                }

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

            isCompleted = true;
        }

        public void FinishTutorial()
        {
            saveData.isFinished = true;
        }

        public void Unload()
        {
            if (arrowCase != null)
                arrowCase.DisableArrow();
        }
    }
}