using UnityEngine;

namespace OrbitalDefense
{
    public sealed class BuildSystem : MonoBehaviour
    {
        [SerializeField] private GameStateController gameState;
        [SerializeField] private ResourceWallet wallet;
        [SerializeField] private RunModifiers modifiers;

        private void Awake()
        {
            gameState ??= FindFirstObjectByType<GameStateController>();
            wallet ??= FindFirstObjectByType<ResourceWallet>();
            modifiers ??= FindFirstObjectByType<RunModifiers>();
        }

        public bool TryBuild(BuildSlot slot, BuildingConfig config)
        {
            if (gameState != null && !gameState.CanBuild)
            {
                return false;
            }

            if (slot == null || config == null || config.Prefab == null || !slot.CanAccept(config))
            {
                return false;
            }

            int cost = GetBuildCost(config);
            if (wallet != null && !wallet.TrySpend(cost))
            {
                return false;
            }

            modifiers?.ConsumeBuildingCostMultiplier();

            Transform point = slot.PlacementPoint;
            Building building = Instantiate(config.Prefab, point.position, point.rotation, point);
            building.Initialize(config, slot);
            slot.Occupy(building);
            AudioService.PlayBuild();
            return true;
        }

        public bool TrySell(BuildSlot slot)
        {
            if (gameState != null && !gameState.CanBuild)
            {
                return false;
            }

            if (slot == null || slot.CurrentBuilding == null)
            {
                return false;
            }

            Building building = slot.CurrentBuilding;
            int refund = building.Config != null ? Mathf.FloorToInt(building.Config.BuildCost * 0.5f) : 0;
            slot.Clear();
            Destroy(building.gameObject);
            wallet?.Add(refund);
            AudioService.PlaySell();
            return true;
        }

        public bool TryUpgrade(BuildSlot slot)
        {
            if (gameState != null && !gameState.CanBuild)
            {
                return false;
            }

            if (slot == null || slot.CurrentBuilding == null || !slot.CurrentBuilding.CanUpgrade())
            {
                return false;
            }

            Building building = slot.CurrentBuilding;
            int cost = GetUpgradeCost(building);
            if (wallet != null && !wallet.TrySpend(cost))
            {
                return false;
            }

            building.Upgrade();
            AudioService.PlayUpgrade();
            return true;
        }

        public int GetUpgradeCost(Building building)
        {
            if (building == null || building.Config == null)
            {
                return 0;
            }

            return Mathf.Max(1, Mathf.RoundToInt(building.Config.BuildCost * (0.65f + building.Level * 0.35f)));
        }

        public int GetBuildCost(BuildingConfig config)
        {
            if (config == null)
            {
                return 0;
            }

            float costMultiplier = modifiers != null ? modifiers.CurrentBuildingCostMultiplier : 1f;
            return Mathf.Max(0, Mathf.RoundToInt(config.BuildCost * costMultiplier));
        }

        public bool CanAffordBuild(BuildingConfig config)
        {
            return wallet == null || wallet.CanSpend(GetBuildCost(config));
        }

        public bool CanAffordUpgrade(Building building)
        {
            return wallet == null || wallet.CanSpend(GetUpgradeCost(building));
        }
    }
}
