using UnityEngine;

namespace OrbitalDefense
{
    public sealed class TurretWeapon : MonoBehaviour
    {
        [SerializeField] private Building building;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform muzzle;
        [SerializeField] private LayerMask enemyMask = ~0;
        [SerializeField] private RunModifiers modifiers;

        private float cooldown;

        private void Awake()
        {
            building ??= GetComponent<Building>();
            muzzle ??= transform;
            modifiers ??= FindFirstObjectByType<RunModifiers>();
        }

        private void Update()
        {
            if (building == null || building.Config == null || projectilePrefab == null)
            {
                return;
            }

            cooldown -= Time.deltaTime;
            if (cooldown > 0f)
            {
                return;
            }

            Health target = FindTarget();
            if (target == null)
            {
                return;
            }

            Fire(target);
            float boostedFireRate = building.Config.FireRate * BoosterAura.GetFireRateMultiplier(transform.position);
            cooldown = 1f / Mathf.Max(0.1f, boostedFireRate);
        }

        private Health FindTarget()
        {
            float rangeMultiplier = modifiers != null ? modifiers.CannonRangeMultiplier : 1f;
            float range = building.Config.Range * building.LevelMultiplier * rangeMultiplier;
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

        private void Fire(Health target)
        {
            Projectile projectile = Instantiate(projectilePrefab, muzzle.position, Quaternion.identity);
            float damageMultiplier = modifiers != null ? modifiers.CannonDamageMultiplier : 1f;
            projectile.Launch(target, Mathf.RoundToInt(building.Config.Damage * building.LevelMultiplier * damageMultiplier), building.Config.ProjectileSpeed);
        }
    }
}
