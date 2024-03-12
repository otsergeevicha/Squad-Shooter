using System.Collections.Generic;
using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    public class ObjectEnabler : MonoBehaviour
    {
        public List<GameObject> objects = new List<GameObject>();
        public float delay;

        public void Awake()
        {
            Tween.DelayedCall(delay, () =>
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    objects[i].SetActive(true);
                }
            });
        }
    }
}