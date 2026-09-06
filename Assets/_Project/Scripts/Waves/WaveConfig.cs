using System;
using UnityEngine;

namespace OrbitalDefense
{
    [CreateAssetMenu(menuName = "Orbital Defense/Config/Wave", fileName = "WaveConfig")]
    public sealed class WaveConfig : ScriptableObject
    {
        [SerializeField] private int waveNumber = 1;
        [SerializeField] private EnemySpawnGroup[] spawnGroups;
        [SerializeField] private int rewardOnComplete = 25;
        [SerializeField] private float spawnRadius = 7f;

        public int WaveNumber => waveNumber;
        public EnemySpawnGroup[] SpawnGroups => spawnGroups;
        public int RewardOnComplete => rewardOnComplete;
        public float SpawnRadius => spawnRadius;
    }

    [Serializable]
    public sealed class EnemySpawnGroup
    {
        [SerializeField] private EnemyConfig enemy;
        [SerializeField] private int count = 5;
        [SerializeField] private float delayBetweenSpawns = 0.5f;
        [SerializeField] private float angleDegrees;
        [SerializeField] private float angleSpreadDegrees = 15f;

        public EnemyConfig Enemy => enemy;
        public int Count => count;
        public float DelayBetweenSpawns => delayBetweenSpawns;
        public float AngleDegrees => angleDegrees;
        public float AngleSpreadDegrees => angleSpreadDegrees;
    }
}

