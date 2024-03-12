using System.Collections;
using System.Linq;
using UnityEngine;

namespace Watermelon
{
    public class SparksUIAnimation : MonoBehaviour
    {
        [SerializeField] GameObject sparkPrefab;
        [SerializeField] RectTransform[] sparkPositions;

        private Pool sparkPool;
        private Coroutine sparksCoroutine;


        private void Start()
        {
            sparkPool = PoolManager.AddPool(new PoolSettings( "UI Spark", sparkPrefab, 5, true));

            for (int i = 0; i < sparkPositions.Length; i++)
            {
                sparkPositions[i].gameObject.SetActive(false);
            }
        }

        public void StartAnimation()
        {
            if (sparkPositions.Length > 0)
                sparksCoroutine = StartCoroutine(SparkAnimation());
        }

        public void StopAnimation()
        {
            if (sparksCoroutine != null)
                StopCoroutine(sparksCoroutine);
        }

        private IEnumerator SparkAnimation()
        {
            WaitForSeconds waitForSeconds;

            RectTransform[] tempSparkObjects;

            while (true)
            {
                waitForSeconds = new WaitForSeconds(UnityEngine.Random.Range(0.2f, 0.5f));

                tempSparkObjects = sparkPositions.Where(x => !x.gameObject.activeSelf).ToArray();
                if (!tempSparkObjects.IsNullOrEmpty())
                {
                    RectTransform parentSpark = tempSparkObjects.GetRandomItem();
                    parentSpark.gameObject.SetActive(true);

                    GameObject sparkObject = sparkPool.GetPooledObject();
                    sparkObject.gameObject.SetActive(true);
                    sparkObject.transform.SetParent(parentSpark);
                    sparkObject.transform.localPosition = Vector3.zero;
                    sparkObject.transform.localScale = Vector3.zero;
                    sparkObject.transform.localRotation = Quaternion.identity;

                    sparkObject.transform.DOScale(UnityEngine.Random.Range(0.4f, 1.2f), 0.5f).SetEasing(Ease.Type.CircOut).OnComplete(delegate
                    {
                        sparkObject.transform.DOScale(0, 0.4f).SetEasing(Ease.Type.CircIn).OnComplete(delegate
                        {
                            sparkObject.SetActive(false);
                            sparkObject.transform.SetParent(null);

                            parentSpark.gameObject.SetActive(false);
                        });
                    });
                }

                yield return waitForSeconds;
            }
        }
    }
}