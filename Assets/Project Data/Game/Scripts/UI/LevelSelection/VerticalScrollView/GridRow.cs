using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class GridRow : Image
    {
        public int Id { get; private set; }

        private List<GameObject> items;

        protected override void Awake()
        {
            color = Color.clear;

            rectTransform.pivot = new Vector2(0.5f, 1);

            rectTransform.anchorMin = new Vector2(0.5f, 1);
            rectTransform.anchorMax = new Vector2(0.5f, 1);

            raycastTarget = false;
            maskable = true;
        }

        public void Init(Vector2 childSize, float spacing, int itemsAmount, int firstItemId, Pool itemsPool, int id, int lastItemId)
        {
            Id = id;

            rectTransform.sizeDelta = new Vector2(
                itemsAmount * childSize.x + (itemsAmount - 1) * spacing,
                childSize.y);

            float leftBorder = -(itemsAmount * childSize.x + (itemsAmount - 1) * spacing) / 2;

            items = new List<GameObject>();

            for (int i = 0; i < itemsAmount; i++)
            {
                if (id * itemsAmount + i >= lastItemId) break; 

                GridItem item = itemsPool.GetPooledObject().GetComponent<GridItem>();

                RectTransform itemRect = item.GetRectTransform();

                itemRect.transform.SetParent(rectTransform);
                itemRect.transform.localScale = Vector3.one;
                itemRect.transform.localRotation = Quaternion.identity;

                itemRect.pivot = new Vector2(0, 1);

                itemRect.anchorMin = new Vector2(0.5f, 1);
                itemRect.anchorMax = new Vector2(0.5f, 1);

                itemRect.sizeDelta = childSize;

                itemRect.anchoredPosition3D = new Vector3(leftBorder + i * (childSize.x + spacing), 0, 0);

                item.InitGridItem(firstItemId + i);

                items.Add(itemRect.gameObject);
            }
        }

        public void ReturnItemsToPool()
        {
            for(int i = 0; i < items.Count; i++)
            {
                items[i].SetActive(false);
                items[i].transform.SetParent(null);
            }
        }
    }

}