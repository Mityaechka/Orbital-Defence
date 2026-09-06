using UnityEngine;

namespace OrbitalDefense
{
    public sealed class MineProducer : MonoBehaviour
    {
        [SerializeField] private Building building;
        [SerializeField] private ResourceWallet wallet;
        [SerializeField] private RunModifiers modifiers;

        private float timer;

        private void Awake()
        {
            building ??= GetComponent<Building>();
            wallet ??= FindFirstObjectByType<ResourceWallet>();
            modifiers ??= FindFirstObjectByType<RunModifiers>();
        }

        private void Update()
        {
            if (building == null || building.Config == null || wallet == null)
            {
                return;
            }

            float interval = Mathf.Max(0.1f, building.Config.ProductionInterval);
            timer += Time.deltaTime;

            if (timer < interval)
            {
                return;
            }

            timer -= interval;
            float multiplier = modifiers != null ? modifiers.MineProductionMultiplier : 1f;
            multiplier *= BoosterAura.GetMiningMultiplier(transform.position);
            wallet.Add(Mathf.RoundToInt(building.Config.ProductionAmount * building.LevelMultiplier * multiplier));
        }
    }
}
