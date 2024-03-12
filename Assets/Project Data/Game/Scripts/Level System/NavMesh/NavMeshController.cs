using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Watermelon.LevelSystem
{

    public static class NavMeshController
    {
        private static List<INavMeshAgent> navMeshAgents;
        private static int navMeshAgentsCount;

        private static NavMeshSurface navMeshSurface;
        public static NavMeshSurface NavMeshSurface => navMeshSurface;

        private static bool isNavMeshCalculated;
        public static bool IsNavMeshCalculated => isNavMeshCalculated;

        public static event SimpleCallback OnNavMeshRecalculated;

        private static TweenCase navMeshTweenCase;
        private static bool navMeshRecalculating;

        public static void Initialise(GameObject parentObject, NavMeshData navMeshData)
        {
            navMeshSurface = parentObject.AddComponent<NavMeshSurface>();
            navMeshSurface.enabled = false;

            navMeshSurface.agentTypeID = 0; // Humanoid
            navMeshSurface.defaultArea = 0; // Walkable
            navMeshSurface.collectObjects = CollectObjects.Children;
            navMeshSurface.layerMask = 1 << PhysicsHelper.LAYER_GROUND;
            navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

            navMeshSurface.overrideVoxelSize = true;
            navMeshSurface.voxelSize = 0.2f;

            navMeshSurface.minRegionArea = 2;
            navMeshSurface.buildHeightMesh = false;

            navMeshSurface.navMeshData = navMeshData;
            navMeshSurface.enabled = true;

            navMeshAgents = new List<INavMeshAgent>();
            navMeshAgentsCount = 0;
        }

        public static void RecalculateNavMesh(SimpleCallback simpleCallback)
        {
            if (navMeshRecalculating)
                return;

            navMeshRecalculating = true;

            navMeshTweenCase = new NavMeshSurfaceTweenCase(navMeshSurface).OnComplete(delegate
            {
                OnRecalculationFinished();

                simpleCallback?.Invoke();
            }).StartTween();
        }

        private static void OnRecalculationFinished()
        {
            isNavMeshCalculated = true;

            // Activate agents
            for (int i = 0; i < navMeshAgentsCount; i++)
            {
                navMeshAgents[i].OnNavMeshUpdated();
            }

            navMeshRecalculating = false;

            navMeshTweenCase = null;

            OnNavMeshRecalculated?.Invoke();
        }

        public static void InvokeOrSubscribe(INavMeshAgent navMeshAgent)
        {
            if (isNavMeshCalculated)
            {
                navMeshAgent.OnNavMeshUpdated();
            }
            else
            {
                navMeshAgents.Add(navMeshAgent);
                navMeshAgentsCount++;
            }
        }

        public static void ForceActivation()
        {
            if (isNavMeshCalculated)
                return;

            if (navMeshTweenCase != null)
            {
                navMeshTweenCase.Kill();
                navMeshTweenCase = null;
            }

            OnRecalculationFinished();
        }

        public static void ClearAgents()
        {
            navMeshAgents.Clear();
            navMeshAgentsCount = 0;
        }

        public static void Reset()
        {
            if (navMeshTweenCase != null)
            {
                navMeshTweenCase.Kill();
                navMeshTweenCase = null;
            }

            navMeshRecalculating = false;
            isNavMeshCalculated = false;

            navMeshAgents.Clear();
            navMeshAgentsCount = 0;
        }
    }
}