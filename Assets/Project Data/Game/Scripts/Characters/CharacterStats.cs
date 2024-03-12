using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class CharacterStats
    {
        [SerializeField] int health;
        public int Health => health;

        [Space]
        [SerializeField] float bulletDamageMultiplier = 1.0f;
        public float BulletDamageMultiplier => bulletDamageMultiplier;

        [SerializeField] int power;
        public int Power => power;

        // key upgrade - "ideal" way to play the game, based on this upgrades sequence is built economy
        [SerializeField] int keyUpgradeNumber;
        public int KeyUpgradeNumber => keyUpgradeNumber;
    }
}