using System.Collections;
using UnityEngine;

namespace OrbitalDefense
{
    [RequireComponent(typeof(Health))]
    public sealed class DamageFlash : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float duration = 0.08f;

        private Health health;
        private Color originalColor;
        private Coroutine flashRoutine;

        private void Awake()
        {
            health = GetComponent<Health>();
            targetRenderer ??= GetComponentInChildren<SpriteRenderer>();

            if (targetRenderer != null)
            {
                originalColor = targetRenderer.color;
            }
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.Changed += HandleHealthChanged;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Changed -= HandleHealthChanged;
            }
        }

        private void HandleHealthChanged(int current, int max)
        {
            if (current <= 0 || current >= max || targetRenderer == null)
            {
                return;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(Flash());
        }

        private IEnumerator Flash()
        {
            targetRenderer.color = flashColor;
            yield return new WaitForSeconds(duration);
            targetRenderer.color = originalColor;
            flashRoutine = null;
        }
    }
}
