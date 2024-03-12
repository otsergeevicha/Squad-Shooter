using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    using UI.Particle;

    public class ExperienceStarsManager : MonoBehaviour
    {
        private readonly string TRAIL_POOL_NAME = "Custom UI Trail";
        private readonly string PARTICLE_POOL_NAME = "Custom UI Particle";

        [Header("Data")]
        [SerializeField] ExperienceStarsFlightData starsData;

        [Header("Stars")]
        [SerializeField] RectTransform starsHolder;

        [Space]
        [SerializeField] GameObject starUIPrefab;

        [Space]
        [SerializeField] Transform starIconTransform;
        [SerializeField] JuicyBounce starIconBounce;

        [Header("Particles")]
        [SerializeField] RectTransform particlesParent;

        [Space]
        [SerializeField] GameObject trailPrefab;
        [SerializeField] GameObject particlePrefab;


        private PoolGeneric<UIParticleTrail> trailPool;
        private PoolGeneric<UIParticle> particlePool;

        private Pool starsPool;


        private List<ExpStarData> starsInfo = new List<ExpStarData>();
        private System.Action OnComplete;

        private ExperienceUIController experienceUIController;

        public void Awake()
        {
            AssignPools();
        }

        public void Initialise(ExperienceUIController experienceUIController)
        {
            this.experienceUIController = experienceUIController;

            starIconBounce.Initialise(starIconTransform);
        }

        private void AssignPools()
        {
            if (PoolManager.PoolExists(TRAIL_POOL_NAME))
                trailPool = PoolManager.GetPoolByName<UIParticleTrail>(TRAIL_POOL_NAME);
            else
                trailPool = new PoolGeneric<UIParticleTrail>(new PoolSettings
                {
                    name = TRAIL_POOL_NAME,
                    singlePoolPrefab = trailPrefab,
                    size = 5,
                    objectsContainer = particlesParent,
                    autoSizeIncrement = true,
                    type = Pool.PoolType.Single
                });

            if (PoolManager.PoolExists(TRAIL_POOL_NAME))
                particlePool = PoolManager.GetPoolByName<UIParticle>(PARTICLE_POOL_NAME);
            else
                particlePool = new PoolGeneric<UIParticle>(new PoolSettings
                {
                    name = PARTICLE_POOL_NAME,
                    singlePoolPrefab = particlePrefab,
                    size = 100,
                    objectsContainer = particlesParent,
                    autoSizeIncrement = true,
                    type = Pool.PoolType.Single
                });

            if (PoolManager.PoolExists(starUIPrefab.name))
                starsPool = PoolManager.GetPoolByName(starUIPrefab.name);
            else
                starsPool = PoolManager.AddPool(new PoolSettings(starUIPrefab.name, starUIPrefab, 3, true, transform));
        }

        public void PlayXpGainedAnimation(int starsAmount, Vector3 worldPos, System.Action OnComplete = null)
        {
            this.OnComplete = OnComplete;

            starsAmount = Mathf.Clamp(starsAmount, 1, 10);

            for (int i = 0; i < starsAmount; i++)
            {
                RectTransform starRect = starsPool.GetPooledObject().GetComponent<RectTransform>();

                starRect.SetParent(transform.parent);
                starRect.anchoredPosition = Camera.main.WorldToScreenPoint(worldPos) + new Vector3(Random.Range(-25f, 25f), Random.Range(-25f, 25f), 0f);
                starRect.SetParent(starsHolder);

                Vector2 startDirection = Random.onUnitSphere;
                Vector2 endPoint = Vector2.zero;

                var data = new ExpStarData()
                {
                    star = starRect,

                    startPoint = starRect.anchoredPosition,
                    middlePoint = starRect.anchoredPosition + startDirection * starsData.FirstStageDistance,

                    key1 = starRect.anchoredPosition + startDirection * starsData.Key1,
                    key2 = endPoint + starsData.Key2,

                    endPoint = endPoint,

                    startTime = Time.time,
                    duration1 = starsData.FirstStageDuration,
                    duration2 = starsData.SecondStageDuration
                };

                data.SetCurves(starsData);

                var trail = trailPool.GetPooledComponent();

                trail.SetPool(particlePool);

                trail.AnchoredPos = starRect.anchoredPosition;
                trail.transform.localScale = Vector3.one;

                data.SetTrail(trail);

                starsInfo.Add(data);
            }
        }

        private void Update()
        {
            if (starsInfo.IsNullOrEmpty())
                return;

            for (int i = 0; i < starsInfo.Count; i++)
            {
                var data = starsInfo[i];

                if (data.Update())
                {
                    starsInfo.RemoveAt(i);
                    i--;

                    starIconBounce.Bounce();

                    experienceUIController.OnStarHitted();
                }
            }

            if (starsInfo.IsNullOrEmpty())
                OnComplete?.Invoke();
        }

        [Button]
        public void Spawn2Stars()
        {
            ExperienceController.GainXPPoints(2);
        }
        [Button]
        public void Spawn5Stars()
        {
            ExperienceController.GainXPPoints(5);
        }
        [Button]
        public void Spawn10Stars()
        {
            ExperienceController.GainXPPoints(10);
        }

        private class ExpStarData
        {
            public RectTransform star;

            public Vector2 startPoint;
            public Vector2 middlePoint;

            public Vector2 key1;
            public Vector2 key2;

            public Vector2 endPoint;

            public float startTime;
            public float duration1;
            public float duration2;

            private ExperienceStarsFlightData data;

            private UIParticleTrail trail;

            public void SetCurves(ExperienceStarsFlightData data)
            {
                this.data = data;
            }

            public void SetTrail(UIParticleTrail trail)
            {
                this.trail = trail;

                trail.Init();
                trail.IsPlaying = true;
            }

            public bool Update()
            {
                var time = Time.time - startTime;

                if (time > duration1)
                {
                    var t = (time - duration1) / duration2;

                    if (t >= 1)
                    {
                        star.gameObject.SetActive(false);

                        trail.DisableWhenReady = true;
                        trail.IsPlaying = false;

                        return true;
                    }

                    SecondStageUpdate(t);
                }
                else
                {
                    var t = time / duration1;

                    FirstStageUpdate(t);
                }

                return false;
            }

            public void FirstStageUpdate(float t)
            {
                var prevPos = star.anchoredPosition;

                star.anchoredPosition = Vector2.Lerp(startPoint, middlePoint, data.PathCurve1.Evaluate(t));
                star.localScale = Vector3.one * data.StarsScale1.Evaluate(t);

                trail.NormalizedVelocity = (star.anchoredPosition - prevPos).normalized;

                trail.AnchoredPos = star.anchoredPosition;
            }

            public void SecondStageUpdate(float t)
            {
                var prevPos = star.anchoredPosition;

                star.anchoredPosition = Bezier.EvaluateCubic(middlePoint, key1, key2, endPoint, data.PathCurve2.Evaluate(t));
                star.localScale = Vector3.one * data.StarsScale2.Evaluate(t);

                trail.NormalizedVelocity = (star.anchoredPosition - prevPos).normalized;

                trail.AnchoredPos = star.anchoredPosition;
            }
        }
    }
}