using UnityEngine;

namespace OrbitalDefense
{
    public sealed class CommandCoreUpgrade : MonoBehaviour
    {
        [SerializeField] private CoreIntegrity coreIntegrity;
        [SerializeField] private ResourceWallet wallet;
        [SerializeField] private WaveSystem waveSystem;
        [SerializeField] private int maxBranchLevel = 3;
        [SerializeField] private int miningRewardPerLevel = 15;
        [SerializeField] private int shieldBlocksPerLevel = 1;
        [SerializeField] private float integrityIncreasePercentPerShieldLevel = 10f;

        public event System.Action Changed;

        public int MiningLevel { get; private set; }
        public int DefenseLevel { get; private set; }
        public int ShieldLevel { get; private set; }
        public int ShieldBlocksRemaining { get; private set; }
        public int MaxBranchLevel => maxBranchLevel;

        private void Awake()
        {
            coreIntegrity ??= FindAnyObjectByType<CoreIntegrity>();
            wallet ??= FindAnyObjectByType<ResourceWallet>();
            waveSystem ??= FindAnyObjectByType<WaveSystem>();
        }

        private void OnEnable()
        {
            if (waveSystem != null)
            {
                waveSystem.WaveStarted += HandleWaveStarted;
                waveSystem.WaveCompleted += HandleWaveCompleted;
            }
        }

        private void OnDisable()
        {
            if (waveSystem != null)
            {
                waveSystem.WaveStarted -= HandleWaveStarted;
                waveSystem.WaveCompleted -= HandleWaveCompleted;
            }
        }

        public bool TryApplyUpgrade(UpgradeConfig upgrade)
        {
            if (upgrade == null || IsUpgradeMaxed(upgrade))
            {
                return false;
            }

            switch (upgrade.EffectType)
            {
                case UpgradeEffectType.CommandCoreMining:
                    MiningLevel++;
                    break;
                case UpgradeEffectType.CommandCoreDefense:
                    DefenseLevel++;
                    break;
                case UpgradeEffectType.CommandCoreShield:
                    ShieldLevel++;
                    ShieldBlocksRemaining += shieldBlocksPerLevel;
                    coreIntegrity?.IncreaseMaxIntegrityPercent(integrityIncreasePercentPerShieldLevel);
                    break;
                default:
                    return false;
            }

            Changed?.Invoke();
            return true;
        }

        public bool IsUpgradeMaxed(UpgradeConfig upgrade)
        {
            if (upgrade == null)
            {
                return false;
            }

            switch (upgrade.EffectType)
            {
                case UpgradeEffectType.CommandCoreMining:
                    return MiningLevel >= MaxBranchLevel;
                case UpgradeEffectType.CommandCoreDefense:
                    return DefenseLevel >= MaxBranchLevel;
                case UpgradeEffectType.CommandCoreShield:
                    return ShieldLevel >= MaxBranchLevel;
                default:
                    return false;
            }
        }

        public int GetBranchLevel(UpgradeConfig upgrade)
        {
            if (upgrade == null)
            {
                return 0;
            }

            switch (upgrade.EffectType)
            {
                case UpgradeEffectType.CommandCoreMining:
                    return MiningLevel;
                case UpgradeEffectType.CommandCoreDefense:
                    return DefenseLevel;
                case UpgradeEffectType.CommandCoreShield:
                    return ShieldLevel;
                default:
                    return 0;
            }
        }

        public bool TryBlockImpact()
        {
            if (ShieldBlocksRemaining <= 0)
            {
                return false;
            }

            ShieldBlocksRemaining--;
            Changed?.Invoke();
            return true;
        }

        private void HandleWaveStarted(int waveNumber, int totalWaves)
        {
            ShieldBlocksRemaining = ShieldLevel * shieldBlocksPerLevel;
            Changed?.Invoke();
        }

        private void HandleWaveCompleted(int waveNumber, int totalWaves)
        {
            if (wallet != null && MiningLevel > 0)
            {
                wallet.Add(MiningLevel * miningRewardPerLevel);
            }

            Changed?.Invoke();
        }
    }
}
