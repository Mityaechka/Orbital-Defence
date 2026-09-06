using UnityEngine;

namespace OrbitalDefense
{
    [CreateAssetMenu(menuName = "Orbital Defense/Config/Time Scale", fileName = "TimeScaleConfig")]
    public sealed class TimeScaleConfig : ScriptableObject
    {
        [SerializeField] private float normalSpeed = 1f;
        [SerializeField] private float fastSpeed = 2f;

        public float NormalSpeed => normalSpeed;
        public float FastSpeed => fastSpeed;
    }
}

