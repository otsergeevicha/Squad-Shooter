using UnityEngine;
using Watermelon;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    // basic drop item without any special behaviour
    // override this class to add extra data fields
    public class ItemDropBehaviour : BaseDropBehaviour
    {
        [SerializeField] Animator animator;
        [SerializeField] Collider triggerRef;
        [SerializeField] bool useAutoPickup = true;

        private TweenCase[] throwTweenCases;

        public override void Initialise(DropData dropData, float availableToPickDelay = -1, float autoPickDelay = -1, bool ignoreCollector = false)
        {
            this.dropData = dropData;
            this.availableToPickDelay = availableToPickDelay;
            this.autoPickDelay = autoPickDelay;

            isPicked = false;

            animator.enabled = false;

            CharacterBehaviour.OnDied += ItemDisable;
        }

        public override void Drop()
        {
            animator.enabled = true;
            triggerRef.enabled = true;
        }

        public override void Throw(Vector3 position, AnimationCurve movemenHorizontalCurve, AnimationCurve movementVerticalCurve, float time)
        {
            LevelController.OnPlayerExitLevelEvent += AutoPick;

            throwTweenCases = new TweenCase[2];

            triggerRef.enabled = false;

            throwTweenCases[0] = transform.DOMoveXZ(position.x, position.z, time).SetCurveEasing(movemenHorizontalCurve);
            throwTweenCases[1] = transform.DOMoveY(position.y, time).SetCurveEasing(movementVerticalCurve).OnComplete(delegate
            {
                animator.enabled = true;

                Tween.DelayedCall(availableToPickDelay, () =>
                {
                    triggerRef.enabled = true;
                });

                if (autoPickDelay != -1f)
                {
                    Tween.DelayedCall(autoPickDelay, () =>
                    {
                        Pick();
                        CharacterBehaviour.GetBehaviour().OnItemPicked(this);
                    });
                }
            });
        }

        private void AutoPick()
        {
            if (useAutoPickup)
            {
                CharacterBehaviour.GetBehaviour().OnItemPicked(this);

                Pick(false);
            }
            else
            {
                ItemDisable();
            }

            LevelController.OnPlayerExitLevelEvent -= AutoPick;
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

            animator.enabled = false;
            triggerRef.enabled = false;

            if (moveToPlayer)
            {
                transform.DOMove(CharacterBehaviour.Transform.position.SetY(4f), 0.3f).SetEasing(Ease.Type.SineIn).OnComplete(() =>
                {
                    ItemDisable();

                    if (dropData.dropType == DropableItemType.Currency)
                        AudioController.PlaySound(AudioController.Sounds.coinPickUp, 0.4f);
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