using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public class RewardedChestBehavior : AbstractChestBehavior
    {
        protected static readonly int IS_OPEN_HASH = Animator.StringToHash("IsOpen");

        [SerializeField] Animator rvAnimator;
        [SerializeField] Button rvButton;
        [SerializeField] Transform adHolder;
        [SerializeField] Canvas adCanvas;

        private void Awake()
        {
            rvButton.onClick.AddListener(OnButtonClick);
            adHolder.transform.localScale = Vector3.zero;
        }

        private void LateUpdate()
        {
            adCanvas.transform.forward = Camera.main.transform.forward;
        }

        public override void Init(List<DropData> drop)
        {
            base.Init(drop);

            rvAnimator.transform.localScale = Vector3.zero;

            isRewarded = true;
        }

        public override void ChestApproached()
        {
            if (opened)
                return;

            animatorRef.SetTrigger(SHAKE_HASH);
            rvAnimator.SetBool(IS_OPEN_HASH, true);
        }

        public override void ChestLeft()
        {
            if (opened)
                return;

            animatorRef.SetTrigger(IDLE_HASH);
            rvAnimator.SetBool(IS_OPEN_HASH, false);
        }

        private void OnButtonClick()
        {
            RewardChecker.Instance.unityEvent.AddListener(ADDListener);
            YG.YandexGame.Instance._RewardedShow(0);
        }

        public void ADDListener()
        {
            opened = true;

            animatorRef.SetTrigger(OPEN_HASH);
            rvAnimator.SetBool(IS_OPEN_HASH, false);

            Tween.DelayedCall(0.3f, () =>
            {
                DropResources();
                particle.SetActive(false);
                Vibration.Vibrate(AudioController.Vibrations.shortVibration);
            });

            RewardChecker.Instance.unityEvent.RemoveAllListeners();
        }
    }
}