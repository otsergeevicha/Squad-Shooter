using TMPro;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    [RequireComponent(typeof(Animation))]
    public class TutorialLabelBehaviour : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI label;

        private Animation labelAnimation;

        private Transform parentTransform;
        private Vector3 offset;

        private void Awake()
        {
            labelAnimation = GetComponent<Animation>();
        }

        private void Update()
        {
            transform.position = parentTransform.position + offset;
        }

        public void Activate(string text, Transform parentTransform, Vector3 offset)
        {
            this.parentTransform = parentTransform;
            this.offset = offset;

            label.text = text;

            gameObject.SetActive(true);
            labelAnimation.enabled = true;
        }

        public void Disable()
        {
            gameObject.SetActive(false);
            labelAnimation.enabled = false;
        }
    }
}