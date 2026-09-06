using UnityEngine;

namespace OrbitalDefense
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SpriteBurstEffect : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.35f;
        [SerializeField] private float startScale = 0.20f;
        [SerializeField] private float endScale = 0.85f;

        private SpriteRenderer spriteRenderer;
        private Color startColor;
        private float age;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            startColor = spriteRenderer.color;
            transform.localScale = Vector3.one * startScale;
        }

        private void Update()
        {
            age += Time.deltaTime;
            float t = Mathf.Clamp01(age / Mathf.Max(0.01f, lifetime));
            transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t);

            Color color = startColor;
            color.a *= 1f - t;
            spriteRenderer.color = color;

            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
