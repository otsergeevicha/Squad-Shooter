using UnityEngine;

namespace Watermelon
{
    public class LineNavigationArrowCase : BaseNavigationArrowCase
    {
        private static readonly int MATERIAL_ALPHA_HASH = Shader.PropertyToID("_Alpha");

        private const float DEFAULT_ALPHA = 0.5f;
        private const float DEFAULT_LINE_Y = 1.0f;

        private LineRenderer arrowLineRenderer;

        private MaterialPropertyBlock arrowPropertyBlock;

        private TweenCase fadeTweenCase;

        private Vector3[] linePositions;

        public LineNavigationArrowCase(Transform parentTransform, GameObject arrowObject, Vector3 targetPosition) : base(parentTransform, arrowObject, targetPosition)
        {
            arrowTransform.rotation = Quaternion.LookRotation((targetPosition - parentTransform.position).SetY(0).normalized, Vector3.up);

            arrowLineRenderer = arrowObject.GetComponent<LineRenderer>();

            // Create arrow property block
            arrowPropertyBlock = new MaterialPropertyBlock();

            linePositions = new Vector3[2];

            RecalculateLinePositions();

            // Do scale animation
            fadeTweenCase = arrowLineRenderer.DOPropertyBlockFloat(MATERIAL_ALPHA_HASH, arrowPropertyBlock, DEFAULT_ALPHA, 0.4f);
        }

        private void RecalculateLinePositions()
        {
            linePositions[0].x = 0;
            linePositions[0].y = DEFAULT_LINE_Y;
            linePositions[0].z = 0;

            linePositions[1].x = 0;
            linePositions[1].y = DEFAULT_LINE_Y;
            linePositions[1].z = Vector3.Distance(targetPosition, parentTransform.position) - 2;

            arrowLineRenderer.SetPositions(linePositions);
        }

        public override void LateUpdate()
        {
            arrowTransform.SetPositionAndRotation(parentTransform.position, Quaternion.LookRotation((targetPosition - parentTransform.position).SetY(0).normalized));

            RecalculateLinePositions();
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

            arrowLineRenderer.DOPropertyBlockFloat(MATERIAL_ALPHA_HASH, arrowPropertyBlock, 0, 0.4f).OnComplete(delegate
            {
                arrowTransform.gameObject.SetActive(false);
            });
        }

        public override void PlayerEnteredZone()
        {
            // Hide arrow, player is near the target
            IsVisible = true;

            if (fadeTweenCase != null && fadeTweenCase.isActive) fadeTweenCase.Kill();

            // Do fade animation
            fadeTweenCase = arrowLineRenderer.DOPropertyBlockFloat(MATERIAL_ALPHA_HASH, arrowPropertyBlock, 0, 0.4f);
        }

        public override void PlayerLeavedZone()
        {
            // Show arrow, player is far from the target
            IsVisible = false;

            if (fadeTweenCase != null && fadeTweenCase.isActive) fadeTweenCase.Kill();

            // Do fade animation
            fadeTweenCase = arrowLineRenderer.DOPropertyBlockFloat(MATERIAL_ALPHA_HASH, arrowPropertyBlock, DEFAULT_ALPHA, 0.4f);
        }
    }
}