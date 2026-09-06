using UnityEngine;

namespace OrbitalDefense
{
    [CreateAssetMenu(menuName = "Orbital Defense/Config/Building", fileName = "BuildingConfig")]
    public sealed class BuildingConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private string displayNameKey;
        [SerializeField] private BuildingKind kind;
        [SerializeField] private Sprite icon;
        [SerializeField] private Building prefab;

        [Header("Build")]
        [SerializeField] private int buildCost = 50;
        [SerializeField] private int maxLevel = 3;
        [SerializeField] private BuildSlotType[] allowedSlots;

        [Header("Mine")]
        [SerializeField] private int productionAmount = 10;
        [SerializeField] private float productionInterval = 5f;

        [Header("Cannon")]
        [SerializeField] private int damage = 10;
        [SerializeField] private float fireRate = 1f;
        [SerializeField] private float range = 3.5f;
        [SerializeField] private float projectileSpeed = 6f;

        [Header("Booster")]
        [SerializeField] private float boostRadius = 2f;
        [SerializeField] private float miningBoostPercent = 25f;
        [SerializeField] private float fireRateBoostPercent = 15f;

        public string Id => id;
        public string DisplayName => displayName;
        public string DisplayNameKey => !string.IsNullOrWhiteSpace(displayNameKey) ? displayNameKey : $"building.{id}.name";
        public BuildingKind Kind => kind;
        public Sprite Icon => icon;
        public Building Prefab => prefab;
        public int BuildCost => buildCost;
        public int MaxLevel => maxLevel;
        public int ProductionAmount => productionAmount;
        public float ProductionInterval => productionInterval;
        public int Damage => damage;
        public float FireRate => fireRate;
        public float Range => range;
        public float ProjectileSpeed => projectileSpeed;
        public float BoostRadius => boostRadius;
        public float MiningBoostPercent => miningBoostPercent;
        public float FireRateBoostPercent => fireRateBoostPercent;

        public bool CanBuildOn(BuildSlotType slotType)
        {
            if (allowedSlots == null || allowedSlots.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < allowedSlots.Length; i++)
            {
                if (allowedSlots[i] == slotType)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
