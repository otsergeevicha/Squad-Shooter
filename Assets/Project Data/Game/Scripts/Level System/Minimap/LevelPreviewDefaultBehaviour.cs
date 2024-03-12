using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public class LevelPreviewDefaultBehaviour : LevelPreviewBaseBehaviour
    {
        [SerializeField] Image circleDefaultImage;
        [SerializeField] Image circleLockedImage;
        [SerializeField] Image selectedImage;

        public override void Initialise()
        {

        }

        public override void Activate(bool animate = false)
        {
            selectedImage.gameObject.SetActive(true);
            circleDefaultImage.gameObject.SetActive(true);

            circleLockedImage.gameObject.SetActive(false);
        }

        public override void Complete()
        {
            circleDefaultImage.gameObject.SetActive(true);

            selectedImage.gameObject.SetActive(false);
            circleLockedImage.gameObject.SetActive(false);
        }

        public override void Lock()
        {
            circleLockedImage.gameObject.SetActive(true);

            circleDefaultImage.gameObject.SetActive(false);
            selectedImage.gameObject.SetActive(false);
        }
    }
}
