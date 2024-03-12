using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UIController : MonoBehaviour
    {
        private static UIController uiController;

        [SerializeField] UIPage[] pages;
        [SerializeField] FloatingCloud currencyCloud;
        [SerializeField] NotchSaveArea notchSaveArea;

        private static Dictionary<Type, UIPage> pagesLink = new Dictionary<Type, UIPage>();

        private static bool isTablet;
        public static bool IsTablet => isTablet;

        private static Canvas mainCanvas;
        public static Canvas MainCanvas => mainCanvas;
        public static CanvasScaler CanvasScaler { get; private set; }

        private static UIGame gamePage;
        public static UIGame GamePage => gamePage;

        private static Camera mainCamera;

        private static SimpleCallback localPageClosedCallback;

        public static event OnPageOpenedCallback OnPageOpenedEvent;
        public static event OnPageClosedCallback OnPageClosedEvent;

        public void Initialise()
        {
            uiController = this;

            mainCanvas = GetComponent<Canvas>();
            CanvasScaler = GetComponent<CanvasScaler>();

            isTablet = GetTabletStage();
            mainCamera = Camera.main;

            CanvasScaler.matchWidthOrHeight = isTablet ? 1 : 0;

            pagesLink = new Dictionary<Type, UIPage>();
            for (int i = 0; i < pages.Length; i++)
            {
                pages[i].CacheComponents();

                pagesLink.Add(pages[i].GetType(), pages[i]);
            }

            // Cache game page
            gamePage = (UIGame)pagesLink[typeof(UIGame)];
        }

        public void InitialisePages()
        {
            for (int i = 0; i < pages.Length; i++)
            {
                pages[i].Initialise();
                pages[i].DisableCanvas();
            }

            // Initialise currency cloud
            currencyCloud.Initialise();

            // Refresh notch save area
            notchSaveArea.Refresh();
        }

        public static void ResetPages()
        {
            UIController controller = uiController;
            if (controller != null)
            {
                for (int i = 0; i < controller.pages.Length; i++)
                {
                    if (controller.pages[i].IsPageDisplayed)
                    {
                        controller.pages[i].Unload();
                    }
                }
            }
        }

        public static void ShowPage<T>() where T : UIPage
        {
            Type pageType = typeof(T);
            UIPage page = pagesLink[pageType];
            if (!page.IsPageDisplayed)
            {
                page.PlayShowAnimation();
                page.EnableCanvas();
                page.GraphicRaycaster.enabled = true;
            }
        }

        public static void HidePage<T>(SimpleCallback onPageClosed = null)
        {
            Type pageType = typeof(T);
            UIPage page = pagesLink[pageType];
            if (page.IsPageDisplayed)
            {
                localPageClosedCallback = onPageClosed;

                page.GraphicRaycaster.enabled = false;
                page.PlayHideAnimation();
            }
            else
            {
                onPageClosed?.Invoke();
            }
        }

        public static void OnPageClosed(UIPage page)
        {
            page.DisableCanvas();

            OnPageClosedEvent?.Invoke(page, page.GetType());

            if (localPageClosedCallback != null)
            {
                localPageClosedCallback.Invoke();
                localPageClosedCallback = null;
            }
        }

        public static void OnPageOpened(UIPage page)
        {
            OnPageOpenedEvent?.Invoke(page, page.GetType());
        }

        public static T GetPage<T>() where T : UIPage
        {
            return pagesLink[typeof(T)] as T;
        }

        public static void SetGameUIInputState(bool state)
        {
            gamePage.GraphicRaycaster.enabled = state;
        }

        private static bool GetTabletStage()
        {
#if UNITY_IOS
            bool deviceIsIpad = UnityEngine.iOS.Device.generation.ToString().Contains("iPad");
            if (deviceIsIpad)
                return true;

            return false;
#else
            var aspectRatio = Mathf.Max(Screen.width, Screen.height) / (float)Mathf.Min(Screen.width, Screen.height);
            return (GetDeviceDiagonalSizeInInches() > 6.5f && aspectRatio < 1.7f);
#endif
        }

        public static float GetDeviceDiagonalSizeInInches()
        {
            float screenWidth = Screen.width / Screen.dpi;
            float screenHeight = Screen.height / Screen.dpi;
            float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));

            return diagonalInches;
        }

        public static Vector3 FixUIElementToWorld(Transform target, Vector3 offset)
        {
            Vector3 targPos = target.transform.position + offset;
            Vector3 camForward = mainCamera.transform.forward;

            float distInFrontOfCamera = Vector3.Dot(targPos - (mainCamera.transform.position + camForward), camForward);
            if (distInFrontOfCamera < 0f)
            {
                targPos -= camForward * distInFrontOfCamera;
            }

            return RectTransformUtility.WorldToScreenPoint(mainCamera, targPos);
        }

        #region Helpers
        [Button]
        public void RefreshSafeAreas()
        {
            if (!Application.isPlaying)
                return;

            // Refresh notch save area
            notchSaveArea.Refresh();
        }
        #endregion

        private void OnDestroy()
        {
            currencyCloud.Clear();
        }

        public delegate void OnPageOpenedCallback(UIPage page, Type pageType);
        public delegate void OnPageClosedCallback(UIPage page, Type pageType);
    }
}
