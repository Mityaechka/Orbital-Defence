using System;
using UnityEngine;

namespace OrbitalDefense
{
    public sealed class ResourceWallet : MonoBehaviour
    {
        [SerializeField] private int startingMinerals = 100;

        public event Action<int> MineralsChanged;

        public int Minerals { get; private set; }

        private void Awake()
        {
            Minerals = Mathf.Max(0, startingMinerals);
        }

        private void Start()
        {
            MineralsChanged?.Invoke(Minerals);
        }

        public bool CanSpend(int amount)
        {
            return amount >= 0 && Minerals >= amount;
        }

        public bool TrySpend(int amount)
        {
            if (!CanSpend(amount))
            {
                return false;
            }

            Minerals -= amount;
            MineralsChanged?.Invoke(Minerals);
            return true;
        }

        public void Add(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Minerals += amount;
            MineralsChanged?.Invoke(Minerals);
        }
    }
}

