using UnityEngine;

namespace Watermelon
{
    public class NavigationArrowCase : BaseNavigationArrowCase
    {
        private Transform graphicsTransform;

        private TweenCase scaleTweenCase;

        public NavigationArrowCase(Transform parentTransform, GameObject arrowObject, Vector3 targetPosition) : base(parentTransform, arrowObject, targetPosition)
        {
            // Prepare arrow position and rotation
            arrowTransform.position = parentTransform.position;
            arrowTransform.rotation = Quaternion.LookRotation((targetPosition - parentTransform.position).SetY(0).normalized, Vector3.up);

            graphicsTransform = arrowTransform.GetChild(0);
            graphicsTransform.localScale = Vector3.zero;

            // Do scale animation
            scaleTweenCase = graphicsTransform.DOScale(1.0f, 0.4f).SetEasing(Ease.Type.SineIn);
        }

        public override void LateUpdate()
        {
            // Fix arrow transform to player position
            arrowTransform.position = parentTransform.position;

            // Rotate arrow to target
            arrowTransform.rotation = Quaternion.Lerp(arrowTransform.rotation, Quaternion.LookRotation((targetPosition - parentTransform.position).SetY(0).normalized, Vector3.up), 0.075f);
        }

        public override void DisableArrow()
        {
            if (IsTargetReached)
                return;

            IsTargetReached = true;

            IsShowing = false;
            IsVisible = false;

            // Remove object from distance toggle
            DistanceToggle.RemoveObject(this);

            graphicsTransform.DOScale(0.0f, 0.4f).SetEasing(Ease.Type.SineOut).OnComplete(delegate
            {
                arrowTransform.gameObject.SetActive(false);
            });
        }

        public override void PlayerEnteredZone()
        {
            // Hide arrow, player is near the target
            IsVisible = true;

            if (scaleTweenCase != null && scaleTweenCase.isActive) scaleTweenCase.Kill();

            scaleTweenCase = graphicsTransform.DOScale(0, 0.3f).SetEasing(Ease.Type.BackIn);
        }

        public override void PlayerLeavedZone()
        {
            // Show arrow, player is far from the target
            IsVisible = false;

            if (scaleTweenCase != null && scaleTweenCase.isActive) scaleTweenCase.Kill();

            graphicsTransform.transform.localScale = Vector3.zero;
            scaleTweenCase = graphicsTransform.DOScale(1, 0.3f).SetEasing(Ease.Type.BackOut);
        }
    }
}