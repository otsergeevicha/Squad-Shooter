using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class EliteCase
    {
        public List<MeshPair> pairs;
        public List<SimpleMeshPair> simplePairs;

        public void SetElite()
        {
            pairs?.ForEach((pair) => pair.renderer.sharedMesh = pair.eliteMesh);
            simplePairs?.ForEach((pair) => pair.filter.mesh = pair.eliteMesh);
        }

        public void SetRegular()
        {
            pairs?.ForEach((pair) => pair.renderer.sharedMesh = pair.simpleMesh);
            simplePairs?.ForEach((pair) => pair.filter.mesh = pair.simpleMesh);
        }

        [System.Serializable]
        public struct MeshPair
        {
            public SkinnedMeshRenderer renderer;
            public Mesh simpleMesh;
            public Mesh eliteMesh;
        }

        [System.Serializable]
        public struct SimpleMeshPair
        {
            public MeshFilter filter;
            public Mesh simpleMesh;
            public Mesh eliteMesh;
        }
    }
}