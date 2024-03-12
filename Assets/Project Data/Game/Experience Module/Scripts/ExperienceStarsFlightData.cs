using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(menuName = "Content/Data/Stars Flight Data", fileName = "Stars Flight Data")]
    public class ExperienceStarsFlightData : ScriptableObject
    {
        [Header("First Stage (Linear)")]
        [SerializeField] AnimationCurve pathCurve1;
        [SerializeField] AnimationCurve starsScale1;

        public AnimationCurve PathCurve1 => pathCurve1;
        public AnimationCurve StarsScale1 => starsScale1;

        [Space]
        [SerializeField] DuoFloat firstStageDistance;
        [SerializeField] DuoFloat firstStageDuration;

        public float FirstStageDistance => firstStageDistance.Random();
        public float FirstStageDuration => firstStageDuration.Random();

        [Header("Second Stage (Bezier)")]
        [SerializeField] AnimationCurve pathCurve2;
        [SerializeField] AnimationCurve starsScale2;

        public AnimationCurve PathCurve2 => pathCurve2;
        public AnimationCurve StarsScale2 => starsScale2;

        [Space]
        [SerializeField] DuoFloat key1;
        [SerializeField] DuoVector3 key2;

        public float Key1 => key1.Random();
        public Vector2 Key2 => key2.Random();

        [Space]
        [SerializeField] DuoFloat secondStageDuration;

        public float SecondStageDuration => secondStageDuration.Random();
    }
}
