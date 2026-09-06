using UnityEngine;

namespace OrbitalDefense
{
    public sealed class VisualEffectSpawner : MonoBehaviour
    {
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

        private static void Spawn(GameObject prefab, Vector3 position)
        {
            if (prefab == null)
            {
                return;
            }

            Instantiate(prefab, position, Quaternion.identity);
        }
    }
}
