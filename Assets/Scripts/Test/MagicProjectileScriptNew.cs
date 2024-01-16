using UnityEngine;
using System.Collections;
using System.ComponentModel;
using UnityEngine.InputSystem.HID;

namespace MagicArsenal
{
    public class MagicProjectileScriptNew : MonoBehaviour
    {
        public GameObject impactParticle;
        public GameObject projectileParticle;
        public GameObject muzzleParticle;
        [SerializeField] private float destroyTime;

        private Rigidbody rb;

        private float destroyTimer = 0f;
        private bool destroyed = false;

        void Start()
        {
            rb = GetComponent<Rigidbody>();

            projectileParticle = Instantiate(projectileParticle, transform.position, transform.rotation) as GameObject;
            projectileParticle.transform.parent = transform;

            if (muzzleParticle)
            {
                muzzleParticle = Instantiate(muzzleParticle, transform.position, transform.rotation) as GameObject;

                Destroy(muzzleParticle, 1.5f); // Lifetime of muzzle effect.
            }
        }
		
        void FixedUpdate()
        {
            if (destroyed)
            {
                return;
            }
            // Increment the destroyTimer if the projectile hasn't hit anything.
            destroyTimer += Time.deltaTime;

            // Destroy the missile if the destroyTimer exceeds 5 seconds.
            if (destroyTimer >= 5f)
            {
                DestroyMissile();
            }

            RotateTowardsDirection();
        }
        private void OnTriggerEnter(Collider other)
        {
            GameObject impactP = Instantiate(impactParticle, transform.position, Quaternion.FromToRotation(Vector3.up, Vector3.up)) as GameObject;
            Destroy(projectileParticle, 3f);
            Destroy(impactP, 5.0f);
            DestroyMissile();
        }

        private void DestroyMissile()
        {
            destroyed = true;

            Destroy(projectileParticle, 3f);
            Destroy(gameObject);

            ParticleSystem[] trails = GetComponentsInChildren<ParticleSystem>();
            //Component at [0] is that of the parent i.e. this object (if there is any)
            for (int i = 1; i < trails.Length; i++)
            {
                ParticleSystem trail = trails[i];
                if (trail.gameObject.name.Contains("Trail"))
                {
                    trail.transform.SetParent(null);
                    Destroy(trail.gameObject, 2f);
                }
            }
        }

        private void RotateTowardsDirection()// 和訳:方向に回転
        {
            if (rb.velocity != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(rb.velocity.normalized, Vector3.up);
                float angle = Vector3.Angle(transform.forward, rb.velocity.normalized);
                float lerpFactor = angle * Time.deltaTime; // Use the angle as the interpolation factor
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lerpFactor);
            }
        }
    }
}
