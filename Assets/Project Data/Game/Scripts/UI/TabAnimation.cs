using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class TabAnimation : TweenCase
    {
        public RectTransform tweenObject;

        public Vector2 startValue;
        public Vector2 resultValue;

        public TabAnimation(RectTransform tweenObject, Vector2 resultValue)
        {
            this.resultValue = resultValue;
            this.tweenObject = tweenObject;

            startValue = tweenObject.anchoredPosition;

            parentObject = tweenObject.gameObject;
        }

        public override bool Validate()
        {
            return parentObject != null;
        }

        public override void DefaultComplete()
        {
            tweenObject.anchoredPosition = resultValue;
        }

        public override void Invoke(float deltaTime)
        {
            tweenObject.anchoredPosition = Vector2.LerpUnclamped(startValue, resultValue, Interpolate(state));

            if (state >= 1.0f)
            {
                state = 0;
                isCompleted = false;

                Vector2 tempStartValue = startValue;

                startValue = resultValue;
                resultValue = tempStartValue;
            }
        }
    }
}