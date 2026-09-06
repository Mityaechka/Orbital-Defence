using UnityEngine;

namespace OrbitalDefense
{
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 3f;

        private Health target;
        private Vector3 lastKnownTargetPosition;
        private int damage;
        private float speed;
        private float age;

        public void Launch(Health newTarget, int projectileDamage, float projectileSpeed)
        {
            target = newTarget;
            damage = projectileDamage;
            speed = projectileSpeed;
            lastKnownTargetPosition = target != null ? target.transform.position : transform.position;
            age = 0f;
        }

        private void Update()
        {
            age += Time.deltaTime;
            if (age >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            if (target != null)
            {
                lastKnownTargetPosition = target.transform.position;
            }

            transform.position = Vector3.MoveTowards(transform.position, lastKnownTargetPosition, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, lastKnownTargetPosition) <= 0.05f)
            {
                if (target != null)
                {
                    target.TakeDamage(damage);
                }

                VisualEffectSpawner.SpawnHit(lastKnownTargetPosition);
                Destroy(gameObject);
            }
        }
    }
}
