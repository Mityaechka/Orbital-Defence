using UnityEngine;

namespace OrbitalDefense
{
    public sealed class OrbitMover : MonoBehaviour
    {
        [SerializeField] private Transform center;
        [SerializeField] private float radius = 3f;
        [SerializeField] private float degreesPerSecond = 20f;
        [SerializeField] private float startAngleDegrees;

        public float CurrentAngleDegrees { get; private set; }

        public void Initialize(Transform orbitCenter, float orbitRadius, float degreesPerSecond, float startAngleDegrees)
        {
            center = orbitCenter;
            radius = orbitRadius;
            this.degreesPerSecond = degreesPerSecond;
            this.startAngleDegrees = startAngleDegrees;
            CurrentAngleDegrees = startAngleDegrees;
            ApplyPosition();
        }

        private void Awake()
        {
            CurrentAngleDegrees = startAngleDegrees;
        }

        private void Update()
        {
            CurrentAngleDegrees += degreesPerSecond * Time.deltaTime;
            ApplyPosition();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                CurrentAngleDegrees = startAngleDegrees;
                ApplyPosition();
            }
        }

        private void ApplyPosition()
        {
            if (center == null)
            {
                return;
            }

            float radians = CurrentAngleDegrees * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * radius;
            transform.position = center.position + offset;
        }
    }
}

