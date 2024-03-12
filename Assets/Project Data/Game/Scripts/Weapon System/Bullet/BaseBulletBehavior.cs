using UnityEngine;

namespace Watermelon.SquadShooter
{
    public abstract class BaseBulletBehavior : MonoBehaviour
    {
        protected float damage;
        protected float speed;
        private bool autoDisableOnHit;

        private TweenCase disableTweenCase;

        protected BaseEnemyBehavior currentTarget;

        public virtual void Initialise(float damage, float speed, BaseEnemyBehavior currentTarget, float autoDisableTime, bool autoDisableOnHit = true)
        {
            this.damage = damage;
            this.speed = speed;
            this.autoDisableOnHit = autoDisableOnHit;

            this.currentTarget = currentTarget;

            if (autoDisableTime > 0)
            {
                disableTweenCase = Tween.DelayedCall(autoDisableTime, delegate
                {
                    // Disable bullet
                    gameObject.SetActive(false);
                });
            }
        }

        protected virtual void FixedUpdate()
        {
            if (speed != 0)
                transform.position += transform.forward * speed * Time.fixedDeltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
            {
                BaseEnemyBehavior baseEnemyBehavior = other.GetComponent<BaseEnemyBehavior>();
                if (baseEnemyBehavior != null)
                {
                    if (!baseEnemyBehavior.IsDead)
                    {
                        if (disableTweenCase != null && !disableTweenCase.isCompleted)
                            disableTweenCase.Kill();

                        // Disable bullet
                        if (autoDisableOnHit)
                            gameObject.SetActive(false);

                        // Deal damage to enemy
                        baseEnemyBehavior.TakeDamage(CharacterBehaviour.NoDamage ? 0 : damage, transform.position, transform.forward);

                        // Call hit callback
                        OnEnemyHitted(baseEnemyBehavior);
                    }
                }
            }
            else
            {
                OnObstacleHitted();
            }
        }

        private void OnDisable()
        {
            if (disableTweenCase != null && !disableTweenCase.isCompleted)
                disableTweenCase.Kill();
        }

        private void OnDestroy()
        {
            if (disableTweenCase != null && !disableTweenCase.isCompleted)
                disableTweenCase.Kill();
        }

        protected abstract void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior);

        protected virtual void OnObstacleHitted()
        {
            if (disableTweenCase != null && !disableTweenCase.isCompleted)
                disableTweenCase.Kill();

            gameObject.SetActive(false);
        }
    }
}