using UnityEngine;

namespace OrbitalDefense
{
    [RequireComponent(typeof(OrbitMover))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class OrbitalMoonSelectionFeedback : MonoBehaviour
    {
        [SerializeField] private OrbitalSetupController setupController;
        [SerializeField] private Color selectedColor = new(0.66f, 0.95f, 1f, 1f);
        [SerializeField] private float selectedScale = 1.12f;
        [SerializeField] private float smoothSpeed = 16f;

        private OrbitMover orbitMover;
        private SpriteRenderer spriteRenderer;
        private Color baseColor;
        private Vector3 baseScale;
        private bool isSelected;

        private void Awake()
        {
            orbitMover = GetComponent<OrbitMover>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            setupController ??= FindFirstObjectByType<OrbitalSetupController>();
            baseColor = spriteRenderer.color;
            baseScale = transform.localScale;
        }

        private void OnEnable()
        {
            if (setupController != null)
            {
                setupController.SelectedMoonChanged += HandleSelectedMoonChanged;
                HandleSelectedMoonChanged(setupController.SelectedMoon);
            }
        }

        private void OnDisable()
        {
            if (setupController != null)
            {
                setupController.SelectedMoonChanged -= HandleSelectedMoonChanged;
            }
        }

        private void Update()
        {
            Vector3 targetScale = isSelected ? baseScale * selectedScale : baseScale;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * smoothSpeed);

            if (spriteRenderer != null)
            {
                Color targetColor = isSelected ? selectedColor : baseColor;
                spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, Time.unscaledDeltaTime * smoothSpeed);
            }
        }

        private void HandleSelectedMoonChanged(OrbitMover selectedMoon)
        {
            isSelected = selectedMoon == orbitMover;
        }
    }
}
