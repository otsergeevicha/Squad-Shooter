using System.Collections.Generic;
using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    public class BossSniperBulletBehavior : EnemyBaseBulletBehavior
    {
        private readonly int REFLECTIONS_NUMBER = 4;
        private static readonly int PARTICLE_WAll_HIT_HASH = ParticlesController.GetHash("Minigun Wall Hit");

        [SerializeField] LayerMask collisionLayer;

        private int nextPointId = 0;
        private Vector3 NextPoint => points[nextPointId];

        private List<Vector3> points;

        public void Initialize(float damage, float speed, float selfDestroyDistance, List<Vector3> points)
        {
            Initialise(damage, speed, selfDestroyDistance);

            nextPointId = 0;

            this.points = new List<Vector3>(points.ToArray());

            if (points.Count < REFLECTIONS_NUMBER)
            {
                AddPoints();
            }
        }

        protected override void FixedUpdate()
        {
            var frameDistance = speed * Time.fixedDeltaTime;
            var distanceToPoint = (NextPoint - transform.position).magnitude;

            if (frameDistance > distanceToPoint)
            {
                transform.position = NextPoint;

                nextPointId++;

                if (nextPointId >= points.Count)
                {
                    SelfDestroy();
                }
                else
                {
                    ParticlesController.PlayParticle(PARTICLE_WAll_HIT_HASH).SetPosition(transform.position);
                    transform.forward = (NextPoint - transform.position).normalized;
                }
            }
            else
            {
                var direction = (NextPoint - transform.position).normalized;

                transform.position += direction * frameDistance;
            }
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsHelper.LAYER_PLAYER)
            {
                CharacterBehaviour characterBehaviour = other.GetComponent<CharacterBehaviour>();
                if (characterBehaviour != null)
                {
                    // Deal damage to enemy
                    characterBehaviour.TakeDamage(damage);

                    SelfDestroy();
                }
            }
        }

        private void AddPoints()
        {
            Vector3 startPos;
            Vector3 direction;

            if (points.Count == 0)
            {
                startPos = transform.position;
                direction = transform.forward;
            }
            else
            {
                startPos = points.Last();
                if (points.Count > 1)
                {
                    direction = (points[^1] - points[^2]).normalized;
                }
                else
                {
                    direction = (startPos - transform.position).normalized;
                }
            }

            for (int i = points.Count; i < REFLECTIONS_NUMBER; i++)
            {

                if (Physics.Raycast(startPos, direction, out var hitInfo, 300f, collisionLayer))
                {
                    startPos = hitInfo.point - direction * 0.2f;
                    direction = Vector3.Reflect(direction, -hitInfo.normal);

                    points.Add(startPos);
                }
                else
                {
                    var hitPoint = startPos + direction * 300;

                    points.Add(hitPoint);

                    break;
                }
            }
        }
    }
}