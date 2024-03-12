using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Watermelon.SquadShooter;

namespace Watermelon
{
    [Serializable]
    public class VerticalGridScrollView : Image, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        private static readonly string GRID_ROW_POOL_NAME = "grid_row";
        private static Pool gridRowPool;

        [SerializeField] Vector2 childSize = new Vector2(200, 200);
        [SerializeField] Vector2 spacings = new Vector2(20, 20);

        [Space]
        [SerializeField] int columnsAmount = 4;

        [Space]
        [SerializeField] GameObject gridItem;
        [SerializeField] int initialItemsAmount = 20;
        [Space]
        [SerializeField] float rubberLength = 200;
        [SerializeField] float rubberPower = 50;

        [Space]
        [SerializeField] float inertion = 0.2f;

        private string gridItemPoolName;
        private Pool itemsPool;

        int rowsAmount;
        int itemsAmount;

        int visibleRowsAmount;
        float gridViewHeight;
        float childHeight;

        bool isDragging;
        float lastDragDelta;

        public GameObject GridItem
        {
            get => gridItem;
            set => SetGridItem(gridItem);
        }

        public Vector2 ChildSize
        {
            get => childSize;
            set => childSize = value;
        }

        public Vector2 Spacings
        {
            get => spacings;
            set => spacings = value;
        }

        private List<GridRow> rows;

        protected override void Awake()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return; // Editor boolshit
#endif
            color = Color.white;
            isMaskingGraphic = true;

            // Creating pool of rows, if it wasn't created earlier by another VerticalGridScrollView in the project

            if (gridRowPool == null)
            {
                GameObject gridRowObject = new GameObject("Grid Row");
                gridRowObject.AddComponent<RectTransform>();
                gridRowObject.AddComponent<GridRow>();

                gridRowPool = PoolManager.AddPool(new PoolSettings
                {
                    name = GRID_ROW_POOL_NAME,
                    objectsContainer = null,
                    singlePoolPrefab = gridRowObject,
                    size = 10,
                    autoSizeIncrement = true,
                    type = Pool.PoolType.Single
                });
            }

            // Creaating pool of items if it is not null

            gridItemPoolName = $"grid_item_{GetInstanceID()}";

