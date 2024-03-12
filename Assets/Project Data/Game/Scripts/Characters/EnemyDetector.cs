using System.Collections.Generic;
using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{

    public class EnemyDetector : MonoBehaviour
    {
        [SerializeField] float checkDelay = 1f;

        private SphereCollider detectorCollider;
        public SphereCollider DetectorCollider => detectorCollider;

        private int detectedEnemiesCount;
        private List<BaseEnemyBehavior> detectedEnemies;
        public List<BaseEnemyBehavior> DetectedEnemies => detectedEnemies;

        private BaseEnemyBehavior closestEnemy;
        public BaseEnemyBehavior ClosestEnemy => closestEnemy;

        public float DetectorRadius => detectorCollider.radius;

        private float nextClosestCheckTime = 0.0f;

        private IEnemyDetector enemyDetector;

        public void Initialise(IEnemyDetector enemyDetector)
        {
            this.enemyDetector = enemyDetector;

            // Get detector collider
            detectorCollider = GetComponent<SphereCollider>();

            // Prepare variables
            detectedEnemies = new List<BaseEnemyBehavior>();
            detectedEnemiesCount = 0;

            // Subscribe to enemy dying callback
            BaseEnemyBehavior.OnDiedEvent += OnEnemyDied;
        }

        public void SetRadius(float radius)
        {
            detectorCollider.radius = radius;
        }

        private void OnEnemyDied(BaseEnemyBehavior enemy)
        {
            RemoveEnemy(enemy);
        }

        public void UpdateClosestEnemy()
        {
            if (detectedEnemiesCount == 0)
            {
                if (closestEnemy != null)
                    enemyDetector.OnCloseEnemyChanged(null);

                closestEnemy = null;

                return;
            }

            float minDistanceSqr = float.MaxValue;
            BaseEnemyBehavior tempEnemy = null;

            for (int i = 0; i < detectedEnemiesCount; i++)
            {
                var enemy = detectedEnemies[i];

                float distanceSqr = (transform.position - enemy.transform.position).sqrMagnitude;

                if (distanceSqr < minDistanceSqr && !enemy.IsDead)
                {
                    tempEnemy = enemy;
                    minDistanceSqr = distanceSqr;
                }
            }

            if (closestEnemy != tempEnemy)
                enemyDetector.OnCloseEnemyChanged(tempEnemy);

            closestEnemy = tempEnemy;
        }

        private void Update()
        {
            if (detectedEnemiesCount > 1 && Time.time > nextClosestCheckTime)
            {
                nextClosestCheckTime = Time.time + checkDelay;

                UpdateClosestEnemy();
            }
        }

        private void RemoveEnemy(BaseEnemyBehavior enemy)
        {
            int enemyIndex = detectedEnemies.FindIndex(x => x == enemy);
            if (enemyIndex != -1)
            {
                detectedEnemies.RemoveAt(enemyIndex);
                detectedEnemiesCount--;

                UpdateClosestEnemy();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(PhysicsHelper.TAG_ENEMY))
            {
                BaseEnemyBehavior enemy = other.GetComponent<BaseEnemyBehavior>();
                if (enemy != null)
                {
                    if (!detectedEnemies.Contains(enemy))
                    {
                        detectedEnemies.Add(enemy);
                        detectedEnemiesCount++;

                        UpdateClosestEnemy();
                    }
                }
            }
        }

        public void TryAddClosestEnemy(BaseEnemyBehavior enemy)
        {
            if (!detectedEnemies.Contains(enemy))
            {
                if (Vector3.Distance(enemy.transform.position, transform.position) <= DetectorRadius)
                {
                    detectedEnemies.Add(enemy);
                    detectedEnemiesCount++;

                    UpdateClosestEnemy();
                }
            }
            else
            {
                UpdateClosestEnemy();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag(PhysicsHelper.TAG_ENEMY))
            {
                BaseEnemyBehavior enemy = other.GetComponent<BaseEnemyBehavior>();
                if (enemy != null)
                {
                    RemoveEnemy(enemy);
                }
            }
        }

        public void ClearZombiesList()
        {
            detectedEnemies.Clear();

            UpdateClosestEnemy();
        }

        private void OnDestroy()
        {
            BaseEnemyBehavior.OnDiedEvent -= OnEnemyDied;
        }

        public void Reload()
        {
            detectedEnemies.Clear();
            detectedEnemiesCount = 0;
            closestEnemy = null;
        }
    }
}
