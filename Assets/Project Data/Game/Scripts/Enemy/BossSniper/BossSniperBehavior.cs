using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class BossSniperBehavior : BaseEnemyBehavior
    {
        [Header("Bullet")]
        [SerializeField] GameObject bulletPrefab;
        [SerializeField] Transform shootingPoint;
        [SerializeField] ParticleSystem gunFireParticle;
        [SerializeField] float bulletSpeed;
        [SerializeField] LayerMask collisionLayer;

        [Space]
        [SerializeField] List<LaserLine> lasers;
        public List<LaserLine> Lasers => lasers;

        [SerializeField] float yellowAimingDuration;
        public float YellowAinimgDuration => yellowAimingDuration;

        [SerializeField] float redAimingDuration;
        public float RedAimingDuration => redAimingDuration;

        [SerializeField] bool isRedStatic;
        public bool IsRedStatic => isRedStatic;

        [SerializeField] float laserThickness;

        [SerializeField] Color yellowColor;
        [Space]
        [SerializeField] Color redColor;

        [Space]
        [SerializeField] GameObject auraParticle;

        private Pool bulletPool;

        protected override void Awake()
        {
            base.Awake();

            bulletPool = new Pool(new PoolSettings(bulletPrefab.name, bulletPrefab, 3, true));
        }

        public override void Attack()
        {
            animatorRef.SetTrigger("Shoot");
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            healthbarBehaviour.FollowUpdate();
        }

        public override void Initialise()
        {
            base.Initialise();

            auraParticle.SetActive(true);
        }

        protected override void OnDeath()
        {
            base.OnDeath();

            auraParticle.SetActive(false);
        }

        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {
            switch (enemyCallbackType)
            {
                case EnemyCallbackType.Hit:
                    BossSniperBulletBehavior bullet = bulletPool.GetPooledObject(new PooledObjectSettings(false).SetPosition(shootingPoint.position).SetEulerRotation(shootingPoint.eulerAngles)).GetComponent<BossSniperBulletBehavior>();
                    bullet.transform.forward = transform.forward;
                    bullet.Initialize(GetCurrentDamage(), bulletSpeed, 1000, points);

                    gunFireParticle.Play();
                    break;

                case EnemyCallbackType.HitFinish:

                    InvokeOnAttackFinished();
                    break;
            }
        }

        public void MakeLaserYellow()
        {
            lasers.ForEach((laser) => laser.SetColor(yellowColor));
        }

        public void MakeLaserRed()
        {
            lasers.ForEach((laser) => laser.SetColor(redColor));
        }

        public void EnableLaser()
        {
            lasers.ForEach((laser) => laser.SetActive(true));
        }

        public void DisableLaser()
        {
            lasers.ForEach((laser) => laser.SetActive(false));
        }

        private List<Vector3> points;

        public void AimLaser()
        {
            var startPos = shootingPoint.position;
            var direction = Rotation * Vector3.forward;

            points = new List<Vector3>();

            for (int i = 0; i < lasers.Count; i++)
            {
                var laser = lasers[i];

                laser.SetActive(true);

                var shouldEnd = false;
                if (Physics.Raycast(startPos, direction, out var hitInfo, 300f, collisionLayer))
                {
                    laser.SetPosition(startPos, hitInfo.point, new Vector3(laserThickness, laserThickness, Vector3.Distance(hitInfo.point, startPos)));

                    startPos = hitInfo.point - direction * 0.2f;
                    direction = Vector3.Reflect(direction, -hitInfo.normal);

                    points.Add(startPos);

                    if (hitInfo.collider.gameObject == Target.gameObject)
                        shouldEnd = true;
                }
                else
                {
                    var hitPoint = startPos + direction * 300;

                    points.Add(hitPoint);

                    laser.SetPosition(startPos, hitPoint, new Vector3(laserThickness, laserThickness, Vector3.Distance(hitPoint, startPos)));

                    shouldEnd = true;
                }

                if (shouldEnd)
                {
                    for (int j = i + 1; j < lasers.Count; j++)
                    {
                        var laserToDisable = lasers[j];

                        laserToDisable.SetActive(false);
                    }

                    break;
                }
            }
        }

        public bool RaycastBox(Ray ray, BoxCollider box, out Vector3 intersectionPoint, out Vector3 normal)
        {
            float tmin = float.MinValue;
            float tmax = float.MaxValue;

            // Transform the ray to be in the box's local space
            Matrix4x4 worldToLocal = box.transform.worldToLocalMatrix;
            ray.origin = worldToLocal.MultiplyPoint(ray.origin);
            ray.direction = worldToLocal.MultiplyVector(ray.direction);

            normal = Vector3.zero;

            // Calculate t values for each plane of the box
            for (int i = 0; i < 3; i++)
            {
                if (Mathf.Abs(ray.direction[i]) < Mathf.Epsilon)
                {
                    if (ray.origin[i] < -box.size[i] / 2 || ray.origin[i] > box.size[i] / 2)
                    {
                        intersectionPoint = Vector3.zero;
                        normal = Vector3.zero;
                        return false;
                    }
                }
                else
                {
                    float t1 = (-box.size[i] / 2 - ray.origin[i]) / ray.direction[i];
                    float t2 = (box.size[i] / 2 - ray.origin[i]) / ray.direction[i];
                    if (t1 > t2)
                    {
                        float temp = t1;
                        t1 = t2;
                        t2 = temp;
                    }
                    if (t1 > tmin)
                    {
                        tmin = t1;
                        normal = Vector3.zero;
                        normal[i] = -1;
                    }
                    if (t2 < tmax)
                    {
                        tmax = t2;
                    }
                    if (tmin > tmax)
                    {
                        intersectionPoint = Vector3.zero;
                        normal = Vector3.zero;
                        return false;
                    }
                }
            }

            // Calculate intersection point and normal in world space
            intersectionPoint = ray.origin + ray.direction * tmin;
            normal = box.transform.TransformDirection(normal);

            return true;
        }

        [System.Serializable]
        public class LaserLine
        {
            [SerializeField] MeshRenderer meshRenderer;

            public void SetColor(Color color)
            {
                meshRenderer.material.SetColor("_BaseColor", color);
            }

            public void SetActive(bool isActive)
            {
                meshRenderer.gameObject.SetActive(isActive);
            }

            public void SetPosition(Vector3 startPos, Vector3 hitPos, Vector3 scale)
            {
                var middlePoint = (startPos + hitPos) / 2f;

                meshRenderer.transform.position = middlePoint;
                meshRenderer.transform.localScale = scale;
                meshRenderer.transform.rotation = Quaternion.LookRotation((hitPos - startPos).normalized);
            }
        }
    }
}