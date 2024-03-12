using UnityEngine;

namespace Watermelon
{
    public interface IDistanceToggle
    {
        public bool IsShowing { get; }
        public bool IsVisible { get; }

        public float ShowingDistance { get; }
        public Vector3 DistancePointPosition { get; }

        public void PlayerEnteredZone();
        public void PlayerLeavedZone();
    }
}