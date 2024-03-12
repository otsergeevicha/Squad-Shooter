using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class EnemyBaseBulletBehavior : MonoBehaviour
    {
        [SerializeField] TrailRenderer trailRenderer;

        protected float damage;
        protected float speed;

        protected float selfDestroyDistance;
        protected float distanceTraveled = 0;

        protected TweenCase disableTweenCase;

        public virtual void Initialise(float damage, float speed, float selfDestroyDistance)
        {
            this.damage = damage;
            this.speed = speed;

            this.selfDestroyDistance = selfDestroyDistance;
            distanceTraveled = 0;

            trailRenderer.Clear();
            float time = trailRenderer.time;
            trailRenderer.time = 0;

            gameObject.SetActive(true);
            Tween.NextFrame(() =>
            {
                trailRenderer.Clear();
                trailRenderer.gameObject.SetActive(true);
                trailRenderer.Clear();

                trailRenderer.time = time;
            });
        }

        protected virtual void FixedUpdate()
        {
            transform.position += transform.forward * speed * Time.fixedDeltaTime;

            if (selfDestroyDistance != -1)
            {
                distanceTraveled += speed * Time.fixedDeltaTime;

                if (distanceTraveled >= selfDestroyDistance)
                {
                    SelfDestroy();
                }
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
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
            else if (other.gameObject.layer == PhysicsHelper.LAYER_OBSTACLE)
            {
                SelfDestroy();
            }
        }

        public void SelfDestroy()
        {
            // Disable bullet
            trailRenderer.Clear();
            gameObject.SetActive(false);
            trailRenderer.gameObject.SetActive(false);
        }
    }
}