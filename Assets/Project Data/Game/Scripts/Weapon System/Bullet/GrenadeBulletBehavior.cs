using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class GrenadeBulletBehavior : BaseBulletBehavior
    {
        private readonly static int SPLASH_PARTICLE_HASH = ParticlesController.GetHash("Grenade Hit");
        private readonly static int WALL_SPLASH_PARTICLE_HASH = ParticlesController.GetHash("Grenade Wall Hit");

        [SerializeField] TrailRenderer trailRenderer;
        [SerializeField] Transform graphicsTransform;

        private float explosionRadius;
        private DuoInt damageValue;
        private CharacterBehaviour characterBehaviour;

        private TweenCase movementTween;

        private Vector3 position;
        private Vector3 prevPosition;

        public void Initialise(DuoInt damage, float speed, BaseEnemyBehavior currentTarget, float autoDisableTime, bool autoDisableOnHit, float shootingRadius, CharacterBehaviour characterBehaviour, DuoFloat bulletHeight, float explosionRadius)
        {
            base.Initialise(0f, speed, currentTarget, autoDisableTime, autoDisableOnHit);

            this.explosionRadius = explosionRadius;
            this.characterBehaviour = characterBehaviour;

            Vector3 targetPosition = currentTarget.transform.position + new Vector3(Random.Range(-4.0f, 4.0f), 0, Random.Range(-4.0f, 4.0f));

            float distanceMultiplier = Mathf.InverseLerp(0, shootingRadius, Vector3.Distance(characterBehaviour.transform.position, targetPosition));
            float bulletFlyTime = 1 / speed;

            damageValue = damage;

            movementTween = transform.DOBezierMove(targetPosition, Mathf.Lerp(bulletHeight.firstValue, bulletHeight.secondValue, distanceMultiplier), 0, bulletFlyTime).OnComplete(delegate
            {
                OnEnemyHitted(null);
            });

            Tween.DelayedCall(bulletFlyTime * 0.8f, () =>
            {
                AudioController.PlaySound(AudioController.Sounds.shotLavagun, 0.6f);
            });
        }

        private void Update()
        {
            prevPosition = position;
            position = transform.position;
        }

        protected override void FixedUpdate()
        {

        }

        protected override void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior)
        {
            if (movementTween != null && !movementTween.isCompleted)
                movementTween.Kill();

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

            for (int i = 0; i < hitColliders.Length; i++)
            {
                if (hitColliders[i].gameObject.layer == PhysicsHelper.LAYER_ENEMY)
                {
                    BaseEnemyBehavior enemy = hitColliders[i].GetComponent<BaseEnemyBehavior>();
                    if (enemy != null && !enemy.IsDead)
                    {
                        float explosionDamageMultiplier = 1.0f - Mathf.InverseLerp(0, explosionRadius, Vector3.Distance(transform.position, hitColliders[i].transform.position));

                        // Deal damage to enemy
                        enemy.TakeDamage(CharacterBehaviour.NoDamage ? 0 : damageValue.Lerp(explosionDamageMultiplier), transform.position, (transform.position - prevPosition).normalized);

                        characterBehaviour.MainCameraCase.Shake(0.04f, 0.04f, 0.3f, 0.8f);
                    }
                }
            }

            AudioController.PlaySound(AudioController.Sounds.explode);

            // Disable projectile
            gameObject.SetActive(false);

            // Spawn splash particle
            ParticlesController.PlayParticle(SPLASH_PARTICLE_HASH).SetPosition(transform.position);

            if (trailRenderer != null)
                trailRenderer.Clear();
        }

        protected override void OnObstacleHitted()
        {
            base.OnObstacleHitted();
            if (trailRenderer != null)
                trailRenderer.Clear();

            ParticlesController.PlayParticle(WALL_SPLASH_PARTICLE_HASH).SetPosition(transform.position);
        }
    }
}