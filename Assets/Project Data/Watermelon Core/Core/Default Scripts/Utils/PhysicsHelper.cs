using UnityEngine;

namespace Watermelon
{
    public static class PhysicsHelper
    {
        public static readonly int LAYER_INTERACTABLE = LayerMask.NameToLayer("Interactable");
        public static readonly int LAYER_GROUND = LayerMask.NameToLayer("Ground");
        public static readonly int LAYER_OBSTACLE = LayerMask.NameToLayer("Obstacle");
        public static readonly int LAYER_DEFAULT = LayerMask.NameToLayer("Default");
        public static readonly int LAYER_SPAWN_OBJECT = LayerMask.NameToLayer("SpawnObject");
        public static readonly int LAYER_ENEMY = LayerMask.NameToLayer("Enemy");
        public static readonly int LAYER_PLAYER = LayerMask.NameToLayer("Player");

        public const string TAG_PLAYER = "Player";
        public const string TAG_ENEMY = "Enemy";
        public const string TAG_ITEM = "Item";
        public const string TAG_CHEST = "Chest";
        public const string TAG_GRAB_POINT = "GrabPoint";

        public static void Init()
        {

        }
    }
}