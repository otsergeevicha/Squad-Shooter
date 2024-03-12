using UnityEngine;

namespace Watermelon
{
    public abstract class BaseTutorial : MonoBehaviour, ITutorial
    {
        [SerializeField] 
        protected TutorialID tutorialId;
        public TutorialID TutorialID => tutorialId;

        public abstract bool IsActive { get; }
        public abstract bool IsFinished { get; }
        public abstract int Progress { get; }

        protected bool isInitialised;
        public bool IsInitialised => isInitialised;

        private void OnEnable()
        {
            TutorialController.RegisterTutorial(this);
        }

        private void OnDisable()
        {
            TutorialController.RemoveTutorial(this);
        }

        public abstract void Initialise();

        public abstract void StartTutorial();
        public abstract void FinishTutorial();

        public abstract void Unload();
    }
}