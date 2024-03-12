using Watermelon;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class CharacterSave : ISaveObject
    {
        public int UpgradeLevel = 0;

        public void Flush()
        {

        }
    }
}