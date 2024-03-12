using UnityEngine;
using UnityEngine.Animations.Rigging;
using Watermelon;
using Watermelon.Outline;

namespace Watermelon.SquadShooter
{
    public abstract class BaseCharacterGraphics : MonoBehaviour
    {
        private static readonly int PARTICLE_UPGRADE = ParticlesController.GetHash("Upgrade");

        private readonly int ANIMATION_SHOT_HASH = Animator.StringToHash("Shot");
        private readonly int ANIMATION_HIT_HASH = Animator.StringToHash("Hit");

        private readonly int JUMP_ANIMATION_HASH = Animator.StringToHash("Jump");
        private readonly int GRUNT_ANIMATION_HASH = Animator.StringToHash("Grunt");

        [SerializeField]
        protected Animator characterAnimator;

        [Space]
        [SerializeField] SkinnedMeshRenderer meshRenderer;
        public SkinnedMeshRenderer MeshRenderer => meshRenderer;

        [Header("Movement")]
        [SerializeField] MovementSettings movementSettings;
        public MovementSettings MovementSettings => movementSettings;

        [SerializeField] MovementSettings movementAimingSettings;
        public MovementSettings MovementAimingSettings => movementAimingSettings;

        [Header("Hands Rig")]
        [SerializeField] TwoBoneIKConstraint leftHandRig;
        public TwoBoneIKConstraint LeftHandRig => leftHandRig;

        [SerializeField] TwoBoneIKConstraint rightHandRig;
        public TwoBoneIKConstraint RightHandRig => rightHandRig;

        [Header("Weapon")]
        [SerializeField] Transform minigunHolderTransform;
        public Transform MinigunHolderTransform => minigunHolderTransform;

        [SerializeField] Transform shootGunHolderTransform;
        public Transform ShootGunHolderTransform => shootGunHolderTransform;

        [SerializeField] Transform rocketHolderTransform;
        public Transform RocketHolderTransform => rocketHolderTransform;

        [SerializeField] Transform teslaHolderTransform;
        public Transform TeslaHolderTransform => teslaHolderTransform;

        [Space]
        [SerializeField] Rig mainRig;
        [SerializeField] Transform leftHandController;
        [SerializeField] Transform rightHandController;

        protected CharacterBehaviour characterBehaviour;
        protected CharacterAnimationHandler animationHandler;

        protected Material characterMaterial;
        public Material CharacterMaterial => characterMaterial;

        private int animatorShootingLayerIndex;

        private AnimatorOverrideController animatorOverrideController;

        private TweenCase rigWeightCase;

        public virtual void Initialise(CharacterBehaviour characterBehaviour)
        {
            this.characterBehaviour = characterBehaviour;

            animationHandler = characterAnimator.GetComponent<CharacterAnimationHandler>();
            animationHandler.Inititalise(characterBehaviour);

            animatorOverrideController = new AnimatorOverrideController(characterAnimator.runtimeAnimatorController);
            characterAnimator.runtimeAnimatorController = animatorOverrideController;

            characterMaterial = meshRenderer.sharedMaterial;

            animatorShootingLayerIndex = characterAnimator.GetLayerIndex("Shooting");
        }

        public abstract void OnMovingStarted();
        public abstract void OnMovingStoped();
        public abstract void OnMoving(float speedPercent, Vector3 direction, bool isTargetFound);

        public virtual void OnDeath() { }

        public void Jump()
        {
            characterAnimator.SetTrigger(JUMP_ANIMATION_HASH);

            rigWeightCase.KillActive();
            mainRig.weight = 0f;
        }

