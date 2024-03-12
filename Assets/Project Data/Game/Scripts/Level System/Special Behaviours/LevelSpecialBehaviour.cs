using UnityEngine;

namespace Watermelon.LevelSystem
{
    public abstract class LevelSpecialBehaviour : ScriptableObject
    {
        public abstract void OnLevelInitialised();

        public abstract void OnLevelLoaded();
        public abstract void OnLevelUnloaded();

        public abstract void OnLevelStarted();
        public abstract void OnLevelFailed();
        public abstract void OnLevelCompleted();

        public abstract void OnRoomEntered();
        public abstract void OnRoomLeaved();
    }
}
