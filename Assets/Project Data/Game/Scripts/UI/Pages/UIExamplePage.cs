using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UIExamplePage : UIPage
    {
        [SerializeField] Image backgroundImage;

        private RectTransform pageRectTransform;

        private Color defaultBackgroundColor;

        private UIGame gamePage;

        public override void Initialise()
        {
            // Cache variables
            pageRectTransform = (RectTransform)transform;
            defaultBackgroundColor = backgroundImage.color;

            // Or get them from other components
            gamePage = UIController.GetPage<UIGame>();
        }

        public override void PlayShowAnimation()
        {
            // Reset components
            backgroundImage.color = defaultBackgroundColor.SetAlpha(0.0f);

            // Play animation
            backgroundImage.DOColor(defaultBackgroundColor, 0.3f).OnComplete(delegate
            {
                // IMPORTANT
                // For the correct work of UIController events you should call UIController.OnPageOpened(this) method right after page animations are completed
                UIController.OnPageOpened(this);
            });
        }

        public override void PlayHideAnimation()
        {
            // Play animation
            backgroundImage.DOFade(0.0f, 0.3f).OnComplete(delegate
            {
                // IMPORTANT
                // For the correct work of UIController events you should call UIController.OnPageClosed(this) method right after page animations are completed
                UIController.OnPageClosed(this);
            });
        }

        #region Buttons
        public void OnCloseButtonClicked()
        {
            // or instead you can use UIController static method and add OnPageHidded callback
            UIController.HidePage<UIExamplePage>(() =>
            {
                // Is called when UIExamplePage page is hidden
            });
        }
        #endregion
    }
}
