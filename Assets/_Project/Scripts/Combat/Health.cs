using System;
using UnityEngine;

namespace OrbitalDefense
{
    public sealed class Health : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 20;

        public event Action<Health> Died;
        public event Action<int, int> Changed;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;

        private void Awake()
        {
            CurrentHealth = Mathf.Max(1, maxHealth);
        }

        private void Start()
        {
            Changed?.Invoke(CurrentHealth, MaxHealth);
        }

        public void Configure(int health)
        {
            maxHealth = Mathf.Max(1, health);
            CurrentHealth = maxHealth;
            Changed?.Invoke(CurrentHealth, MaxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || CurrentHealth <= 0)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            VisualEffectSpawner.SpawnEnemyDamage(amount, transform.position);
            Changed?.Invoke(CurrentHealth, MaxHealth);

            if (CurrentHealth == 0)
            {
                VisualEffectSpawner.SpawnDeath(transform.position);
                Died?.Invoke(this);
                Destroy(gameObject);
            }
        }
    }
}
