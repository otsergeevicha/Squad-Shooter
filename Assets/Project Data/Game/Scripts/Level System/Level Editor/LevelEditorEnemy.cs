#pragma warning disable 649
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    [ExecuteInEditMode]
    public class LevelEditorEnemy : MonoBehaviour
    {
#if UNITY_EDITOR
        public EnemyType type;
        public bool isElite;
        public List<Transform> pathPoints;
        public Transform pathPointsContainer;

        //Gizmo
        private const int LINE_HEIGHT = 30;
        private Color enemyColor;
        private Color defaultColor;
        private Color goldColor;
        private Material enemyMaterial;
        private StartPointHandles startPointHandles;
        private bool isStartPointHandlesInited;


        public void Awake()
        {
            pathPoints = new List<Transform>();
            enemyColor = Random.ColorHSV(0f, 1, 0.75f, 1, 1f, 1, 1, 1);
            enemyMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            enemyMaterial.color = enemyColor;
            goldColor = new Color(1, 204 / 255f, 0);
            isStartPointHandlesInited = false;
        }

        public void Update()
        {
            for (int i = 0; i < pathPointsContainer.childCount; i++)
            {
                if (!pathPoints.Contains(pathPointsContainer.GetChild(i)))
                {
                    pathPoints.Add(pathPointsContainer.GetChild(i));
                }
            }

            for (int i = pathPoints.Count - 1; i >= 0; i--)
            {
                if(pathPoints[i] == null)
                {
                    pathPoints.RemoveAt(i);
                }
            }

            if(isElite && (!isStartPointHandlesInited))
            {
                startPointHandles =  gameObject.AddComponent<StartPointHandles>();
                startPointHandles.diskRadius = 4.5f;
                startPointHandles.thickness = 7f;
                startPointHandles.diskColor = goldColor;
                startPointHandles.displayText = false;
                isStartPointHandlesInited = true;
            }
            else if(!isElite && isStartPointHandlesInited)
            {
                DestroyImmediate(startPointHandles);
                isStartPointHandlesInited = false;
            }

        }

        [Button]
        public void AddPathPoint()
        {
            GameObject sphere;

            sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(pathPointsContainer);
            sphere.transform.localPosition = Vector3.back * 12;
            sphere.transform.localScale = Vector3.one * 5f;
            pathPoints.Add(sphere.transform);

            sphere.GetComponent<MeshRenderer>().sharedMaterial = enemyMaterial;
            Selection.activeGameObject = sphere;
        }

        [Button]
        public void ApplyMaterialToPathPoints()
        {
            MeshRenderer renderer;

            for (int i = 0; i < pathPoints.Count; i++)
            {
                renderer = pathPoints[i].GetComponent<MeshRenderer>();
                renderer.sharedMaterial = enemyMaterial;
            }
        }

        private void DrawLine(Vector3 tempLineStart, Vector3 tempLineEnd)
        {
            Vector3 offset = Vector3.zero.AddToY(0.01f);

            for (int i = 0; i < LINE_HEIGHT; i++)
            {
                Gizmos.DrawLine(tempLineStart + i * offset, tempLineEnd + i * offset);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = enemyColor;

            Gizmos.DrawSphere(transform.position + Vector3.up * 16, 2f);

            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                DrawLine(pathPoints[i].transform.position, pathPoints[i + 1].transform.position);
            }


            Gizmos.color = defaultColor;
        }

        public Vector3[] GetPathPoints()
        {
            Vector3[] result = new Vector3[pathPoints.Count];

            for (int i = 0; i < pathPoints.Count; i++)
            {
                result[i] = pathPoints[i].localPosition + pathPointsContainer.transform.parent.localPosition;
            }

            return result;
        }

#endif
    }
}