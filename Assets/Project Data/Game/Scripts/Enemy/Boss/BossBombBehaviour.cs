using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    public class BossBombBehaviour : MonoBehaviour
    {
        private readonly int PARTICLE_HIT_HASH = ParticlesController.GetHash("Boss Bomb Hit");
        private readonly int PARTICLE_EXPLOSION_HASH = ParticlesController.GetHash("Boss Bomb Explosion");
        private readonly int PARTICLE_EXPLOSION_RADIUS_HASH = ParticlesController.GetHash("Boss Bomb Radius");

        private bool isPlaced;

        private float duration;
        private float damage;
        private float radius;

        private BossBomberBehaviour bossEnemyBehaviour;

        public void Initialise(BossBomberBehaviour bossEnemyBehaviour, float duration, float damage, float radius)
        {
            this.bossEnemyBehaviour = bossEnemyBehaviour;
            this.duration = duration;
            this.damage = damage;
            this.radius = radius;

            isPlaced = false;

            transform.localScale = Vector3.one;
            transform.rotation = Random.rotation;
        }

        private void Update()
        {
            if (!isPlaced)
            {
                transform.Rotate(transform.right * Time.deltaTime * 50, Space.Self);
            }
        }

        public void OnPlaced()
        {
            isPlaced = true;

            ParticlesController.PlayParticle(PARTICLE_HIT_HASH).SetPosition(transform.position);
            ParticlesController.PlayParticle(PARTICLE_EXPLOSION_RADIUS_HASH).SetPosition(transform.position).SetDuration(duration);

            transform.DOScale(2.0f, duration).SetEasing(Ease.Type.CubicIn).OnComplete(delegate
            {
                bool playerHitted = false;

                Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
                for (int i = 0; i < hitColliders.Length; i++)
                {
                    if (hitColliders[i].gameObject.layer == PhysicsHelper.LAYER_PLAYER)
                    {
                        CharacterBehaviour characterBehaviour = hitColliders[i].GetComponent<CharacterBehaviour>();
                        if (characterBehaviour != null)
                        {
                            // Deal damage to player
                            characterBehaviour.TakeDamage(damage);

                            characterBehaviour.MainCameraCase.Shake(0.04f, 0.04f, 0.3f, 0.8f);

                            playerHitted = true;
                        }
                    }
                }

                ParticlesController.PlayParticle(PARTICLE_EXPLOSION_HASH).SetPosition(transform.position);
                AudioController.PlaySound(AudioController.Sounds.explode);

                if (bossEnemyBehaviour != null)
                    bossEnemyBehaviour.OnBombExploded(this, playerHitted);

                gameObject.SetActive(false);
            });
        }
    }
}