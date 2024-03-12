using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.LevelSystem;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class UIGame : UIPage
    {
        [SerializeField] Joystick joystick;
        [SerializeField] RectTransform floatingTextHolder;

        [Space]
        [SerializeField] TextMeshProUGUI areaText;

        [Space]
        [SerializeField] Transform roomsHolder;
        [SerializeField] GameObject roomIndicatorUIPrefab;

        [Space]
        [SerializeField] Image fadeImage;
        [SerializeField] TextMeshProUGUI coinsText;

        [Space]
        [SerializeField] BlackFadeBehavior blackFade;
        public BlackFadeBehavior BlackFade => blackFade;

        public Joystick Joystick => joystick;
        public RectTransform FloatingTextHolder => floatingTextHolder;

        private List<UIRoomIndicator> roomIndicators = new List<UIRoomIndicator>();
        private PoolGeneric<UIRoomIndicator> roomIndicatorsPool;

        private void Awake()
        {
            roomIndicatorsPool = new PoolGeneric<UIRoomIndicator>(new PoolSettings(roomIndicatorUIPrefab.name, roomIndicatorUIPrefab, 3, true, roomsHolder));
        }

        public void FadeAnimation(float time, float startAlpha, float targetAlpha, Ease.Type easing, SimpleCallback callback, bool disableOnComplete = false)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = fadeImage.color.SetAlpha(startAlpha);
            fadeImage.DOFade(targetAlpha, time).SetEasing(easing).OnComplete(delegate
            {
                callback?.Invoke();

                if (disableOnComplete)
                    fadeImage.gameObject.SetActive(false);
            });
        }

        public override void Initialise()
        {
            joystick.Initialise(UIController.MainCanvas);
        }

        public override void PlayHideAnimation()
        {
            OverlayUI.HideOverlay();

            UIController.OnPageClosed(this);
        }

        public override void PlayShowAnimation()
        {
            OverlayUI.HideOverlay();

            UIController.OnPageOpened(this);

            blackFade.SlideReveal();

            UIMainMenu.DotsBackground.gameObject.SetActive(false);
        }

        public void InitRoomsUI(RoomData[] rooms)
        {
            roomIndicatorsPool.ReturnToPoolEverything();
            roomIndicators.Clear();

            for (int i = 0; i < rooms.Length; i++)
            {
                roomIndicators.Add(roomIndicatorsPool.GetPooledComponent());
                roomIndicators[i].Init();

                if (i == 0)
                    roomIndicators[i].SetAsReached();
            }

            areaText.text = LevelController.GetCurrentAreaText();
        }

        public void UpdateReachedRoomUI(int roomReachedIndex)
        {
            roomIndicators[roomReachedIndex % roomIndicators.Count].SetAsReached();
        }

        public void UpdateCoinsText(int newAmount)
        {
            coinsText.text = CurrenciesHelper.Format(newAmount);
        }
    }
}