            if(gridItem != null)
            {
                if (gridItem.GetComponent<GridItem>() == null)
                {
                    throw new GridScrollViewException("Object does not implement 'GridItem' interface");
                }

                if (PoolManager.PoolExists(gridItemPoolName) || itemsPool != null)
                {
                    throw new GridScrollViewException($"Pool '{gridItemPoolName}' already exists");
                }

                itemsPool = PoolManager.AddPool(new PoolSettings { 
                    name = gridItemPoolName,
                    objectsContainer = null,
                    singlePoolPrefab = gridItem,
                    size = initialItemsAmount,
                    autoSizeIncrement = true,
                    type = Pool.PoolType.Single
                });
            }


        }

        /// <summary>
        /// This method initializes Grid Scroll View with provided amount of items 
        /// </summary>
        /// <param name="itemsAmount">Amount of items</param>
        /// <param name="firstItem">Item id that should be visible</param>
        public void InitGrid(int itemsAmount, int firstItem = 0)
        {

            if (itemsPool == null)
            {
                throw new GridScrollViewException($"Pool '{gridItemPoolName}' does not exists");
            }

            if (firstItem >= itemsAmount) firstItem = itemsAmount - 1;
            if (firstItem < 0) firstItem = 0;

            int firstRowId = firstItem / columnsAmount;

            this.itemsAmount = itemsAmount;

            Clear();

            rows = new List<GridRow>();

            // Calculating data nessesary for initialization and scrolling

            rowsAmount = itemsAmount / columnsAmount;

            if (itemsAmount % columnsAmount != 0) rowsAmount++;

            childHeight = childSize.y + spacings.y;

            visibleRowsAmount = Mathf.CeilToInt(rectTransform.rect.size.y / (childHeight)) + 1;

            gridViewHeight = rectTransform.rect.height;

            if(firstRowId + visibleRowsAmount > rowsAmount)
            {
                firstRowId = rowsAmount - visibleRowsAmount;
            }

            if (firstRowId < 0) firstRowId = 0;

            if (visibleRowsAmount > rowsAmount) { 
                visibleRowsAmount = rowsAmount;

                gridViewHeight = visibleRowsAmount * childHeight;

                firstRowId = 0;
            }

            // Initializing rows

            for(int i = 0; i < visibleRowsAmount; i++)
            {
                GridRow row = gridRowPool.GetPooledObject().GetComponent<GridRow>();

                row.transform.SetParent(rectTransform);
                row.transform.localScale = Vector3.one;
                row.transform.localRotation = Quaternion.identity;

                row.Init(childSize, spacings.x, columnsAmount, (i + firstRowId) * columnsAmount, itemsPool, i + firstRowId, itemsAmount);

                row.rectTransform.anchoredPosition3D = new Vector3(0, -(i * childHeight), 0);

                rows.Add(row);
            }
        }

        /// <summary>
        /// This method initializes grid item pool
        /// </summary>
        /// <param name="gridItem">Item of grid scroll view. Should implement GridItem interface</param>
        private void SetGridItem(GameObject gridItem)
        {
            if (gridItem.GetComponent<GridItem>() == null) throw new GridScrollViewException("Object does not implement 'GridItem' interface");

            if (itemsPool != null) throw new GridScrollViewException("Cannot change Griditem object after it has been initialized");

            itemsPool = PoolManager.AddPool(new PoolSettings
            {
                name = gridItemPoolName,
                objectsContainer = null,
                singlePoolPrefab = gridItem,
                size = initialItemsAmount,
                autoSizeIncrement = true,
                type = Pool.PoolType.Single
            });

            this.gridItem = gridItem;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isDragging = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isDragging = false;
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return; //suppress second execution when leaving Play-Mode
#endif

            //Rubber movement and inertion

            if (!isDragging && !rows.IsNullOrEmpty())
            {

                var first = rows[0];
                var last = rows[rows.Count - 1];

                // inertion

                if (Mathf.Abs(lastDragDelta) > 5)
                {
                    lastDragDelta *= 0.9f;
                }
                else
                {
                    lastDragDelta = 0;
                }


                // Top Rubber
                if (first.Id == 0)
                {
                    if(first.rectTransform.anchoredPosition.y < 0)
                    {
                        float rubberDelta = Ease.GetFunction(Ease.Type.SineIn).Interpolate(Mathf.InverseLerp(20, -rubberLength, first.rectTransform.anchoredPosition.y)) * rubberPower;

                        if (rubberDelta < 5) rubberDelta = 0;

                        lastDragDelta = Mathf.Lerp(lastDragDelta, rubberDelta, inertion);
                    }
                }


                // Bottom Rubber
                if(last.Id == rowsAmount - 1)
                {
                    float minPos = -gridViewHeight + last.rectTransform.rect.height;

                    if (last.rectTransform.anchoredPosition.y > minPos)
                    {
                        float rubberDelta = Ease.GetFunction(Ease.Type.SineIn).Interpolate(Mathf.InverseLerp(minPos - 20, minPos + rubberLength, last.rectTransform.anchoredPosition.y)) * -rubberPower;

                        if (rubberDelta > -5) rubberDelta = 0;

                        lastDragDelta = Mathf.Lerp(lastDragDelta, rubberDelta, inertion);
                    }
                }

                if (lastDragDelta != 0)
                {
                    Move(lastDragDelta);
                }
            }
        }

        
        public void OnDrag(PointerEventData eventData)
        {
            float yOffset = eventData.delta.y;

            lastDragDelta = Mathf.Lerp(lastDragDelta, yOffset, inertion);

            Move(yOffset);
        }


        /// <summary>
        /// This method scrolls Grid Scroll view verticaly 
        /// </summary>
        /// <param name="deltaY">scroll amount</param>
        private void Move(float deltaY)
        {
            var first = rows[0];
            var last = rows[rows.Count - 1];

            // Top constrain and rubber
            if (rows[0].Id == 0)
            {
                if (first.rectTransform.anchoredPosition.y + deltaY < 0 && deltaY < 0)
                {
                    deltaY *= Ease.GetFunction(Ease.Type.SineOut).Interpolate(Mathf.InverseLerp(-rubberLength, 0, first.rectTransform.anchoredPosition.y));

                    if (deltaY < -first.rectTransform.anchoredPosition.y - rubberLength) deltaY = -first.rectTransform.anchoredPosition.y - rubberLength;
                }
            }

            // Bottom constrain and rubber
            if (rows[rows.Count - 1].Id == rowsAmount - 1)
            {
                float minPos = -gridViewHeight + childHeight;

                if (last.rectTransform.anchoredPosition.y + deltaY > minPos && deltaY > 0)
                {
                    deltaY *= Ease.GetFunction(Ease.Type.SineOut).Interpolate(Mathf.InverseLerp(minPos + rubberLength, minPos, last.rectTransform.anchoredPosition.y));

                    if (last.rectTransform.anchoredPosition.y + deltaY > minPos + rubberLength) deltaY = minPos + rubberLength - last.rectTransform.anchoredPosition.y;
                }
            }
            
            // Actual movement
            for (int i = 0; i < rows.Count; i++)
            {
                rows[i].rectTransform.anchoredPosition += Vector2.up * deltaY;
            }

            // Moving top row to the bottom
            if(first.rectTransform.anchoredPosition.y - first.rectTransform.rect.height > 0 && deltaY > 0)
            {
                if(last.Id != rowsAmount - 1)
                {
                    rows.Remove(first);

                    first.ReturnItemsToPool();

                    int newId = last.Id + 1;

                    first.Init(childSize, spacings.x, columnsAmount, newId * columnsAmount, itemsPool, newId, itemsAmount);

                    first.rectTransform.anchoredPosition = Vector2.up * (last.rectTransform.anchoredPosition.y - childHeight);

                    rows.Add(first);
                }
            }

            // Moving bottom row to the top
            if(last.rectTransform.anchoredPosition.y < -rectTransform.rect.height && deltaY < 0)
            {
                if(first.Id != 0)
                {
                    rows.Remove(last);

                    last.ReturnItemsToPool();

                    int newId = first.Id - 1;

                    last.Init(childSize, spacings.x, columnsAmount, newId * columnsAmount, itemsPool, newId, itemsAmount);

                    last.rectTransform.anchoredPosition = Vector2.up * (first.rectTransform.anchoredPosition.y + childHeight);

                    rows.Insert(0, last);
                }
            }
        }


        /// <summary>
        /// This method cleares Grid Scroll view of all rows and items;
        /// </summary>
        public void Clear()
        {
            if (!rows.IsNullOrEmpty())
            {
                foreach(var row in rows)
                {
                    row.ReturnItemsToPool();

                    row.transform.SetParent(null);
                    row.gameObject.SetActive(false);
                }

                rows.Clear();
            }
        }

        private class GridScrollViewException : Exception
        {
            public GridScrollViewException() { }

            public GridScrollViewException(string message) : base(message) { }
        }
    }
}