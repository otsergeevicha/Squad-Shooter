using UnityEngine;

namespace Watermelon.LevelSystem
{
    public sealed class GateExitPointBehaviour : ExitPointBehaviour
    {
        private readonly int IDLE_HASH = Animator.StringToHash("Idle");
        private readonly int OPEN_HASH = Animator.StringToHash("Open");

        [SerializeField] Animator gatesAnimator;

        public override void Initialise()
        {
            gatesAnimator.Play(IDLE_HASH);
        }

        public override void OnExitActivated()
        {
            isExitActivated = true;

            gatesAnimator.Play(OPEN_HASH);

            ParticlesController.RingEffectController.SpawnEffect(transform.position.SetY(0.1f), 30, 2, Ease.Type.Linear);

            AudioController.PlaySound(AudioController.Sounds.complete);

            Tween.DelayedCall(0.15f, () =>
            {
                AudioController.PlaySound(AudioController.Sounds.door);
            });
        }

        public override void OnPlayerEnteredExit()
        {
            isExitActivated = false;
            LevelController.OnPlayerExitLevel();
        }

        public override void Unload()
        {

        }
    }
}