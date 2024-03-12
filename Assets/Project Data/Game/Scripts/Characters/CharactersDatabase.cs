using System.Linq;
using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    [CreateAssetMenu(fileName = "Character Database", menuName = "Content/Characters/Character Database")]
    public class CharactersDatabase : ScriptableObject
    {
        [SerializeField] Character[] characters;
        public Character[] Characters => characters;

        public void Initialise()
        {
            characters.OrderBy(c => c.RequiredLevel);

            for (int i = 0; i < characters.Length; i++)
            {
                characters[i].Initialise();
            }
        }

        public Character GetCharacter(CharacterType characterType)
        {
            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i].Type == characterType)
                    return characters[i];
            }

            return null;
        }

        public Character GetLastUnlockedCharacter()
        {
            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i].RequiredLevel > ExperienceController.CurrentLevel)
                {
                    return characters[Mathf.Clamp(i - 1, 0, characters.Length - 1)];
                }
            }

            return null;
        }

        public Character GetNextCharacterToUnlock()
        {
            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i].RequiredLevel > ExperienceController.CurrentLevel)
                {
                    return characters[i];
                }
            }

            return null;
        }
    }
}