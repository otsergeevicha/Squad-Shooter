#pragma warning disable 0414

using UnityEngine;
using UnityEngine.EventSystems;

namespace Watermelon
{
    public class UITouchHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public static bool Enabled { get; set; }

        private static bool isPointerDown;

        public static float ClampedOffset { get; private set; }

        [SerializeField] float maxOffset;
        [SerializeField] float minOffset;

        [SerializeField] float snappingLerp;

        private static Vector2 center;
        private static Vector2 absolutePosition;

        public static Vector2 Offset { get => absolutePosition - center; set => absolutePosition = center + value; }

        private void Awake()
        {
            isPointerDown = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPointerDown = true;

            center = eventData.position;
            absolutePosition = eventData.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPointerDown = false;

            center = absolutePosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            absolutePosition = eventData.position;

            if (Offset.magnitude < minOffset) return;
            if (Offset.magnitude > maxOffset)
            {
                center = absolutePosition - Offset.normalized * maxOffset;
            }

            ClampedOffset = Mathf.Clamp01(Mathf.InverseLerp(minOffset, maxOffset, Offset.magnitude));
        }

        private void Update()
        {
            if (isPointerDown)
            {
                center = Vector2.Lerp(center, absolutePosition, snappingLerp * Time.deltaTime);

                ClampedOffset = Mathf.Clamp01(Mathf.InverseLerp(minOffset, maxOffset, Offset.magnitude));
            }
        }

        public static Vector3 GetInputDirection()
        {
            if (!isPointerDown) return Vector3.zero;

            if (ClampedOffset <= 0) return Vector3.zero;

            Vector3 prevPoint = center;
            Vector3 currentpoint = absolutePosition;

            prevPoint.z = 1;
            currentpoint.z = 1;

            Vector3 worldPrevPoint = Camera.main.ScreenToWorldPoint(prevPoint);
            Vector3 worldCurrentPoint = Camera.main.ScreenToWorldPoint(currentpoint);

            return (worldCurrentPoint - worldPrevPoint).normalized;
        }
    }
}