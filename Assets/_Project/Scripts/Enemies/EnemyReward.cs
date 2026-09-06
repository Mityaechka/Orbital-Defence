using UnityEngine;

namespace OrbitalDefense
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(EnemyMover))]
    public sealed class EnemyReward : MonoBehaviour
    {
        [SerializeField] private ResourceWallet wallet;

        private Health health;
        private EnemyMover enemyMover;

        private void Awake()
        {
            health = GetComponent<Health>();
            enemyMover = GetComponent<EnemyMover>();
            wallet ??= FindFirstObjectByType<ResourceWallet>();
        }

        private void OnEnable()
        {
            health.Died += HandleDied;
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= HandleDied;
            }
        }

        private void HandleDied(Health _)
        {
            if (wallet != null && enemyMover != null && enemyMover.Config != null)
            {
                wallet.Add(enemyMover.Config.Reward);
            }
        }
    }
}

