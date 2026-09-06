using UnityEngine;

namespace OrbitalDefense
{
    public sealed class Building : MonoBehaviour
    {
        [SerializeField] private BuildingConfig config;

        public BuildingConfig Config => config;
        public BuildSlot Slot { get; private set; }
        public int Level { get; private set; } = 1;
        public float LevelMultiplier => 1f + Mathf.Max(0, Level - 1) * 0.25f;

        public void Initialize(BuildingConfig buildingConfig, BuildSlot slot)
        {
            config = buildingConfig;
            Slot = slot;
            Level = 1;
        }

        public bool CanUpgrade()
        {
            return config != null && Level < config.MaxLevel;
        }

        public void Upgrade()
        {
            if (!CanUpgrade())
            {
                return;
            }

            Level++;
        }
    }
}
