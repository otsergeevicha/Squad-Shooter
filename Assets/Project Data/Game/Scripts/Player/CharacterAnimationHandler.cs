using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class CharacterAnimationHandler : MonoBehaviour
    {
        private CharacterBehaviour characterBehaviour;

        public void Inititalise(CharacterBehaviour characterBehaviour)
        {
            this.characterBehaviour = characterBehaviour;
        }

        public void JumpEnding()
        {
            characterBehaviour.SpawnWeapon();
        }
    }
}