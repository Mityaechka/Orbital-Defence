using UnityEngine;

namespace OrbitalDefense
{
    [RequireComponent(typeof(Health))]
    public sealed class EnemyMover : MonoBehaviour
    {
        [SerializeField] private EnemyConfig config;
        [SerializeField] private Transform target;

        public EnemyConfig Config => config;

        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        public void Initialize(EnemyConfig enemyConfig, Transform targetTransform)
        {
            config = enemyConfig;
            target = targetTransform;

            if (health == null)
            {
                health = GetComponent<Health>();
            }

            if (config != null)
            {
                health.Configure(config.Health);
            }
        }

        private void Update()
        {
            if (target == null || config == null)
            {
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, target.position, config.Speed * Time.deltaTime);
        }
    }
}

