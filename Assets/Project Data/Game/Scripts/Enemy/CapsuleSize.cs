using UnityEngine;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public struct CapsuleSize
    {
        public Vector3 center;
        public float radius;
        public float height;

        public void Apply(CapsuleCollider capsuleCollider)
        {
            capsuleCollider.center = center;
            capsuleCollider.radius = radius;
            capsuleCollider.height = height;
        }

        public void DrawGizmo(Transform transform, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawWireCube(transform.position + center, new Vector3(radius * 2, height, radius * 2));
        }
    }
}