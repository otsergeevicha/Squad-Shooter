using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class NavigationArrowController
    {
        [SerializeField] GameObject lineArrowPrefab;

        private static List<BaseNavigationArrowCase> activeArrows = new List<BaseNavigationArrowCase>();
        private static int activeArrowsCount;

        private static Pool lineArrowPool;

        public void Initialise()
        {
            lineArrowPool = new Pool(new PoolSettings()
            {
                autoSizeIncrement = true,
                objectsContainer = null,
                size = 0,
                type = Pool.PoolType.Single,
                singlePoolPrefab = lineArrowPrefab,
                name = "Line Navigation Arrow"
            });

            activeArrows = new List<BaseNavigationArrowCase>();
        }

        public static LineNavigationArrowCase RegisterLineArrow(Transform parent, Vector3 target)
        {
            LineNavigationArrowCase arrowCase = new LineNavigationArrowCase(parent, lineArrowPool.GetPooledObject(), target);

            activeArrows.Add(arrowCase);
            activeArrowsCount++;

            return arrowCase;
        }

        public void LateUpdate()
        {
            if (activeArrowsCount > 0)
            {
                for (int i = 0; i < activeArrowsCount; i++)
                {
                    if (activeArrows[i].IsArrowFixed)
                        activeArrows[i].UpdateFixedPosition();

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

        public void Unload()
        {
            activeArrows.Clear();
            activeArrowsCount = 0;

            lineArrowPool.ReturnToPoolEverything(true);
        }
    }
}