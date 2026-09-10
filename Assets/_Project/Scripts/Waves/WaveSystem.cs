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
        [SerializeField] private OrbitalSetupController orbitalSetupController;
        [SerializeField] private WaveConfig[] waves;
        [SerializeField] private float waveEndDelay = 2f;
        [SerializeField] private float stageWarningLeadTime = 1f;

        private readonly List<Health> aliveEnemies = new();
        private bool spawning;
        private bool completingWave;
        private int currentWaveIndex = -1;
        private WaveConfig currentWave;
        private int nextStageIndex = -1;

        public event Action<int, int> WaveStarted;
        public event Action<int, int> WaveCompleted;
        public event Action<float> StageWarningStarted;
        public event Action StageWarningEnded;

        public int CurrentWaveNumber => currentWaveIndex + 1;
        public int TotalWaves => waves != null ? waves.Length : 0;
        public bool CanStartNextWave =>
            gameState != null
            && gameState.CurrentPhase == GamePhase.BuildPhase
            && !spawning
            && NextWaveConfig != null
            && (orbitalSetupController == null || orbitalSetupController.IsReadyForWave);
        public WaveConfig NextWaveConfig
        {
            get
            {
                int nextWaveIndex = currentWaveIndex + 1;
                return waves != null && nextWaveIndex >= 0 && nextWaveIndex < waves.Length ? waves[nextWaveIndex] : null;
            }
        }

        private void Awake()
        {
            gameState ??= FindFirstObjectByType<GameStateController>();
            wallet ??= FindFirstObjectByType<ResourceWallet>();
            coreIntegrity ??= FindFirstObjectByType<CoreIntegrity>();
            orbitalSetupController ??= FindFirstObjectByType<OrbitalSetupController>();
        }

        private void Update()
        {
            aliveEnemies.RemoveAll(enemy => enemy == null);

            if (!spawning && !completingWave && gameState != null && gameState.IsWaveActive && aliveEnemies.Count == 0)
            {
                completingWave = true;
                StartCoroutine(CompleteWaveAfterDelay());
            }
        }

        private IEnumerator CompleteWaveAfterDelay()
        {
            yield return new WaitForSeconds(waveEndDelay);
            CompleteWave();
            completingWave = false;
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

            if (!CanStartNextWave || (orbitalSetupController != null && !orbitalSetupController.LockForWave()))
            {
                return;
            }

            currentWaveIndex++;
            currentWave = waves[currentWaveIndex];
            nextStageIndex = 0;
            gameState.EnterWave();
            WaveStarted?.Invoke(CurrentWaveNumber, TotalWaves);
            StartCoroutine(SpawnWave(currentWave));
        }

        private IEnumerator SpawnWave(WaveConfig wave)
        {
            spawning = true;

            if (wave != null && wave.SpawnGroups != null)
            {
                for (int i = 0; i < wave.SpawnGroups.Length; i++)
                {
                    if (gameState != null && gameState.IsGameOver)
                    {
                        break;
                    }

                    EnemySpawnGroup group = wave.SpawnGroups[i];
                    if (group == null)
                    {
                        nextStageIndex = i + 1;
                        continue;
                    }

                    if (i > 0)
                    {
                        if (TryGetStageDirection(wave, i, out float warningAngle))
                        {
                            nextStageIndex = i;
                            StageWarningStarted?.Invoke(warningAngle);
                            yield return new WaitForSeconds(stageWarningLeadTime);
                            StageWarningEnded?.Invoke();
                        }
                        else
                        {
                            StageWarningEnded?.Invoke();
                        }

                        if (gameState != null && gameState.IsGameOver)
                        {
                            break;
                        }
                    }

                    nextStageIndex = i + 1;
                    for (int j = 0; j < group.Count; j++)
                    {
                        SpawnEnemy(wave, group);
                        yield return new WaitForSeconds(Mathf.Max(0.05f, group.DelayBetweenSpawns));
                    }
                }
            }

            spawning = false;
            nextStageIndex = -1;
            StageWarningEnded?.Invoke();
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
            StageWarningEnded?.Invoke();

            foreach (Projectile projectile in FindObjectsByType<Projectile>(FindObjectsSortMode.None))
            {
                Destroy(projectile.gameObject);
            }

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

        public bool TryGetNextWaveDirection(out float angleDegrees)
        {
            return TryGetWaveDirection(currentWaveIndex + 1, out angleDegrees);
        }

        public bool TryGetCurrentOrNextStageDirection(out float angleDegrees)
        {
            if (gameState != null && gameState.IsWaveActive && currentWave != null && nextStageIndex >= 0)
            {
                return TryGetStageDirection(currentWave, nextStageIndex, out angleDegrees);
            }

            return TryGetNextWaveDirection(out angleDegrees);
        }

        public int GetNextWaveDirections(List<float> results)
        {
            if (results == null)
            {
                return 0;
            }

            results.Clear();
            AddWaveDirections(NextWaveConfig, results);
            return results.Count;
        }

        private bool TryGetWaveDirection(int waveIndex, out float angleDegrees)
        {
            angleDegrees = 0f;

            if (waves == null || waveIndex < 0 || waveIndex >= waves.Length)
            {
                return false;
            }

            return TryGetStageDirection(waves[waveIndex], 0, out angleDegrees);
        }

        private static void AddWaveDirections(WaveConfig wave, List<float> results)
        {
            if (wave == null || wave.SpawnGroups == null)
            {
                return;
            }

            for (int i = 0; i < wave.SpawnGroups.Length; i++)
            {
                EnemySpawnGroup group = wave.SpawnGroups[i];
                if (group != null && group.Count > 0)
                {
                    results.Add(group.AngleDegrees);
                }
            }
        }

        private static bool TryGetStageDirection(WaveConfig wave, int startIndex, out float angleDegrees)
        {
            angleDegrees = 0f;

            if (wave == null || wave.SpawnGroups == null || startIndex < 0)
            {
                return false;
            }

            for (int i = startIndex; i < wave.SpawnGroups.Length; i++)
            {
                EnemySpawnGroup group = wave.SpawnGroups[i];
                if (group != null && group.Count > 0)
                {
                    angleDegrees = group.AngleDegrees;
                    return true;
                }
            }

            return false;
        }
    }
}
