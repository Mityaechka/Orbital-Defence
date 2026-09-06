using UnityEngine;

namespace OrbitalDefense
{
    [CreateAssetMenu(menuName = "Orbital Defense/Config/Enemy", fileName = "EnemyConfig")]
    public sealed class EnemyConfig : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private EnemyMover prefab;
        [SerializeField] private int health = 20;
        [SerializeField] private float speed = 1f;
        [SerializeField] private int coreDamage = 8;
        [SerializeField] private int reward = 8;

        public string Id => id;
        public string DisplayName => displayName;
        public EnemyMover Prefab => prefab;
        public int Health => health;
        public float Speed => speed;
        public int CoreDamage => coreDamage;
        public int Reward => reward;
    }
}

