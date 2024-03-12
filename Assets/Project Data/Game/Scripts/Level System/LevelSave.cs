namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class LevelSave : ISaveObject
    {
        public int WorldIndex;
        public int LevelIndex;
        public int LastCompletedLevelCoinBalance;

        public LevelSave()
        {
            WorldIndex = 0;
            LevelIndex = 0;
        }

        public void Flush()
        {

        }
    }
}