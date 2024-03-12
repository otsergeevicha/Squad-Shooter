using UnityEngine;

namespace Watermelon.LevelSystem
{
    [RequireComponent(typeof(BoxCollider))]
    public abstract class ExitPointBehaviour : MonoBehaviour
    {
        protected bool isExitActivated;

        public abstract void Initialise();
        public abstract void OnExitActivated();
        public abstract void OnPlayerEnteredExit();
        public abstract void Unload();

        private void OnTriggerEnter(Collider other)
        {
            if (!isExitActivated)
                return;

            if (other.gameObject.layer.Equals(PhysicsHelper.LAYER_PLAYER))
            {
                OnPlayerEnteredExit();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!isExitActivated)
                return;

            if (other.gameObject.layer.Equals(PhysicsHelper.LAYER_PLAYER))
            {
                OnPlayerEnteredExit();
            }
        }
    }
}