using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class NavigationController : MonoBehaviour
    {
        public const float PATH_UPDATE_TIME = 0.1f;

        [SerializeField] GameObject arrowPrefab;

        private static List<ArrowCase> activeArrows = new List<ArrowCase>();
        private static int activeArrowsCount;

        private static Pool arrowPool;

        public void Initialise()
        {
            arrowPool = new Pool(new PoolSettings()
            {
                autoSizeIncrement = true,
                objectsContainer = null,
                size = 1,
                type = Pool.PoolType.Single,
                singlePoolPrefab = arrowPrefab,
                name = "Nivagation Arrow"
            });

            activeArrows = new List<ArrowCase>();
        }

        public static ArrowCase RegisterArrow(Transform parent, Vector3 target, Color color, float hideDistance = -1, bool autoDisable = false)
        {
            ArrowCase arrowCase = new ArrowCase(parent, arrowPool.GetPooledObject(), color, null, target, hideDistance, autoDisable);

            activeArrows.Add(arrowCase);
            activeArrowsCount++;

            return arrowCase;
        }

        public static ArrowCase RegisterArrow(Transform parent, Transform target, Color color, float hideDistance = -1, bool autoDisable = false)
        {
            ArrowCase arrowCase = new ArrowCase(parent, arrowPool.GetPooledObject(), color, target, Vector3.zero, hideDistance, autoDisable);

            activeArrows.Add(arrowCase);
            activeArrowsCount++;

            return arrowCase;
        }

        private void LateUpdate()
        {
            if (activeArrowsCount > 0)
            {
                for (int i = 0; i < activeArrowsCount; i++)
                {
                    activeArrows[i].LateUpdate();

                    if (activeArrows[i].IsTargetReached)
                    {
                        activeArrows.RemoveAt(i);
                        activeArrowsCount--;

                        i--;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            activeArrows = null;
            activeArrowsCount = 0;
        }

        
    }
}