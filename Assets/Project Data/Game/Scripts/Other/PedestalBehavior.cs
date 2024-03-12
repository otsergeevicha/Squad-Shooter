using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class PedestalBehavior : MonoBehaviour
    {
        [SerializeField] Transform playerPosition;

        public void PlaceCharacter()
        {
            var character = CharacterBehaviour.GetBehaviour();
            character.transform.position = playerPosition.position;
        }
    }
}