using UnityEngine;

namespace OrbitalDefense
{
    public sealed class BuildSlot : MonoBehaviour
    {
        [SerializeField] private BuildSlotType slotType;
        [SerializeField] private Transform placementPoint;

        public BuildSlotType SlotType => slotType;
        public Building CurrentBuilding { get; private set; }
        public bool IsOccupied => CurrentBuilding != null;
        public Transform PlacementPoint => placementPoint != null ? placementPoint : transform;

        public bool CanAccept(BuildingConfig config)
        {
            return config != null && !IsOccupied && config.CanBuildOn(slotType);
        }

        public void Occupy(Building building)
        {
            CurrentBuilding = building;
        }

        public void Clear()
        {
            CurrentBuilding = null;
        }
    }
}

