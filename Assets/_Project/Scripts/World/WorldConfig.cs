using System;
using UnityEngine;

namespace OrbitalDefense
{
    [CreateAssetMenu(menuName = "Orbital Defense/Config/World", fileName = "WorldConfig")]
    public sealed class WorldConfig : ScriptableObject
    {
        [SerializeField] private MoonConfig[] moons;

        public MoonConfig[] Moons => moons;

        public static event Action<WorldConfig> Changed;

        private void OnValidate()
        {
            Changed?.Invoke(this);
        }
    }

    [Serializable]
    public sealed class MoonConfig
    {
        [SerializeField] private string objectName = "Moon";
        [SerializeField] private string orbitRingName = "MoonOrbitRing";
        [SerializeField] private Vector3 scale = new(0.7f, 0.7f, 1f);
        [SerializeField] private float orbitRadius = 2.65f;
        [SerializeField] private float orbitDegreesPerSecond = 18f;
        [SerializeField] private float startAngleDegrees = 35f;
        [SerializeField] private float selfRotationDegreesPerSecond = -22f;
        [SerializeField] private int slotCount = 3;
        [SerializeField] private float slotRadius = 0.48f;
        [SerializeField] private float slotAngleOffsetDegrees = 45f;
        [SerializeField] private Color orbitRingColor = new(0.80f, 0.86f, 1f, 0.18f);

        public string ObjectName => objectName;
        public string OrbitRingName => orbitRingName;
        public Vector3 Scale => scale;
        public float OrbitRadius => orbitRadius;
        public float OrbitDegreesPerSecond => orbitDegreesPerSecond;
        public float StartAngleDegrees => startAngleDegrees;
        public float SelfRotationDegreesPerSecond => selfRotationDegreesPerSecond;
        public int SlotCount => slotCount;
        public float SlotRadius => slotRadius;
        public float SlotAngleOffsetDegrees => slotAngleOffsetDegrees;
        public Color OrbitRingColor => orbitRingColor;
    }
}
