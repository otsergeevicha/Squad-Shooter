using Unity.AI.Navigation;
using UnityEngine;

namespace Watermelon.LevelSystem
{
    public class NavMeshSurfaceTweenCase : TweenCase
    {
        private AsyncOperation asyncOperation;

        public NavMeshSurfaceTweenCase(NavMeshSurface navMeshSurface)
        {
            duration = float.MaxValue;

            asyncOperation = navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
        }

        public override void DefaultComplete()
        {

        }

        public override void Invoke(float deltaTime)
        {
            if (asyncOperation.isDone)
                Complete();
        }

        public override bool Validate()
        {
            return true;
        }
    }
}