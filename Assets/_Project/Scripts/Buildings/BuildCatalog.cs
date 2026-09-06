using UnityEngine;

namespace OrbitalDefense
{
    [CreateAssetMenu(menuName = "Orbital Defense/Config/Build Catalog", fileName = "BuildCatalog")]
    public sealed class BuildCatalog : ScriptableObject
    {
        [SerializeField] private BuildingConfig[] buildings;

        public BuildingConfig[] Buildings => buildings;
    }
}

