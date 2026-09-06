using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitalDefense
{
    public sealed class WaveSystem : MonoBehaviour
    {
        [SerializeField] private GameStateController gameState;
        [SerializeField] private ResourceWallet wallet;
        [SerializeField] private CoreIntegrity coreIntegrity;
        [SerializeField] private Transform planetCenter;
        [SerializeField] private WaveConfig[] waves;

        private readonly List<Health> aliveEnemies = new();
        private bool spawning;
        private int currentWaveIndex = -1;

        public event Action<int, int> WaveStarted;
        public event Action<int, int> WaveCompleted;

        public int CurrentWaveNumber => currentWaveIndex + 1;
        public int TotalWaves => waves != null ? waves.Length : 0;

        private void Awake()
        {
            gameState ??= FindFirstObjectByType<GameStateController>();
            wallet ??= FindFirstObjectByType<ResourceWallet>();
            coreIntegrity ??= FindFirstObjectByType<CoreIntegrity>();
        }

        private void Update()
        {
            aliveEnemies.RemoveAll(enemy => enemy == null);

            if (!spawning && gameState != null && gameState.IsWaveActive && aliveEnemies.Count == 0)
            {
                CompleteWave();
            }
        }

        public void StartNextWave()
        {
            if (gameState == null || gameState.IsGameOver || gameState.IsWaveActive || spawning)
            {
                return;
            }

            if (waves == null || currentWaveIndex + 1 >= waves.Length)
            {
                gameState.EnterVictory();
                return;
            }

            currentWaveIndex++;
            gameState.EnterWave();
            WaveStarted?.Invoke(CurrentWaveNumber, TotalWaves);
            StartCoroutine(SpawnWave(waves[currentWaveIndex]));
        }

        private IEnumerator SpawnWave(WaveConfig wave)
        {
            spawning = true;

            if (wave != null && wave.SpawnGroups != null)
            {
                for (int i = 0; i < wave.SpawnGroups.Length; i++)
                {
                    EnemySpawnGroup group = wave.SpawnGroups[i];
                    for (int j = 0; j < group.Count; j++)
                    {
                        SpawnEnemy(wave, group);
                        yield return new WaitForSeconds(Mathf.Max(0.05f, group.DelayBetweenSpawns));
                    }
                }
            }

            spawning = false;
        }

        private void SpawnEnemy(WaveConfig wave, EnemySpawnGroup group)
        {
            if (group == null || group.Enemy == null || group.Enemy.Prefab == null || planetCenter == null)
            {
                return;
            }

            float angle = group.AngleDegrees + UnityEngine.Random.Range(-group.AngleSpreadDegrees, group.AngleSpreadDegrees);
            Vector3 position = planetCenter.position + Quaternion.Euler(0f, 0f, angle) * Vector3.right * wave.SpawnRadius;
            EnemyMover enemy = Instantiate(group.Enemy.Prefab, position, Quaternion.identity, transform);
            enemy.Initialize(group.Enemy, planetCenter);

            if (enemy.TryGetComponent(out CoreDamageOnReach reachDamage))
            {
                reachDamage.Initialize(planetCenter, coreIntegrity);
            }

            Health health = enemy.GetComponent<Health>();
            aliveEnemies.Add(health);
            health.Died += HandleEnemyDied;
        }

        private void HandleEnemyDied(Health health)
        {
            health.Died -= HandleEnemyDied;
            aliveEnemies.Remove(health);
        }

        private void CompleteWave()
        {
            WaveConfig completedWave = currentWaveIndex >= 0 && waves != null && currentWaveIndex < waves.Length
                ? waves[currentWaveIndex]
                : null;

            if (completedWave != null)
            {
                wallet?.Add(completedWave.RewardOnComplete);
            }

            WaveCompleted?.Invoke(CurrentWaveNumber, TotalWaves);

            if (currentWaveIndex + 1 >= TotalWaves)
            {
                gameState.EnterVictory();
                return;
            }

            gameState.EnterUpgradeChoice();
        }
    }
}
