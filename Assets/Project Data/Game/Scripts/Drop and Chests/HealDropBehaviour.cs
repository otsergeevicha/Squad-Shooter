namespace Watermelon.SquadShooter
{
    public class HealDropBehaviour : ItemDropBehaviour
    {
        public override bool IsPickable(CharacterBehaviour characterBehaviour)
        {
            return !characterBehaviour.FullHealth;
        }
    }
}