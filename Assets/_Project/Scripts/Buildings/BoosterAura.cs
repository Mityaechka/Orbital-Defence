using UnityEngine;

namespace OrbitalDefense
{
    public sealed class BoosterAura : MonoBehaviour
    {
        private static readonly System.Collections.Generic.List<BoosterAura> ActiveAuras = new();

        [SerializeField] private Building building;
        [SerializeField] private RunModifiers modifiers;

        public float Radius => building != null && building.Config != null ? building.Config.BoostRadius * building.LevelMultiplier * (modifiers != null ? modifiers.BoosterRadiusMultiplier : 1f) : 0f;
        public float MiningBoostPercent => building != null && building.Config != null ? building.Config.MiningBoostPercent : 0f;
        public float FireRateBoostPercent => building != null && building.Config != null ? building.Config.FireRateBoostPercent : 0f;

        private void Awake()
        {
            building ??= GetComponent<Building>();
            modifiers ??= FindFirstObjectByType<RunModifiers>();
        }

        private void OnEnable()
        {
            if (!ActiveAuras.Contains(this))
            {
                ActiveAuras.Add(this);
            }
        }

        private void OnDisable()
        {
            ActiveAuras.Remove(this);
        }

        public static float GetMiningMultiplier(Vector3 position)
        {
            return 1f + GetStrongestBoostPercent(position, boost => boost.MiningBoostPercent) / 100f;
        }

        public static float GetFireRateMultiplier(Vector3 position)
        {
            return 1f + GetStrongestBoostPercent(position, boost => boost.FireRateBoostPercent) / 100f;
        }

        private static float GetStrongestBoostPercent(Vector3 position, System.Func<BoosterAura, float> selector)
        {
            float strongest = 0f;
            for (int i = ActiveAuras.Count - 1; i >= 0; i--)
            {
                BoosterAura aura = ActiveAuras[i];
                if (aura == null)
                {
                    ActiveAuras.RemoveAt(i);
                    continue;
                }

                float radius = aura.Radius;
                if (radius <= 0f)
                {
                    continue;
                }

                float sqrDistance = (aura.transform.position - position).sqrMagnitude;
                if (sqrDistance <= radius * radius)
                {
                    strongest = Mathf.Max(strongest, selector(aura));
                }
            }

            return strongest;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.45f, 0.80f, 1f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
    }
}
