using UnityEngine;

namespace OrbitalDefense
{
    public enum UpgradeEffectType
    {
        MineProductionPercent,
        CannonDamagePercent,
        CannonRangePercent,
        BoosterRadiusPercent,
        CoreRepairPercent,
        CoreMaxIntegrityPercent,
        CommandCoreMining,
        CommandCoreDefense,
        CommandCoreShield,
        NextBuildingDiscountPercent
    }

    [CreateAssetMenu(menuName = "Orbital Defense/Config/Upgrade", fileName = "UpgradeConfig")]
    public sealed class UpgradeConfig : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private string description;
        [SerializeField] private string displayNameKey;
        [SerializeField] private string descriptionKey;
        [SerializeField] private UpgradeEffectType effectType;
        [SerializeField] private float value;
        [SerializeField] private int selectionWeight = 100;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public string DisplayNameKey => !string.IsNullOrWhiteSpace(displayNameKey) ? displayNameKey : $"upgrade.{id}.name";
        public string DescriptionKey => !string.IsNullOrWhiteSpace(descriptionKey) ? descriptionKey : $"upgrade.{id}.desc";
        public UpgradeEffectType EffectType => effectType;
        public float Value => value;
        public int SelectionWeight => selectionWeight;
    }
}
