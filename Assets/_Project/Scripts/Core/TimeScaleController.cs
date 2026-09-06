using System;
using UnityEngine;

namespace OrbitalDefense
{
    public sealed class TimeScaleController : MonoBehaviour
    {
        [SerializeField] private TimeScaleConfig config;

        public event Action<float> TimeScaleChanged;

        public float CurrentScale { get; private set; } = 1f;

        private void OnEnable()
        {
            SetNormal();
        }

        private void OnDisable()
        {
            Time.timeScale = 1f;
        }

        public void ToggleSpeed()
        {
            float fast = config != null ? config.FastSpeed : 2f;
            if (Mathf.Approximately(CurrentScale, fast))
            {
                SetNormal();
                return;
            }

            SetFast();
        }

        public void SetNormal()
        {
            float speed = config != null ? config.NormalSpeed : 1f;
            SetScale(speed);
        }

        public void SetFast()
        {
            float speed = config != null ? config.FastSpeed : 2f;
            SetScale(speed);
        }

        public void Pause()
        {
            SetScale(0f);
        }

        private void SetScale(float scale)
        {
            CurrentScale = Mathf.Max(0f, scale);
            Time.timeScale = CurrentScale;
            TimeScaleChanged?.Invoke(CurrentScale);
        }
    }
}

