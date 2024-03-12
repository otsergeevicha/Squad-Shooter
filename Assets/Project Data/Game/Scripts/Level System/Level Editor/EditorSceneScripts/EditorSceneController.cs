#pragma warning disable 649

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    public class EditorSceneController : MonoBehaviour
    {

#if UNITY_EDITOR
        private static EditorSceneController instance;
        public static EditorSceneController Instance { get => instance; }

        [SerializeField] private GameObject container;
        public List<GameObject> rooms;
        private int selectedRoom = 0;
        public Vector3 spawnPoint;
        [SerializeField] float startPointSphereSize;
        public GameObject Container { set => container = value; }
        public float StartPointSphereSize { set => startPointSphereSize = value; }

        public EditorSceneController()
        {
            instance = this;
            rooms = new List<GameObject>();
        }

        public void SpawnEnvironment(GameObject prefab, Vector3 position, Quaternion rotation, LevelEnvironmentType type)
        {
            GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            gameObject.transform.SetParent(rooms[selectedRoom].transform);

            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = rotation;
            LevelEditorEnvironment levelEditorEnvironment = gameObject.AddComponent<LevelEditorEnvironment>();
            levelEditorEnvironment.type = type;
        }

        

        public void SpawnObstacle(GameObject prefab, Vector3 position, Quaternion rotation, LevelObstaclesType type)
        {
            GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            gameObject.transform.SetParent(rooms[selectedRoom].transform);
            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = rotation;
            LevelEditorObstacle levelEditorObstacle = gameObject.AddComponent<LevelEditorObstacle>();
            levelEditorObstacle.type = type;
            Selection.activeGameObject = gameObject;
        }


        public void SpawnEnemy(GameObject prefab, Vector3 position, Quaternion rotation, EnemyType type, bool isElite, Vector3[] pathPoints)
        {
            GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            gameObject.transform.SetParent(rooms[selectedRoom].transform);
            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = rotation;
            LevelEditorEnemy levelEditorEnemy = gameObject.AddComponent<LevelEditorEnemy>();
            levelEditorEnemy.type = type;
            levelEditorEnemy.isElite = isElite;
            GameObject pointsContainer = new GameObject("PathPointsContainer");
            pointsContainer.transform.SetParent(gameObject.transform);
            levelEditorEnemy.pathPointsContainer = pointsContainer.transform;
            pointsContainer.transform.localPosition = Vector3.zero;

            GameObject sphere;

            for (int i = 0; i < pathPoints.Length; i++)
            {
                sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.SetParent(levelEditorEnemy.pathPointsContainer);
                sphere.transform.localPosition = pathPoints[i] - gameObject.transform.localPosition;
                sphere.transform.localScale = Vector3.one * 5f;
                levelEditorEnemy.pathPoints.Add(sphere.transform);
            }

            levelEditorEnemy.ApplyMaterialToPathPoints();
            Selection.activeGameObject = gameObject;
        }

        public void SpawnExitPoint(GameObject prefab, Vector3 position)
        {
            GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            gameObject.transform.SetParent(rooms[selectedRoom].transform);
            gameObject.transform.localPosition = position;

            LevelEditorExitPoint exitPoint = gameObject.AddComponent<LevelEditorExitPoint>();
        }

        public void SpawnChest(GameObject prefab, Vector3 position, Quaternion rotation, LevelChestType type)
        {
            GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            gameObject.transform.SetParent(rooms[selectedRoom].transform);
            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = rotation;
            var levelEditorChest = gameObject.AddComponent<LevelEditorChest>();
            levelEditorChest.type = type;
            Selection.activeGameObject = gameObject;
        }

        public void SpawnRoom()
        {
            GameObject room = new GameObject();
            room.transform.SetParent(container.transform);

            if(rooms.Count > 0)
            {
                room.transform.localPosition = new Vector3(0, 0, rooms.Count * 350);
            }

            rooms.Add(room);
            room.name = "Room #" + rooms.Count;
            selectedRoom = rooms.Count - 1;
        }

        public void DeleteRoom(int index)
        {
            GameObject room = rooms[index];
            rooms.RemoveAt(index);
            DestroyImmediate(room);

        }

        public void SelectRoom(int index)
        {
            selectedRoom = index;
            Selection.activeGameObject = rooms[selectedRoom];
            SceneView.lastActiveSceneView.FrameSelected();
        }

        public EnvironmentEntityData[] CollectEnvironmentsFromRoom(int roomIndex)
        {
            LevelEditorEnvironment[] editorData = rooms[roomIndex].GetComponentsInChildren<LevelEditorEnvironment>();
            EnvironmentEntityData[] result = new EnvironmentEntityData[editorData.Length];

            for (int i = 0; i < editorData.Length; i++)
            {
                result[i] = new EnvironmentEntityData(editorData[i].type, editorData[i].transform.localPosition, editorData[i].transform.localRotation);
            }

            return result;
        }

        public ObstacleEntityData[] CollectObstaclesFromRoom(int roomIndex)
        {
            LevelEditorObstacle[] editorData = rooms[roomIndex].GetComponentsInChildren<LevelEditorObstacle>();
            ObstacleEntityData[] result = new ObstacleEntityData[editorData.Length];

            for (int i = 0; i < editorData.Length; i++)
            {
                result[i] = new ObstacleEntityData(editorData[i].type, editorData[i].transform.localPosition, editorData[i].transform.localRotation);
            }

            return result;
        }

        public EnemyEntityData[] CollectEnemiesFromRoom(int roomIndex)
        {
            LevelEditorEnemy[] editorData = rooms[roomIndex].GetComponentsInChildren<LevelEditorEnemy>();
            EnemyEntityData[] result = new EnemyEntityData[editorData.Length];

            for (int i = 0; i < editorData.Length; i++)
            {
                result[i] = new EnemyEntityData(editorData[i].type, editorData[i].transform.localPosition, editorData[i].transform.localRotation, editorData[i].isElite,editorData[i].GetPathPoints());
            }

            return result;
        }

        public bool CollectExitPointFromRoom(int roomIndex, out Vector3 position)
        {
            LevelEditorExitPoint editorData = rooms[roomIndex].GetComponentInChildren<LevelEditorExitPoint>();

            if(editorData == null)
            {
                position = Vector3.zero;

                return false;
            }
            else
            {
                position = editorData.transform.localPosition;

                return true;
            }
        }

        public List<LevelEditorChest> CollectChestFromRoom(int roomIndex)
        {
            var result = new List<LevelEditorChest>();
            rooms[roomIndex].GetComponentsInChildren(result);

            return result;
        }


        public void SelectGameObject(GameObject selectedGameObject)
        {
            Selection.activeGameObject = selectedGameObject;
        }


        public void Clear()
        {
            rooms.Clear();

            for (int i = container.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(container.transform.GetChild(i).gameObject);
            }
        }

        public void OnDrawGizmos()
        {
            if(selectedRoom < rooms.Count)
            {
                Gizmos.DrawWireSphere(rooms[selectedRoom].transform.position + spawnPoint, startPointSphereSize);
            }
            
        }


#endif
    }
}