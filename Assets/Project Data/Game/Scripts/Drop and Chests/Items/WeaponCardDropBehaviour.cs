using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class WeaponCardDropBehaviour : BaseDropBehaviour
    {
        [SerializeField] Collider triggerRef;
        [SerializeField] Image itemImage;
        [SerializeField] Image backImage;
        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] GameObject particleObject;
        [SerializeField] List<ParticleSystem> rarityParticles = new List<ParticleSystem>();

        public WeaponData Data { get; private set; }
        public WeaponType WeaponType { get; private set; }

        private TweenCase[] throwTweenCases;

        public override void Initialise(DropData dropData, float availableToPickDelay = -1, float autoPickDelay = -1, bool ignoreCollector = false)
        {
            this.dropData = dropData;
            this.availableToPickDelay = availableToPickDelay;
            this.autoPickDelay = autoPickDelay;

            isPicked = false;
            particleObject.transform.localScale = Vector3.zero;

            LevelController.OnPlayerExitLevelEvent += AutoPick;
            CharacterBehaviour.OnDied += ItemDisable;
        }

        public void SetCardData(WeaponType weaponType)
        {
            WeaponType = weaponType;

            Data = WeaponsController.GetWeaponData(weaponType);
            itemImage.sprite = Data.Icon;
            backImage.color = Data.RarityData.MainColor;
            titleText.text = Data.Name;

            for (int i = 0; i < rarityParticles.Count; i++)
            {
                var main = rarityParticles[i].main;
                main.startColor = Data.RarityData.MainColor.SetAlpha(main.startColor.color.a);
            }
        }

        private void AutoPick()
        {
            CharacterBehaviour.GetBehaviour().OnItemPicked(this);

            Pick(false);
            LevelController.OnPlayerExitLevelEvent -= AutoPick;
        }

        public override void Drop()
        {

        }

        public override void Throw(Vector3 position, AnimationCurve movemenHorizontalCurve, AnimationCurve movementVerticalCurve, float time)
        {
            throwTweenCases = new TweenCase[2];

            triggerRef.enabled = false;

            throwTweenCases[0] = transform.DOMoveXZ(position.x, position.z, time).SetCurveEasing(movemenHorizontalCurve);
            throwTweenCases[1] = transform.DOMoveY(position.y, time).SetCurveEasing(movementVerticalCurve).OnComplete(delegate
            {
                Tween.DelayedCall(availableToPickDelay, () =>
                {
                    triggerRef.enabled = true;
                });

                if (autoPickDelay != -1f)
                {
                    Tween.DelayedCall(autoPickDelay, () => Pick());
                }

                particleObject.transform.DOScale(7f, 0.2f).SetEasing(Ease.Type.SineOut);
            });
        }

        public override void Pick(bool moveToPlayer = true)
        {
            LevelController.OnPlayerExitLevelEvent -= AutoPick;

            if (isPicked)
                return;

            isPicked = true;

            // Kill movement tweens
            if (!throwTweenCases.IsNullOrEmpty())
            {
                for (int i = 0; i < throwTweenCases.Length; i++)
                {
                    if (throwTweenCases[i] != null && !throwTweenCases[i].isCompleted)
                    {
                        throwTweenCases[i].Kill();
                    }
                }
            }

            if (moveToPlayer)
            {
                transform.DOMove(CharacterBehaviour.Transform.position.SetY(4f), 0.3f).SetEasing(Ease.Type.SineIn).OnComplete(() =>
                {
                    ItemDisable();
                    AudioController.PlaySound(AudioController.Sounds.cardPickUp);
                });
            }
            else
            {
                ItemDisable();
            }
        }

        public void ItemDisable()
        {
            CharacterBehaviour.OnDied -= ItemDisable;
            gameObject.SetActive(false);
        }
    }
}