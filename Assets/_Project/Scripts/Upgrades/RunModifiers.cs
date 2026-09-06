using UnityEngine;

namespace OrbitalDefense
{
    public sealed class RunModifiers : MonoBehaviour
    {
        public float MineProductionMultiplier { get; private set; } = 1f;
        public float CannonDamageMultiplier { get; private set; } = 1f;
        public float CannonRangeMultiplier { get; private set; } = 1f;
        public float BoosterRadiusMultiplier { get; private set; } = 1f;
        public float NextBuildingDiscountMultiplier { get; private set; } = 1f;

        public void Apply(UpgradeConfig upgrade)
        {
            if (upgrade == null)
            {
                return;
            }

            float multiplier = 1f + upgrade.Value / 100f;
            switch (upgrade.EffectType)
            {
                case UpgradeEffectType.MineProductionPercent:
                    MineProductionMultiplier *= multiplier;
                    break;
                case UpgradeEffectType.CannonDamagePercent:
                    CannonDamageMultiplier *= multiplier;
                    break;
                case UpgradeEffectType.CannonRangePercent:
                    CannonRangeMultiplier *= multiplier;
                    break;
                case UpgradeEffectType.BoosterRadiusPercent:
                    BoosterRadiusMultiplier *= multiplier;
                    break;
                case UpgradeEffectType.NextBuildingDiscountPercent:
                    NextBuildingDiscountMultiplier *= Mathf.Max(0f, 1f - upgrade.Value / 100f);
                    break;
            }
        }

        public float ConsumeBuildingCostMultiplier()
        {
            float multiplier = NextBuildingDiscountMultiplier;
            NextBuildingDiscountMultiplier = 1f;
            return multiplier;
        }

        public float CurrentBuildingCostMultiplier => NextBuildingDiscountMultiplier;
    }
}