        public void Grunt()
        {
            characterAnimator.SetTrigger(GRUNT_ANIMATION_HASH);

            var strength = 0.5f;
            var durationIn = 0.1f;
            var durationOut = 0.15f;

            MinigunHolderTransform.DOMoveY(MinigunHolderTransform.position.y - strength, durationIn).SetEasing(Ease.Type.SineOut).OnComplete(() =>
            {
                MinigunHolderTransform.DOMoveY(MinigunHolderTransform.position.y + strength, durationOut).SetEasing(Ease.Type.SineInOut);
            });

            ShootGunHolderTransform.DOMoveY(ShootGunHolderTransform.position.y - strength, durationIn).SetEasing(Ease.Type.SineOut).OnComplete(() =>
            {
                ShootGunHolderTransform.DOMoveY(ShootGunHolderTransform.position.y + strength, durationOut).SetEasing(Ease.Type.SineInOut);
            });

            RocketHolderTransform.DOMoveY(RocketHolderTransform.position.y - strength, durationIn).SetEasing(Ease.Type.SineOut).OnComplete(() =>
            {
                RocketHolderTransform.DOMoveY(RocketHolderTransform.position.y + strength, durationOut).SetEasing(Ease.Type.SineInOut);
            });

            TeslaHolderTransform.DOMoveY(TeslaHolderTransform.position.y - strength, durationIn).SetEasing(Ease.Type.SineOut).OnComplete(() =>
            {
                TeslaHolderTransform.DOMoveY(TeslaHolderTransform.position.y + strength, durationOut).SetEasing(Ease.Type.SineInOut);
            });

            leftHandController.DOMoveY(leftHandController.position.y - strength, durationIn).SetEasing(Ease.Type.SineOut).OnComplete(() =>
            {
                leftHandController.DOMoveY(leftHandController.position.y + strength, durationOut).SetEasing(Ease.Type.SineInOut);
            });

            rightHandController.DOMoveY(rightHandController.position.y - strength, durationIn).SetEasing(Ease.Type.SineOut).OnComplete(() =>
            {
                rightHandController.DOMoveY(rightHandController.position.y + strength, durationOut).SetEasing(Ease.Type.SineInOut);
            });
        }

        public void EnableRig()
        {
            rigWeightCase = Tween.DoFloat(0, 1, 0.2f, (value) => mainRig.weight = value);
        }

        public abstract void CustomFixedUpdate();

        public void SetShootingAnimation(AnimationClip animationClip)
        {
            animatorOverrideController["Shot"] = animationClip;
        }

        public void OnShoot()
        {
            characterAnimator.Play(ANIMATION_SHOT_HASH, animatorShootingLayerIndex, 0);
        }

        public void PlayHitAnimation()
        {
            characterAnimator.SetTrigger(ANIMATION_HIT_HASH);
        }

        public void PlayBounceAnimation()
        {
            transform.localScale = Vector3.one * 0.6f;
            transform.DOScale(Vector3.one, 0.4f).SetEasing(Ease.Type.BackOut);
        }

        public void PlayUpgradeParticle()
        {
            ParticlesController.PlayParticle(PARTICLE_UPGRADE).SetPosition(transform.position).SetScale((5).ToVector3());
        }

        public abstract void Unload();

        public abstract void Reload();

        public abstract void Disable();

