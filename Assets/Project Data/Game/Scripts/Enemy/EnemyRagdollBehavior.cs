using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class EnemyRagdollBehavior : MonoBehaviour
    {
        private List<RigidbodyCase> rbCases;

        private void Awake()
        {
            var rigidbodies = new List<Rigidbody>();
            GetComponentsInChildren(rigidbodies);

            rbCases = new List<RigidbodyCase>();

            for (int i = 0; i < rigidbodies.Count; i++)
            {
                var rigidbody = rigidbodies[i];

                if (rigidbody.gameObject.layer != 14)
                    continue;

                var rbCase = new RigidbodyCase(rigidbodies[i]);
                rbCase.Disable();

                rbCases.Add(rbCase);
            }
        }

        public void Activate()
        {
            for (int i = 0; i < rbCases.Count; i++)
            {
                rbCases[i].Activate();
            }
        }

        public void ActivateWithForce(Vector3 point, float force, float radius)
        {
            for (int i = 0; i < rbCases.Count; i++)
            {
                rbCases[i].Activate();
                rbCases[i].AddForce(point, force, radius);
            }
        }

        public void Disable()
        {
            if (rbCases.IsNullOrEmpty())
                return;

            for (int i = 0; i < rbCases.Count; i++)
            {
                if (rbCases[i] != null && rbCases[i].rigidbody != null)
                    rbCases[i].Disable();
            }
        }

        public void Reset()
        {
            if (rbCases.IsNullOrEmpty())
                return;

            for (int i = 0; i < rbCases.Count; i++)
            {
                rbCases[i].Reset();
            }
        }

        private class RigidbodyCase
        {
            public Rigidbody rigidbody;
            public Collider collider;
            public Vector3 localPosition;
            public Quaternion localRotation;
            public Vector3 localScale;

            public RigidbodyCase(Rigidbody rigidbody)
            {
                this.rigidbody = rigidbody;

                collider = rigidbody.GetComponent<Collider>();

                localPosition = rigidbody.transform.localPosition;
                localRotation = rigidbody.transform.localRotation;
                localScale = rigidbody.transform.localScale;
            }

            public void Disable()
            {
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
                rigidbody.Sleep();

                if (collider != null)
                    collider.enabled = false;
            }

            public void Activate()
            {
                rigidbody.isKinematic = false;
                rigidbody.useGravity = true;
                rigidbody.WakeUp();

                if (collider != null)
                    collider.enabled = true;
            }

            public void AddForce(Vector3 point, float force, float radius)
            {
                rigidbody.AddExplosionForce(force, point, radius);
            }

            public void Reset()
            {
                rigidbody.transform.localPosition = localPosition;
                rigidbody.transform.localRotation = localRotation;
                rigidbody.transform.localScale = localScale;
            }
        }
    }
}