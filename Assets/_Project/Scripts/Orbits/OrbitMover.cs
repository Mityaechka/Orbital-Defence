using System;
using UnityEngine;

namespace OrbitalDefense
{
    public sealed class OrbitMover : MonoBehaviour
    {
        [SerializeField] private Transform center;
        [SerializeField] private float radius = 3f;
        [SerializeField] private float degreesPerSecond = 20f;
        [SerializeField] private float startAngleDegrees;

        public event Action<float> AngleChanged;

        public float CurrentAngleDegrees { get; private set; }
        public float StartAngleDegrees => startAngleDegrees;
        public Transform Center => center;
        public bool IsLocked { get; private set; }

        public void Initialize(Transform orbitCenter, float orbitRadius, float degreesPerSecond, float startAngleDegrees)
        {
            center = orbitCenter;
            radius = orbitRadius;
            this.degreesPerSecond = degreesPerSecond;
            this.startAngleDegrees = NormalizeAngle(startAngleDegrees);
            IsLocked = false;
            SetAngleInternal(this.startAngleDegrees, false);
        }

        private void Awake()
        {
            startAngleDegrees = NormalizeAngle(startAngleDegrees);
            CurrentAngleDegrees = startAngleDegrees;
            ApplyPosition();
        }

        public void SetAngle(float angleDegrees)
        {
            if (IsLocked)
            {
                return;
            }

            SetAngleInternal(angleDegrees, true);
        }

        public void RotateBy(float deltaDegrees)
        {
            SetAngle(CurrentAngleDegrees + deltaDegrees);
        }

        public void ResetAngle()
        {
            SetAngle(startAngleDegrees);
        }

        public void LockPosition()
        {
            IsLocked = true;
        }

        public void UnlockPosition()
        {
            IsLocked = false;
        }

        private void OnValidate()
        {
            startAngleDegrees = NormalizeAngle(startAngleDegrees);
            CurrentAngleDegrees = startAngleDegrees;
            if (!Application.isPlaying)
            {
                ApplyPosition();
            }
        }

        private void SetAngleInternal(float angleDegrees, bool notifyChange)
        {
            float normalized = NormalizeAngle(angleDegrees);
            bool changed = !Mathf.Approximately(CurrentAngleDegrees, normalized);
            CurrentAngleDegrees = normalized;
            ApplyPosition();

            if (notifyChange && changed)
            {
                AngleChanged?.Invoke(CurrentAngleDegrees);
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

        private static float NormalizeAngle(float angleDegrees)
        {
            return Mathf.Repeat(angleDegrees, 360f);
        }
    }
}
