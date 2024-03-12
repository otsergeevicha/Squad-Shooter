using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(Canvas))]
    public class OverlayUI : MonoBehaviour
    {
        [SerializeField] CurrencyUIPanelSimple coinsUI;

        private static Canvas canvas;

        public void Initialise()
        {
            canvas = GetComponent<Canvas>();

            coinsUI.Initialise();
        }

        public static void ShowOverlay()
        {
            canvas.enabled = true;
        }

        public static void HideOverlay()
        {
            canvas.enabled = false;
        }
    }
}