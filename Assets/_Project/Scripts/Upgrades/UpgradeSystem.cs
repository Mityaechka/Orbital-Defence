using System.Collections.Generic;
using UnityEngine;

namespace OrbitalDefense
{
    public sealed class UpgradeSystem : MonoBehaviour
    {
        [SerializeField] private GameStateController gameState;
        [SerializeField] private CoreIntegrity coreIntegrity;
        [SerializeField] private RunModifiers modifiers;
        [SerializeField] private CommandCoreUpgrade commandCoreUpgrade;
        [SerializeField] private UpgradeConfig[] availableUpgrades;
        [SerializeField] private int choicesPerWave = 3;

        private UpgradeConfig[] currentChoices;

        public UpgradeConfig[] AvailableUpgrades
        {
            get
            {
                if (gameState != null && gameState.CurrentPhase == GamePhase.UpgradeChoice)
                {
                    EnsureCurrentChoices();
                    return currentChoices;
                }

                return currentChoices != null && currentChoices.Length > 0 ? currentChoices : availableUpgrades;
            }
        }

        private void Awake()
        {
            gameState ??= FindAnyObjectByType<GameStateController>();
            coreIntegrity ??= FindAnyObjectByType<CoreIntegrity>();
            modifiers ??= FindAnyObjectByType<RunModifiers>();
            commandCoreUpgrade ??= FindAnyObjectByType<CommandCoreUpgrade>();
        }

        private void OnEnable()
        {
            if (gameState != null)
            {
                gameState.PhaseChanged += HandlePhaseChanged;
            }
        }

        private void OnDisable()
        {
            if (gameState != null)
            {
                gameState.PhaseChanged -= HandlePhaseChanged;
            }
        }

        public bool CanChooseUpgrade(UpgradeConfig upgrade)
        {
            return upgrade != null && (commandCoreUpgrade == null || !commandCoreUpgrade.IsUpgradeMaxed(upgrade));
        }

        public int GetUpgradeLevel(UpgradeConfig upgrade)
        {
            return commandCoreUpgrade != null ? commandCoreUpgrade.GetBranchLevel(upgrade) : 0;
        }

        public void ChooseUpgrade(UpgradeConfig upgrade)
        {
            if (!CanChooseUpgrade(upgrade))
            {
                return;
            }

            modifiers?.Apply(upgrade);
            if (upgrade.EffectType == UpgradeEffectType.CoreRepairPercent && coreIntegrity != null)
            {
                coreIntegrity.RepairPercent(upgrade.Value);
            }
            else if (upgrade.EffectType == UpgradeEffectType.CoreMaxIntegrityPercent && coreIntegrity != null)
            {
                coreIntegrity.IncreaseMaxIntegrityPercent(upgrade.Value);
            }
            else
            {
                commandCoreUpgrade?.TryApplyUpgrade(upgrade);
            }

            gameState?.EnterBuildPhase();
            currentChoices = null;
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            if (phase == GamePhase.UpgradeChoice)
            {
                GenerateCurrentChoices();
            }
            else
            {
                currentChoices = null;
            }
        }

        private void EnsureCurrentChoices()
        {
            if (currentChoices == null || currentChoices.Length == 0)
            {
                GenerateCurrentChoices();
            }
        }

        private void GenerateCurrentChoices()
        {
            List<UpgradeConfig> pool = new();
            if (availableUpgrades != null)
            {
                for (int i = 0; i < availableUpgrades.Length; i++)
                {
                    UpgradeConfig upgrade = availableUpgrades[i];
                    if (CanChooseUpgrade(upgrade))
                    {
                        pool.Add(upgrade);
                    }
                }
            }

            int choiceCount = Mathf.Min(Mathf.Max(1, choicesPerWave), pool.Count);
            List<UpgradeConfig> choices = new(choiceCount);
            for (int i = 0; i < choiceCount; i++)
            {
                UpgradeConfig picked = PickWeighted(pool);
                if (picked == null)
                {
                    break;
                }

                choices.Add(picked);
                pool.Remove(picked);
                if (IsCommandCoreUpgrade(picked))
                {
                    pool.RemoveAll(IsCommandCoreUpgrade);
                }
            }

            currentChoices = choices.ToArray();
        }

        private static UpgradeConfig PickWeighted(List<UpgradeConfig> pool)
        {
            int totalWeight = 0;
            for (int i = 0; i < pool.Count; i++)
            {
                totalWeight += Mathf.Max(1, pool[i].SelectionWeight);
            }

            int roll = Random.Range(0, totalWeight);
            for (int i = 0; i < pool.Count; i++)
            {
                roll -= Mathf.Max(1, pool[i].SelectionWeight);
                if (roll < 0)
                {
                    return pool[i];
                }
            }

            return pool.Count > 0 ? pool[^1] : null;
        }

        private static bool IsCommandCoreUpgrade(UpgradeConfig upgrade)
        {
            if (upgrade == null)
            {
                return false;
            }

            return upgrade.EffectType == UpgradeEffectType.CommandCoreMining
                || upgrade.EffectType == UpgradeEffectType.CommandCoreDefense
                || upgrade.EffectType == UpgradeEffectType.CommandCoreShield;
        }
    }
}
