using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OrbitalDefense
{
    [ExecuteAlways]
    public sealed class MoonSpawner : MonoBehaviour
    {
        [SerializeField] private WorldConfig worldConfig;
        [SerializeField] private Transform orbitCenter;
        [SerializeField] private Sprite moonSprite;
        [SerializeField] private Sprite slotSprite;

        private void OnEnable()
        {
            WorldConfig.Changed += OnWorldConfigChanged;
            Rebuild();
        }

        private void OnDisable()
        {
            WorldConfig.Changed -= OnWorldConfigChanged;
        }

        private void OnWorldConfigChanged(WorldConfig changedConfig)
        {
            if (changedConfig != worldConfig)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.delayCall += DelayedRebuild;
                return;
            }
#endif
            Rebuild();
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.delayCall += DelayedRebuild;
                return;
            }
#endif
            Rebuild();
        }

#if UNITY_EDITOR
        private void DelayedRebuild()
        {
            EditorApplication.delayCall -= DelayedRebuild;
            if (this == null)
            {
                return;
            }

            Rebuild();
        }
#endif

        public void Rebuild()
        {
            if (orbitCenter == null)
            {
                orbitCenter = transform;
            }

            ClearChildren();

            if (worldConfig == null)
            {
                return;
            }

            MoonConfig[] moons = worldConfig.Moons;
            for (int i = 0; i < moons.Length; i++)
            {
                SpawnMoon(moons[i]);
            }
        }

        private void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        private void SpawnMoon(MoonConfig config)
        {
            string objectName = string.IsNullOrWhiteSpace(config.ObjectName) ? "Moon" : config.ObjectName;
            string ringName = string.IsNullOrWhiteSpace(config.OrbitRingName) ? $"{objectName}OrbitRing" : config.OrbitRingName;

            GameObject ring = new GameObject(ringName, typeof(LineRenderer), typeof(OrbitRingRenderer));
            ring.transform.SetParent(transform, false);
            ring.transform.position = orbitCenter.position;
            ring.GetComponent<OrbitRingRenderer>().Initialize(config.OrbitRadius, config.OrbitRingColor);

            GameObject moon = new GameObject(objectName, typeof(SpriteRenderer));
            moon.transform.SetParent(transform, false);
            moon.transform.localScale = config.Scale;
            SpriteRenderer renderer = moon.GetComponent<SpriteRenderer>();
            renderer.sprite = moonSprite;
            renderer.sortingOrder = 1;

            moon.AddComponent<OrbitMover>().Initialize(orbitCenter, config.OrbitRadius, config.OrbitDegreesPerSecond, config.StartAngleDegrees);
            moon.AddComponent<SelfRotator>().Initialize(config.SelfRotationDegreesPerSecond);

            SpawnSurfaceSlots(moon.transform, objectName, config);
        }

        private void SpawnSurfaceSlots(Transform moonTransform, string moonObjectName, MoonConfig config)
        {
            GameObject root = new GameObject($"{moonObjectName}SurfaceSlots");
            root.transform.SetParent(moonTransform, false);

            int count = Mathf.Max(0, config.SlotCount);
            for (int i = 0; i < count; i++)
            {
                float angle = config.SlotAngleOffsetDegrees + i * 360f / count;
                Vector3 position = Quaternion.Euler(0f, 0f, angle) * Vector3.right * config.SlotRadius;

                GameObject slot = new GameObject($"{root.name}_{i + 1:00}", typeof(SpriteRenderer));
                slot.transform.SetParent(root.transform, false);
                slot.transform.localPosition = position;
                slot.transform.localScale = new Vector3(0.22f, 0.22f, 1f);

                SpriteRenderer renderer = slot.GetComponent<SpriteRenderer>();
                renderer.sprite = slotSprite;
                renderer.sortingOrder = 5;

                slot.AddComponent<BuildSlot>().Initialize(BuildSlotType.MoonSurface);
                CircleCollider2D collider = slot.AddComponent<CircleCollider2D>();
                collider.radius = 0.55f;
                slot.AddComponent<BuildSlotSelectionFeedback>();
                slot.AddComponent<BuildSlotSelector>();
            }
        }
    }
}
