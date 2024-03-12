using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class ExperienceLevelData
    {
        [SerializeField] int experienceRequired;
        public int ExperienceRequired => experienceRequired;

        public int Level { get; private set; } 

        public void SetLevel(int level)
        {
            Level = level;
        }

        public void SetExperienceRequred(int amount)
        {
            experienceRequired = amount;
        }
    }
}
