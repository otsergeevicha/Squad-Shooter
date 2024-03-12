namespace Watermelon
{
    public interface ITutorial
    {
        public const string SAVE_IDENTIFIER = "TUTORIAL:{0}";

        public TutorialID TutorialID { get; }
        
        public bool IsActive { get; }
        public bool IsFinished { get; }

        public bool IsInitialised { get; }

        public int Progress { get; }

        public void Initialise();
        public void StartTutorial();
        public void FinishTutorial();
        public void Unload();
    }
}