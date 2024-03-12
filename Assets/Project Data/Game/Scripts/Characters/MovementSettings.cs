using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class MovementSettings
    {
        public float RotationSpeed;

        [Space]
        public float MoveSpeed;
        public float Acceleration;

        [Space]
        public DuoFloat AnimationMultiplier;
    }
}