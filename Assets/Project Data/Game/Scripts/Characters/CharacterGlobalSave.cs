using Watermelon;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class CharacterGlobalSave : ISaveObject
    {
        public CharacterType SelectedCharacterType = CharacterType.Character_01;

        public void Flush()
        {

        }
    }
}