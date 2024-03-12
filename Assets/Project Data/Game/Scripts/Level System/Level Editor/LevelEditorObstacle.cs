#pragma warning disable 649
using UnityEngine;

namespace Watermelon.LevelSystem
{
    public class LevelEditorObstacle : MonoBehaviour
    {
        public LevelObstaclesType type;

        [Button]
        public void MirrorX()
        {
            GameObject spawnedObject = Instantiate(gameObject, transform.parent);
            spawnedObject.transform.localPosition = new Vector3(-transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
        }

        [Button]
        public void MirrorZ()
        {
            GameObject spawnedObject = Instantiate(gameObject, transform.parent);
            spawnedObject.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -transform.localPosition.z);
        }
    }
}