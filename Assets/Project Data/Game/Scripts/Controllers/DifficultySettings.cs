using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class DifficultySettings
    {
        [SerializeField] Difficulty difficulty;
        [SerializeField] float healthMult;
        [SerializeField] float damageMult;
        [SerializeField] float restoredHpMult;

        public Difficulty Difficulty => difficulty;
        public float HealthMult => healthMult;
        public float DamageMult => damageMult;
        public float RestoredHpMult => restoredHpMult;
    }

    public enum Difficulty
    {
        Easy = 0,
        Normal = 1,
        Medium = 2,
        Hard = 3,
        Default = 4
    }
}
