namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class CharacterDynamicAnimation
    {
        public CharacterPanelUI CharacterPanel;

        public float Delay;

        public SimpleCallback OnAnimationStarted;

        public CharacterDynamicAnimation(CharacterPanelUI characterPanel, float delay, SimpleCallback onAnimationStarted)
        {
            CharacterPanel = characterPanel;
            Delay = delay;

            OnAnimationStarted = onAnimationStarted;
        }
    }
}