        public abstract void Activate();

#if UNITY_EDITOR
        [Button("Prepare Model")]
        public void PrepareModel()
        {
            // Get animator component
            Animator tempAnimator = characterAnimator;

            if (tempAnimator != null)
            {
                if (tempAnimator.avatar != null && tempAnimator.avatar.isHuman)
                {
                    // Initialise rig
                    RigBuilder rigBuilder = tempAnimator.GetComponent<RigBuilder>();
                    if (rigBuilder == null)
                    {
                        rigBuilder = tempAnimator.gameObject.AddComponent<RigBuilder>();

                        GameObject rigObject = new GameObject("Main Rig");
                        rigObject.transform.SetParent(tempAnimator.transform);
                        rigObject.transform.ResetLocal();

                        Rig rig = rigObject.AddComponent<Rig>();

                        mainRig = rig;

                        rigBuilder.layers.Add(new RigLayer(rig, true));

                        // Left hand rig
                        GameObject leftHandRigObject = new GameObject("Left Hand Rig");
                        leftHandRigObject.transform.SetParent(rigObject.transform);
                        leftHandRigObject.transform.ResetLocal();

                        GameObject leftHandControllerObject = new GameObject("Controller");
                        leftHandControllerObject.transform.SetParent(leftHandRigObject.transform);
                        leftHandControllerObject.transform.ResetLocal();

                        leftHandController = leftHandControllerObject.transform;

                        Transform leftHandBone = tempAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
                        leftHandControllerObject.transform.position = leftHandBone.position;
                        leftHandControllerObject.transform.rotation = leftHandBone.rotation;

                        TwoBoneIKConstraint leftHandRig = leftHandRigObject.AddComponent<TwoBoneIKConstraint>();
                        leftHandRig.data.target = leftHandControllerObject.transform;
                        leftHandRig.data.root = tempAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                        leftHandRig.data.mid = tempAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                        leftHandRig.data.tip = leftHandBone;

                        // Right hand rig
                        GameObject rightHandRigObject = new GameObject("Right Hand Rig");
                        rightHandRigObject.transform.SetParent(rigObject.transform);
                        rightHandRigObject.transform.ResetLocal();

                        GameObject rightHandControllerObject = new GameObject("Controller");
                        rightHandControllerObject.transform.SetParent(rightHandRigObject.transform);
                        rightHandControllerObject.transform.ResetLocal();

                        rightHandController = rightHandControllerObject.transform;

                        Transform rightHandBone = tempAnimator.GetBoneTransform(HumanBodyBones.RightHand);
                        rightHandControllerObject.transform.position = rightHandBone.position;
                        rightHandControllerObject.transform.rotation = rightHandBone.rotation;

                        TwoBoneIKConstraint rightHandRig = rightHandRigObject.AddComponent<TwoBoneIKConstraint>();
                        rightHandRig.data.target = rightHandControllerObject.transform;
                        rightHandRig.data.root = tempAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                        rightHandRig.data.mid = tempAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                        rightHandRig.data.tip = rightHandBone;

                        this.leftHandRig = leftHandRig;
                        this.rightHandRig = rightHandRig;
                    }

                    movementSettings.RotationSpeed = 8;
                    movementSettings.MoveSpeed = 32;
                    movementSettings.Acceleration = 5000;
                    movementSettings.AnimationMultiplier = new DuoFloat(0, 1.4f);

                    movementAimingSettings.RotationSpeed = 8;
                    movementAimingSettings.MoveSpeed = 28;
                    movementAimingSettings.Acceleration = 5000;
                    movementAimingSettings.AnimationMultiplier = new DuoFloat(0, 1.2f);

                    CharacterAnimationHandler tempAnimationHandler = tempAnimator.GetComponent<CharacterAnimationHandler>();
                    if (tempAnimationHandler == null)
                        tempAnimator.gameObject.AddComponent<CharacterAnimationHandler>();

                    Outlinable tempOutline = tempAnimator.GetComponent<Outlinable>();
                    if (tempOutline == null)
                        tempAnimator.gameObject.AddComponent<Outlinable>();

                    // Create weapon holders
                    // Minigun
                    GameObject miniGunHolderObject = new GameObject("Minigun Holder");
                    miniGunHolderObject.transform.SetParent(tempAnimator.transform);
                    miniGunHolderObject.transform.ResetLocal();
                    miniGunHolderObject.transform.localPosition = new Vector3(1.36f, 4.67f, 2.5f);

                    minigunHolderTransform = miniGunHolderObject.transform;

                    // Shotgun
                    GameObject shotgunHolderObject = new GameObject("Shotgun Holder");
                    shotgunHolderObject.transform.SetParent(tempAnimator.transform);
                    shotgunHolderObject.transform.ResetLocal();
                    shotgunHolderObject.transform.localPosition = new Vector3(1.12f, 4.49f, 1.09f);

                    shootGunHolderTransform = shotgunHolderObject.transform;

                    // Rocket
                    GameObject rocketHolderObject = new GameObject("Rocket Holder");
                    rocketHolderObject.transform.SetParent(tempAnimator.transform);
                    rocketHolderObject.transform.ResetLocal();
                    rocketHolderObject.transform.localPosition = new Vector3(1.09f, 4.23f, 1.8f);
                    rocketHolderObject.transform.localRotation = Quaternion.Euler(-15, 0, 0);

                    rocketHolderTransform = rocketHolderObject.transform;

                    // Tesla
                    GameObject teslaHolderObject = new GameObject("Tesla Holder");
                    teslaHolderObject.transform.SetParent(tempAnimator.transform);
                    teslaHolderObject.transform.ResetLocal();
                    teslaHolderObject.transform.localPosition = new Vector3(1.42f, 5.22f, 2.38f);

                    teslaHolderTransform = teslaHolderObject.transform;

                    // Initialise mesh renderer
                    meshRenderer = tempAnimator.transform.GetComponentInChildren<SkinnedMeshRenderer>();

#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(this);
#endif
                }
                else
                {
                    Debug.LogError("Avatar is missing or type isn't humanoid!");
                }
            }
            else
            {
                Debug.LogWarning("Animator component can't be found!");
            }
        }
#endif
    }
}