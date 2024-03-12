using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class ExperienceSave : ISaveObject
    {
        [SerializeField] int currentLevel = 1;
        public int CurrentLevel { get => currentLevel; set => currentLevel = value; }

        [SerializeField] int currentExperiencePoints;
        public int CurrentExperiencePoints { get => currentExperiencePoints; set => currentExperiencePoints = value; }

        public void Flush()
        {
            
        }
    }
}
