using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    [RequireComponent(typeof(Canvas)), RequireComponent(typeof(GraphicRaycaster))]
    public abstract class UIPage : MonoBehaviour
    {
        protected bool isPageDisplayed;
        public bool IsPageDisplayed { get => isPageDisplayed; set => isPageDisplayed = value; }

        protected Canvas canvas;
        public Canvas Canvas => canvas;

        protected GraphicRaycaster graphicRaycaster;
        public GraphicRaycaster GraphicRaycaster => graphicRaycaster;

        public void CacheComponents()
        {
            canvas = GetComponent<Canvas>();
            graphicRaycaster = GetComponent<GraphicRaycaster>();
        }

        public abstract void Initialise();

        public void EnableCanvas()
        {
            isPageDisplayed = true;

            canvas.enabled = true;
        }

        public void DisableCanvas()
        {
            isPageDisplayed = false;

            canvas.enabled = false;

            UIController.SetGameUIInputState(true);
        }

        public abstract void PlayShowAnimation();
        public abstract void PlayHideAnimation();

        public virtual void Unload()
        {
            isPageDisplayed = false;

            canvas.enabled = false;
        }
    }
}