using UnityEngine;

namespace OrbitalDefense
{
    public sealed class CommandCoreWeapon : MonoBehaviour
    {
        [SerializeField] private CommandCoreUpgrade coreUpgrade;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform muzzle;
        [SerializeField] private LayerMask enemyMask = ~0;
        [SerializeField] private float baseRange = 2.4f;
        [SerializeField] private float rangePerLevel = 0.45f;
        [SerializeField] private float baseFireRate = 0.45f;
        [SerializeField] private float fireRatePerLevel = 0.20f;
        [SerializeField] private float projectileSpeed = 5.2f;
        [SerializeField] private int baseDamage = 4;
        [SerializeField] private int damagePerLevel = 3;

        private float cooldown;

        private void Awake()
        {
            coreUpgrade ??= GetComponent<CommandCoreUpgrade>();
            muzzle ??= transform;
        }

        private void Update()
        {
            int level = coreUpgrade != null ? coreUpgrade.DefenseLevel : 0;
            if (level <= 0 || projectilePrefab == null)
            {
                return;
            }

            cooldown -= Time.deltaTime;
            if (cooldown > 0f)
            {
                return;
            }

            Health target = FindTarget(level);
            if (target == null)
            {
                return;
            }

            Projectile projectile = Instantiate(projectilePrefab, muzzle.position, Quaternion.identity);
            projectile.Launch(target, baseDamage + level * damagePerLevel, projectileSpeed);
            AudioService.PlayCoreShot();
            cooldown = 1f / Mathf.Max(0.1f, baseFireRate + level * fireRatePerLevel);
        }

        private Health FindTarget(int level)
        {
            float range = baseRange + level * rangePerLevel;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, enemyMask);
            Health closest = null;
            float closestSqrDistance = float.PositiveInfinity;

            for (int i = 0; i < hits.Length; i++)
            {
                Health health = hits[i].GetComponentInParent<Health>();
                if (health == null || health.CurrentHealth <= 0)
                {
                    continue;
                }

                float sqrDistance = (health.transform.position - transform.position).sqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    closest = health;
                    closestSqrDistance = sqrDistance;
                }
            }

            return closest;
        }
    }
}
