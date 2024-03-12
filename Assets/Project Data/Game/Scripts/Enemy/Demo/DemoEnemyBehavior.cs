using System.Collections.Generic;
using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class DemoEnemyBehavior : BaseEnemyBehavior
    {
        private static readonly int ANIMATOR_ATTACK_HASH = Animator.StringToHash("Attack");

        [SerializeField] float explosionRadius;
        [SerializeField] GameObject explosionCircle;
        [SerializeField] Transform bombBone;
        [SerializeField] GameObject bombObj;
        [SerializeField] GameObject fuseObj;

        private TweenCase explosionRadiusScaleCase;
        private bool exploded = false;

        private int explosionParticleHash;
        private int explosionDecalParticleHash;

        protected override void Awake()
        {
            base.Awake();

            explosionParticleHash = ParticlesController.GetHash("Bomber Explosion");
            explosionDecalParticleHash = ParticlesController.GetHash("Bomber Explosion Decal");

            CanPursue = true;
        }

        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {
            if (enemyCallbackType == EnemyCallbackType.HitFinish)
            {
                ParticleCase particleCase = ParticlesController.PlayParticle(explosionParticleHash);

                particleCase.SetPosition(bombBone.position);
                particleCase.SetDuration(1f);

                ParticleCase decalCase = ParticlesController.PlayParticle(explosionDecalParticleHash).SetRotation(Quaternion.Euler(-90, 0, 0)).SetScale((10.0f).ToVector3());

                decalCase.SetPosition(transform.position);
                decalCase.SetDuration(5f);

                bombObj.gameObject.SetActive(false);

                if (Vector3.Distance(transform.position, Target.position) <= explosionRadius)
                {
                    characterBehaviour.TakeDamage(GetCurrentDamage());
                }

                List<BaseEnemyBehavior> aliveEnemies = ActiveRoom.GetAliveEnemies();

                for (int i = 0; i < aliveEnemies.Count; i++)
                {
                    BaseEnemyBehavior enemy = aliveEnemies[i];

                    if (enemy == this)
                        continue;

                    if (Vector3.Distance(transform.position, enemy.transform.position) <= explosionRadius)
                    {
                        Vector3 bombPos = bombObj.transform.position;
                        Vector3 direction = (enemy.transform.position.SetY(0) - bombPos.SetY(0)).normalized;

                        enemy.TakeDamage(GetCurrentDamage(), bombPos, direction);
                    }
                }

                explosionCircle.gameObject.SetActive(false);

                exploded = true;

                AudioController.PlaySound(AudioController.Sounds.explode);

                OnDeath();

                gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (!LevelController.IsGameplayActive)
                return;

            healthbarBehaviour.FollowUpdate();
        }

        public override void Attack()
        {
            animatorRef.SetTrigger(ANIMATOR_ATTACK_HASH);
            navMeshAgent.speed = 0;
            navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
            CanPursue = false;
            CanMove = false;

            explosionCircle.gameObject.SetActive(true);

            explosionCircle.transform.localScale = new Vector3(0f, 0.2f, 0f);
            explosionCircle.transform.DOScale(new Vector3(explosionRadius * 2f, explosionRadius * 2f, explosionRadius * 2f), 1.66f).SetEasing(Ease.Type.QuadOut);
        }

        protected override void OnDeath()
        {
            base.OnDeath();

            explosionRadiusScaleCase.KillActive();
            explosionCircle.gameObject.SetActive(false);

            fuseObj.SetActive(false);

            if (exploded)
            {
                ragdollCase.KillActive();
            }
        }
    }
}