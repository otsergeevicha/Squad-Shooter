using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class GenerateNavigation : MonoBehaviour
{
    NavMeshSurface navMesh;

    private void Start()
    {
        navMesh = GetComponent<NavMeshSurface>();
        Invoke(nameof(UpdateNavMesh), 0.2f);
    }

    void UpdateNavMesh()
    {
        navMesh.BuildNavMesh();
    }
}
