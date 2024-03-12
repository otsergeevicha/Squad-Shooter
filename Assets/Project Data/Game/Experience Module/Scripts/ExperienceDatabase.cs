using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Experience Database", menuName = "Content/Experience Database")]
    public class ExperienceDatabase : ScriptableObject
    {
        [SerializeField] List<ExperienceLevelData> experienceData;
        public List<ExperienceLevelData> ExperienceData => experienceData;

        public void Init()
        {
            for (int i = 0; i < experienceData.Count; i++)
            {
                experienceData[i].SetLevel(i + 1);
            }
        }

        public ExperienceLevelData GetDataForLevel(int level)
        {
            return experienceData[Mathf.Clamp(level - 1, 0, experienceData.Count - 1)];
        }
    }

    [System.Serializable]
    public class WorldExperienceData
    {
        [SerializeField] int worldNumber;
        public int WorldNumber => worldNumber;

        [SerializeField] int maxExpLevel;
        public int MaxExpLevel => maxExpLevel;
    }
}
