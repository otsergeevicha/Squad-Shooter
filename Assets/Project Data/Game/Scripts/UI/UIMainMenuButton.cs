using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class UIMainMenuButton
    {
        [SerializeField] RectTransform rect;

        [SerializeField] AnimationCurve showStoreAdButtonsCurve;
        [SerializeField] AnimationCurve hideStoreAdButtonsCurve;
        [SerializeField] float showHideDuration;

        private float savedRectPosX;
        private float rectXPosBehindOfTheScreen;

        private TweenCase showHideCase;

        public void Init(float rectXPosBehindOfTheScreen)
        {
            this.rectXPosBehindOfTheScreen = rectXPosBehindOfTheScreen;
            savedRectPosX = rect.anchoredPosition.x;
        }

        public void Show(bool immediately = false)
        {
            if (showHideCase != null && showHideCase.isActive) return;

            if (immediately)
            {
                rect.anchoredPosition = rect.anchoredPosition.SetX(savedRectPosX);
                return;
            }

            //RESET
            rect.anchoredPosition = rect.anchoredPosition.SetX(rectXPosBehindOfTheScreen);

            showHideCase = rect.DOAnchoredPosition(rect.anchoredPosition.SetX(savedRectPosX), showHideDuration).SetCurveEasing(showStoreAdButtonsCurve);
        }

        public void Hide(bool immediately = false)
        {
            if (showHideCase != null && showHideCase.isActive) return;

            if (immediately)
            {
                rect.anchoredPosition = rect.anchoredPosition.SetX(rectXPosBehindOfTheScreen);
                return;
            }

            //RESET
            rect.anchoredPosition = rect.anchoredPosition.SetX(savedRectPosX);

            showHideCase = rect.DOAnchoredPosition(rect.anchoredPosition.SetX(rectXPosBehindOfTheScreen), showHideDuration).SetCurveEasing(hideStoreAdButtonsCurve);
        }
    }
}
