using UnityEngine;

namespace OrbitalDefense
{
    [RequireComponent(typeof(EnemyMover))]
    public sealed class CoreDamageOnReach : MonoBehaviour
    {
        [SerializeField] private float reachRadius = 0.35f;
        [SerializeField] private Transform target;
        [SerializeField] private CoreIntegrity coreIntegrity;
        [SerializeField] private CommandCoreUpgrade commandCoreUpgrade;

        private EnemyMover enemyMover;

        private void Awake()
        {
            enemyMover = GetComponent<EnemyMover>();
            coreIntegrity ??= FindAnyObjectByType<CoreIntegrity>();
            commandCoreUpgrade ??= FindAnyObjectByType<CommandCoreUpgrade>();
        }

        public void Initialize(Transform targetTransform, CoreIntegrity core)
        {
            target = targetTransform;
            coreIntegrity = core;
        }

        private void Update()
        {
            if (target == null || coreIntegrity == null || enemyMover == null || enemyMover.Config == null)
            {
                return;
            }

            if (Vector3.Distance(transform.position, target.position) > reachRadius)
            {
                return;
            }

            if (commandCoreUpgrade != null && commandCoreUpgrade.TryBlockImpact())
            {
                VisualEffectSpawner.SpawnCoreDamage(target.position);
                AudioService.PlayShieldBlock();
                Destroy(gameObject);
                return;
            }

            coreIntegrity.TakeDamage(enemyMover.Config.CoreDamage);
            VisualEffectSpawner.SpawnCoreDamage(target.position);
            AudioService.PlayCoreDamage();
            Destroy(gameObject);
        }
    }
}
