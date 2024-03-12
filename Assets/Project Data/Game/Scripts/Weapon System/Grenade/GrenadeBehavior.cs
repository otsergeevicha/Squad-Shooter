using System.Collections;
using UnityEngine;
using Watermelon;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class GrenadeBehavior : MonoBehaviour
    {
        public float angle = 45f;
        public float gravity = 150f;

        public DuoVector3 angularVelocityDuo;
        private Vector3 angularVelocity;

        [SerializeField] new Rigidbody rigidbody;
        [SerializeField] MeshRenderer sphereRenderer;
        [SerializeField] float explosionRadius;

        [SerializeField] Color startColor;
        [SerializeField] Color endColor;
        [SerializeField] Ease.Type easing;

        private int explosionParticleHash;
        private int explosionDecalParticleHash;
        float duration;

        private void Awake()
        {
            explosionParticleHash = ParticlesController.GetHash("Bomber Explosion");
            explosionDecalParticleHash = ParticlesController.GetHash("Bomber Explosion Decal");
        }

        public void Throw(Vector3 startPosition, Vector3 targetPosition, float damage)
        {
            gameObject.SetActive(true);
            angularVelocity = angularVelocityDuo.Random();

            transform.position = startPosition;

            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            StartCoroutine(ThrowCoroutine(startPosition, targetPosition, angle, gravity, damage));

            sphereRenderer.gameObject.SetActive(true);
            sphereRenderer.material.SetColor("_BaseColor", startColor);
            sphereRenderer.transform.localScale = Vector3.zero;

            sphereRenderer.material.DOColor(Shader.PropertyToID("_BaseColor"), endColor, duration + 0.5f).SetEasing(easing);
            sphereRenderer.DOScale(explosionRadius * 2, duration + 0.25f).SetEasing(easing);
        }

        IEnumerator ThrowCoroutine(Vector3 startPosition, Vector3 targetPosition, float angle, float gravity, float damage)
        {
            var distance = Vector3.Distance(startPosition, targetPosition);
            var direction = (targetPosition - startPosition).normalized;

            var velocity = distance / (Mathf.Sin(2 * angle * Mathf.Deg2Rad) / gravity);

            var Vx = Mathf.Sqrt(velocity) * Mathf.Cos(angle * Mathf.Deg2Rad);
            var Vy = Mathf.Sqrt(velocity) * Mathf.Sin(angle * Mathf.Deg2Rad);

            duration = distance / Vx;

            var time = 0f;
            var prevPos = transform.position;

            while (time < duration)
            {
                prevPos = transform.position;
                transform.position += Vector3.up * (Vy - gravity * time) * Time.deltaTime + direction * Vx * Time.deltaTime;
                transform.eulerAngles += angularVelocity * Time.deltaTime;

                time += Time.deltaTime;

                yield return null;
            }

            rigidbody.isKinematic = false;
            rigidbody.useGravity = true;

            Vector3 calculatedVelocity = (transform.position - prevPos) / Time.deltaTime;
            Vector3 clampedVelocity = new Vector3(Mathf.Clamp(calculatedVelocity.x, -100f, 100f), Mathf.Clamp(calculatedVelocity.y, -100f, 100f), Mathf.Clamp(calculatedVelocity.z, -100f, 100f));
            rigidbody.velocity = clampedVelocity;
            rigidbody.angularVelocity = angularVelocity;

            yield return new WaitForSeconds(0.5f);

            var explosionCase = ParticlesController.PlayParticle(explosionParticleHash);
            explosionCase.SetPosition(transform.position);

            var explosionDecalCase = ParticlesController.PlayParticle(explosionDecalParticleHash);
            explosionDecalCase.SetPosition(transform.position).SetScale(Vector3.one * 3f).SetRotation(Quaternion.Euler(-90f, 0f, 0f));

            gameObject.SetActive(false);

            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();

            if (Vector3.Distance(transform.position, characterBehaviour.transform.position) <= explosionRadius)
            {
                characterBehaviour.TakeDamage(damage);
            }

            var aliveEnemies = ActiveRoom.GetAliveEnemies();

            for (int i = 0; i < aliveEnemies.Count; i++)
            {
                var enemy = aliveEnemies[i];

                if (enemy == this)
                    continue;

                if (Vector3.Distance(transform.position, enemy.transform.position) <= explosionRadius)
                {
                    var directionToEnemy = (enemy.transform.position.SetY(0) - transform.position.SetY(0)).normalized;
                    enemy.TakeDamage(damage, transform.position, directionToEnemy);
                }
            }
        }
    }
}