using System.Collections.Generic;
using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public abstract class AbstractChestBehavior : MonoBehaviour
    {
        protected static readonly int IDLE_HASH = Animator.StringToHash("Idle");
        protected static readonly int SHAKE_HASH = Animator.StringToHash("Shake");
        protected static readonly int OPEN_HASH = Animator.StringToHash("Open");

        [SerializeField] protected Animator animatorRef;
        [SerializeField] protected GameObject particle;

        public delegate void OnChestOpenedCallback(AbstractChestBehavior chest);

        protected List<DropData> dropData;
        protected DuoInt itemsAmountRange;

        public static event OnChestOpenedCallback OnChestOpenedEvent;

        protected bool opened;
        protected bool isRewarded;

        public virtual void Init(List<DropData> drop)
        {
            opened = false;
            dropData = drop;
            particle.SetActive(true);

            animatorRef.SetTrigger(IDLE_HASH);

            itemsAmountRange = new DuoInt(9, 11);
        }

        public abstract void ChestApproached();
        public abstract void ChestLeft();

        protected void DropResources()
        {
            if (!LevelController.IsGameplayActive)
                return;

            Vector3 dropCenter = transform.position + Vector3.forward * -3f;

            if (!dropData.IsNullOrEmpty())
            {
                for (int i = 0; i < dropData.Count; i++)
                {
                    if (dropData[i].dropType == DropableItemType.Currency)
                    {
                        int itemsAmount = Mathf.Clamp(itemsAmountRange.Random(), 1, dropData[i].amount);

                        List<int> itemValues = LevelController.SplitIntEqually(dropData[i].amount, itemsAmount);

                        for (int j = 0; j < itemsAmount; j++)
                        {
                            Tween.DelayedCall(i * 0.05f, () =>
                            {
                                var data = dropData[i].Clone();
                                data.amount = itemValues[j];

                                Drop.DropItem(data, dropCenter, Vector3.zero.SetY(Random.Range(0f, 360f)), DropFallingStyle.Coin, itemValues[j], 0.5f, rewarded: isRewarded);
                            });
                        }

                        AudioController.PlaySound(AudioController.Sounds.chestOpen, 1f);
                    }
                    else if (dropData[i].dropType == DropableItemType.WeaponCard)
                    {
                        for (int j = 0; j < dropData[i].amount; j++)
                        {
                            WeaponCardDropBehaviour card = Drop.DropItem(dropData[i], dropCenter, Vector3.zero, DropFallingStyle.Default, 1, 0.6f).GetComponent<WeaponCardDropBehaviour>();
                            card.SetCardData(dropData[i].cardType);
                        }
                    }
                    else
                    {
                        for (int j = 0; j < dropData[i].amount; j++)
                        {
                            Drop.DropItem(dropData[i], dropCenter, Vector3.zero.SetY(Random.Range(0f, 360f)), DropFallingStyle.Default, 1, 0.6f);
                        }
                    }
                }
            }

            OnChestOpenedEvent?.Invoke(this);
        }
    }
}