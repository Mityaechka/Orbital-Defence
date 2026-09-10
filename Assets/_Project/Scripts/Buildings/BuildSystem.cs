using UnityEngine;

namespace OrbitalDefense
{
    public sealed class BuildSystem : MonoBehaviour
    {
        private const float MineWorldScale = 0.78f;
        private const float CannonWorldScale = 0.66f;
        private const float BoosterWorldScale = 0.72f;

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
            Transform parent = point.parent != null ? point.parent : point;
            Building building = Instantiate(config.Prefab, point.position, point.rotation, parent);
            StabilizeBuildingTransform(building.transform, config);
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

        private static void StabilizeBuildingTransform(Transform building, BuildingConfig config)
        {
            if (building == null || config == null)
            {
                return;
            }

            Transform parent = building.parent;
            if (parent == null)
            {
                building.localScale = Vector3.one * GetBuildingWorldScale(config.Kind);
                return;
            }

            Vector3 targetWorldScale = Vector3.one * GetBuildingWorldScale(config.Kind);
            Vector3 parentScale = parent.lossyScale;
            building.localScale = new Vector3(
                SafeDivide(targetWorldScale.x, parentScale.x),
                SafeDivide(targetWorldScale.y, parentScale.y),
                SafeDivide(targetWorldScale.z, parentScale.z));

            foreach (SpriteRenderer renderer in building.GetComponentsInChildren<SpriteRenderer>())
            {
                renderer.sortingOrder = Mathf.Max(renderer.sortingOrder, 12);
            }
        }

        private static float SafeDivide(float value, float divisor)
        {
            return Mathf.Abs(divisor) > 0.0001f ? value / divisor : value;
        }

        private static float GetBuildingWorldScale(BuildingKind kind)
        {
            return kind switch
            {
                BuildingKind.Mine => MineWorldScale,
                BuildingKind.Cannon => CannonWorldScale,
                BuildingKind.OrbitalBooster => BoosterWorldScale,
                _ => CannonWorldScale
            };
        }
    }
}
