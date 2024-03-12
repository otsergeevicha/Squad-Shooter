using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    public abstract class BaseGunBehavior : MonoBehaviour
    {
        private static readonly int PARTICLE_UPGRADE = ParticlesController.GetHash("Gun Upgrade");

        [Header("Animations")]
        [SerializeField] AnimationClip characterShootAnimation;

        [Space]
        [SerializeField] Transform leftHandHolder;
        [SerializeField] Transform rightHandHolder;

        [Header("Upgrade")]
        [SerializeField] Vector3 upgradeParticleOffset;
        [SerializeField] float upgradeParticleSize = 1.0f;

        protected CharacterBehaviour characterBehaviour;
        protected WeaponData data;

        protected DuoInt damage;
        public DuoInt Damage => damage;

        private Transform leftHandRigController;
        private Transform rightHandRigController;

        public virtual void Initialise(CharacterBehaviour characterBehaviour, WeaponData data)
        {
            this.characterBehaviour = characterBehaviour;
            this.data = data;
        }

        public void InitialiseCharacter(BaseCharacterGraphics characterGraphics)
        {
            leftHandRigController = characterGraphics.LeftHandRig.data.target;
            rightHandRigController = characterGraphics.RightHandRig.data.target;

            characterGraphics.SetShootingAnimation(characterShootAnimation);
        }

        public virtual void OnLevelLoaded()
        {

        }

        public virtual void GunUpdate()
        {

        }

        public void UpdateHandRig()
        {
            leftHandRigController.position = leftHandHolder.position;
            rightHandRigController.position = rightHandHolder.position;

            leftHandRigController.rotation = leftHandHolder.rotation;
            rightHandRigController.rotation = rightHandHolder.rotation;
        }

        public abstract void Reload();
        public abstract void OnGunUnloaded();
        public abstract void PlaceGun(BaseCharacterGraphics characterGraphics);

        public abstract void SetGraphicsState(bool state);
        public abstract void RecalculateDamage();

        public AnimationClip GetShootAnimationClip()
        {
            return characterShootAnimation;
        }

        public virtual void PlayBounceAnimation()
        {
            transform.localScale = Vector3.one * 0.6f;
            transform.DOScale(Vector3.one, 0.4f).SetEasing(Ease.Type.BackOut);
        }

        public void SetDamage(DuoInt damage)
        {
            this.damage = damage;
        }

        public void SetDamage(int minDamage, int maxDamage)
        {
            damage = new DuoInt(minDamage, maxDamage);
        }

        public void PlayUpgradeParticle()
        {
            ParticlesController.PlayParticle(PARTICLE_UPGRADE).SetPosition(transform.position + upgradeParticleOffset).SetRotation(transform.rotation).SetScale(upgradeParticleSize.ToVector3());
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(transform.position + upgradeParticleOffset, upgradeParticleSize.ToVector3());
        }
    }
}