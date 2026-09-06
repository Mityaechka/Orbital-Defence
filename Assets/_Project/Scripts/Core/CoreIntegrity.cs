using System;
using UnityEngine;

namespace OrbitalDefense
{
    public sealed class CoreIntegrity : MonoBehaviour
    {
        [SerializeField] private int maxIntegrity = 100;

        public event Action<int, int> IntegrityChanged;
        public event Action Depleted;

        public int CurrentIntegrity { get; private set; }
        public int MaxIntegrity => maxIntegrity;

        private void Awake()
        {
            CurrentIntegrity = Mathf.Max(1, maxIntegrity);
        }

        private void Start()
        {
            IntegrityChanged?.Invoke(CurrentIntegrity, MaxIntegrity);
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || CurrentIntegrity <= 0)
            {
                return;
            }

            CurrentIntegrity = Mathf.Max(0, CurrentIntegrity - amount);
            IntegrityChanged?.Invoke(CurrentIntegrity, MaxIntegrity);

            if (CurrentIntegrity == 0)
            {
                Depleted?.Invoke();
            }
        }

        public void Repair(int amount)
        {
            if (amount <= 0 || CurrentIntegrity <= 0)
            {
                return;
            }

            CurrentIntegrity = Mathf.Min(MaxIntegrity, CurrentIntegrity + amount);
            IntegrityChanged?.Invoke(CurrentIntegrity, MaxIntegrity);
        }

        public void RepairPercent(float percent)
        {
            Repair(Mathf.RoundToInt(MaxIntegrity * Mathf.Max(0f, percent) / 100f));
        }

        public void IncreaseMaxIntegrityPercent(float percent)
        {
            int increase = Mathf.RoundToInt(MaxIntegrity * Mathf.Max(0f, percent) / 100f);
            if (increase <= 0 || CurrentIntegrity <= 0)
            {
                return;
            }

            maxIntegrity += increase;
            CurrentIntegrity = Mathf.Min(MaxIntegrity, CurrentIntegrity + increase);
            IntegrityChanged?.Invoke(CurrentIntegrity, MaxIntegrity);
        }
    }
}
