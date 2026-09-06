using UnityEngine;

namespace OrbitalDefense
{
    public sealed class CommandCoreVisual : MonoBehaviour
    {
        [SerializeField] private CoreIntegrity coreIntegrity;
        [SerializeField] private Transform coreBody;
        [SerializeField] private SpriteRenderer glowRenderer;
        [SerializeField] private float maxScaleBoost = 0.35f;

        private Vector3 initialScale;
        private Color initialGlowColor;
        private int initialMaxIntegrity;

        private void Awake()
        {
            coreIntegrity ??= FindAnyObjectByType<CoreIntegrity>();
            coreBody ??= transform;

            initialScale = coreBody.localScale;
            initialGlowColor = glowRenderer != null ? glowRenderer.color : Color.white;
            initialMaxIntegrity = coreIntegrity != null ? coreIntegrity.MaxIntegrity : 100;
        }

        private void OnEnable()
        {
            if (coreIntegrity != null)
            {
                coreIntegrity.IntegrityChanged += HandleIntegrityChanged;
                HandleIntegrityChanged(coreIntegrity.CurrentIntegrity, coreIntegrity.MaxIntegrity);
            }
        }

        private void OnDisable()
        {
            if (coreIntegrity != null)
            {
                coreIntegrity.IntegrityChanged -= HandleIntegrityChanged;
            }
        }

        private void HandleIntegrityChanged(int current, int max)
        {
            float upgradeProgress = Mathf.Clamp01((max - initialMaxIntegrity) / (float)initialMaxIntegrity);
            coreBody.localScale = initialScale * (1f + upgradeProgress * maxScaleBoost);

            if (glowRenderer != null)
            {
                Color glow = initialGlowColor;
                glow.a = Mathf.Lerp(initialGlowColor.a, 0.78f, upgradeProgress);
                glowRenderer.color = glow;
            }
        }
    }
}
