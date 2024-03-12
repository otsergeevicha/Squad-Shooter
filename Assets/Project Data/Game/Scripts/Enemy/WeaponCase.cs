using System;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    [Serializable]
    public class WeaponCase
    {
        public Transform weaponTransform;
        public Transform weaponHolderTransform;

        public Vector3 LocalPosition { get; set; }
        public Quaternion LocalRotation { get; set; }
        public Vector3 LocalScale { get; set; }

        public void Init()
        {
            if (weaponTransform != null)
                weaponTransform.gameObject.SetActive(true);
        }

        public void Activate()
        {
            if (weaponTransform != null)
                weaponTransform.gameObject.SetActive(false);
        }

        public void Reset()
        {
            if (weaponTransform != null)
                weaponTransform.gameObject.SetActive(true);
        }
    }
}