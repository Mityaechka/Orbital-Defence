using TMPro;
using UnityEngine;

namespace OrbitalDefense
{
    public sealed class VisualEffectSpawner : MonoBehaviour
    {
        private const float EnemyDamageFontSize = 10;
        private const float CoreDamageFontSize = 10;

        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private GameObject deathEffectPrefab;
        [SerializeField] private GameObject coreDamageEffectPrefab;

        private static VisualEffectSpawner instance;

        private void Awake()
        {
            instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public static void SpawnHit(Vector3 position)
        {
            Spawn(instance != null ? instance.hitEffectPrefab : null, position);
        }

        public static void SpawnDeath(Vector3 position)
        {
            Spawn(instance != null ? instance.deathEffectPrefab : null, position);
        }

        public static void SpawnCoreDamage(Vector3 position)
        {
            Spawn(instance != null ? instance.coreDamageEffectPrefab : null, position);
        }

        public static void SpawnEnemyDamage(int amount, Vector3 position)
        {
            DamageNumberPopup.Spawn(amount, position + new Vector3(0f, 0.32f, 0f), new Color(1f, 0.92f, 0.30f, 1f), EnemyDamageFontSize);
        }

        public static void SpawnCoreDamageNumber(int amount, Vector3 position)
        {
            DamageNumberPopup.Spawn(amount, position + new Vector3(0f, 0.72f, 0f), new Color(1f, 0.24f, 0.18f, 1f), CoreDamageFontSize);
        }

        private static void Spawn(GameObject prefab, Vector3 position)
        {
            if (prefab == null)
            {
                return;
            }

            Instantiate(prefab, position, Quaternion.identity);
        }
    }

    internal sealed class DamageNumberPopup : MonoBehaviour
    {
        private const float Lifetime = 0.75f;

        [SerializeField] private Vector3 velocity = new(0f, 0.75f, 0f);
        [SerializeField] private float scalePunch = 0.35f;

        private TMP_Text text;
        private Color startColor;
        private float age;

        public static void Spawn(int amount, Vector3 position, Color color, float fontSize)
        {
            if (amount <= 0)
            {
                return;
            }

            GameObject go = new($"DamageNumber_{amount}", typeof(TextMeshPro), typeof(DamageNumberPopup));
            go.transform.position = position;

            TextMeshPro label = go.GetComponent<TextMeshPro>();
            label.text = amount.ToString();
            label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = color;
            label.sortingOrder = 80;
            label.textWrappingMode = TextWrappingModes.NoWrap;

            DamageNumberPopup popup = go.GetComponent<DamageNumberPopup>();
            popup.text = label;
            popup.startColor = color;
        }

        private void Awake()
        {
            text ??= GetComponent<TMP_Text>();
            startColor = text != null ? text.color : Color.white;
        }

        private void Update()
        {
            age += Time.deltaTime;
            float t = Mathf.Clamp01(age / Lifetime);

            transform.position += velocity * Time.deltaTime;
            float scale = 1f + (1f - t) * scalePunch;
            transform.localScale = Vector3.one * scale;

            if (text != null)
            {
                Color color = startColor;
                color.a = 1f - t;
                text.color = color;
            }

            if (age >= Lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
