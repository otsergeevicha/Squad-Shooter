using UnityEngine;

namespace Watermelon
{
    public abstract class BaseNavigationArrowCase : IDistanceToggle
    {
        protected Vector3 targetPosition;

        protected Transform parentTransform;

        protected Transform arrowTransform;

        public bool IsTargetReached { get; protected set; }

        public bool IsShowing { get; protected set; }
        public bool IsVisible { get; protected set; }

        private float showingDistance = 12;
        public float ShowingDistance => showingDistance;

        public Vector3 DistancePointPosition => targetPosition;

        protected Transform fixedTargetTransform;

        protected bool isArrowFixed;
        public bool IsArrowFixed => isArrowFixed;

        public BaseNavigationArrowCase(Transform parentTransform, GameObject arrowObject, Vector3 targetPosition)
        {
            this.parentTransform = parentTransform;
            this.targetPosition = targetPosition;

            // Get transform refference and reset parent
            arrowTransform = arrowObject.transform;
            arrowTransform.SetParent(null);

            // Enable arrow object
            arrowObject.SetActive(true);

            IsShowing = true;
            IsVisible = false;

            // Reset target follow settings
            isArrowFixed = false;
            fixedTargetTransform = null;

            arrowTransform.gameObject.SetActive(true);

            // Add object to distance toggle
            DistanceToggle.AddObject(this);
        }

        public virtual void UpdateTargetPosition(Vector3 position)
        {
            targetPosition = position;
        }

        public void UpdateFixedPosition()
        {
            targetPosition = fixedTargetTransform.position;
        }

        public void FixArrowToTarget(Transform target)
        {
            fixedTargetTransform = target;
            isArrowFixed = true;
        }

        public void UnfixArrow()
        {
            isArrowFixed = false;
            fixedTargetTransform = null;
        }

        public abstract void LateUpdate();

        public abstract void DisableArrow();

        public abstract void PlayerEnteredZone();
        public abstract void PlayerLeavedZone();
    }
}