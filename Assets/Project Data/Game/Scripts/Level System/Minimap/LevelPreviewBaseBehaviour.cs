using UnityEngine;

namespace Watermelon.SquadShooter
{
    public abstract class LevelPreviewBaseBehaviour : MonoBehaviour
    {
        public abstract void Initialise();
        public abstract void Activate(bool animate = false);
        public abstract void Lock();
        public abstract void Complete();
    }
}