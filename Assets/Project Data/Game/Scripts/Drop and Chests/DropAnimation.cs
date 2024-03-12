using UnityEngine;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class DropAnimation
    {
        [SerializeField] DropFallingStyle fallStyle;
        public DropFallingStyle FallStyle => fallStyle;

        [SerializeField] AnimationCurve fallAnimationCurve;
        public AnimationCurve FallAnimationCurve => fallAnimationCurve;

        [SerializeField] AnimationCurve fallYAnimationCurve;
        public AnimationCurve FallYAnimationCurve => fallYAnimationCurve;

        [SerializeField] float fallTime;
        public float FallTime => fallTime;

        [SerializeField] float offsetY;
        public float OffsetY => offsetY;

        [SerializeField] float radius;
        public float Radius => radius;
    }
}