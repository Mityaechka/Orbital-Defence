using System.Collections.Generic;
using System;
using UnityEngine;

namespace OrbitalDefense
{
    [ExecuteAlways]
    public sealed class OrbitalSetupController : MonoBehaviour
    {
        [SerializeField] private GameStateController gameState;
        [SerializeField] private float rotationStepDegrees = 30f;

        private readonly List<OrbitMover> registeredMoons = new();
        private float[] lockedAngleSnapshot = Array.Empty<float>();
        private bool isLocked;

        public event Action<OrbitMover> SelectedMoonChanged;

        public OrbitMover SelectedMoon { get; private set; }
        public bool CanEditSetup => gameState == null || gameState.CurrentPhase == GamePhase.BuildPhase;
        public bool IsReadyForWave => CanEditSetup && registeredMoons.Count > 0;
        public float RotationStepDegrees => rotationStepDegrees;
        public IReadOnlyList<float> LockedAngleSnapshot => lockedAngleSnapshot;

        private void Awake()
        {
            gameState ??= FindFirstObjectByType<GameStateController>();
        }

        private void OnEnable()
        {
            if (gameState != null)
            {
                gameState.PhaseChanged += HandlePhaseChanged;
            }

            SyncWithCurrentPhase();
            RegisterExistingMoons();
        }

        private void OnDisable()
        {
            if (gameState != null)
            {
                gameState.PhaseChanged -= HandlePhaseChanged;
            }

            registeredMoons.Clear();
        }

        public void RegisterMoon(OrbitMover moon)
        {
            if (moon == null)
            {
                return;
            }

            CleanupNullEntries();
            if (registeredMoons.Contains(moon))
            {
                return;
            }

            registeredMoons.Add(moon);
            ApplyLockState(moon);
        }

        public void UnregisterMoon(OrbitMover moon)
        {
            if (moon == null)
            {
                return;
            }

            registeredMoons.Remove(moon);
            if (SelectedMoon == moon)
            {
                SelectMoon(null);
            }
        }

        public void ResetAllMoons()
        {
            if (!CanEditSetup)
            {
                return;
            }

            CleanupNullEntries();
            for (int i = 0; i < registeredMoons.Count; i++)
            {
                registeredMoons[i].ResetAngle();
            }
        }

        public bool ResetSelectedMoon()
        {
            if (SelectedMoon == null || !CanEditSetup)
            {
                return false;
            }

            SelectedMoon.ResetAngle();
            return true;
        }

        public bool SelectMoon(OrbitMover moon)
        {
            if (moon != null && (!CanEditSetup || !registeredMoons.Contains(moon)))
            {
                return false;
            }

            if (SelectedMoon == moon)
            {
                return true;
            }

            SelectedMoon = moon;
            SelectedMoonChanged?.Invoke(SelectedMoon);
            return true;
        }

        public bool RotateSelectedByStep(int direction)
        {
            if (SelectedMoon == null || !CanEditSetup || direction == 0)
            {
                return false;
            }

            SelectedMoon.RotateBy(Mathf.Sign(direction) * rotationStepDegrees);
            return true;
        }

        public bool LockForWave()
        {
            if (!IsReadyForWave)
            {
                return false;
            }

            CleanupNullEntries();
            lockedAngleSnapshot = new float[registeredMoons.Count];
            for (int i = 0; i < registeredMoons.Count; i++)
            {
                lockedAngleSnapshot[i] = registeredMoons[i].CurrentAngleDegrees;
            }

            SelectMoon(null);
            SetLocked(true);
            return true;
        }

        public void UnlockForSetup()
        {
            SetLocked(false);
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            SyncWithCurrentPhase(phase);
        }

        private void SyncWithCurrentPhase()
        {
            SyncWithCurrentPhase(gameState != null ? gameState.CurrentPhase : GamePhase.BuildPhase);
        }

        private void SyncWithCurrentPhase(GamePhase phase)
        {
            bool shouldLock = phase == GamePhase.Wave || phase == GamePhase.UpgradeChoice || phase == GamePhase.Victory;
            if (shouldLock)
            {
                SelectMoon(null);
            }

            SetLocked(shouldLock);
        }

        private void RegisterExistingMoons()
        {
            CleanupNullEntries();
            OrbitMover[] orbitMovers = FindObjectsByType<OrbitMover>(FindObjectsSortMode.None);
            for (int i = 0; i < orbitMovers.Length; i++)
            {
                RegisterMoon(orbitMovers[i]);
            }
        }

        private void ApplyLockState(OrbitMover moon)
        {
            if (moon == null)
            {
                return;
            }

            if (isLocked)
            {
                moon.LockPosition();
            }
            else
            {
                moon.UnlockPosition();
            }
        }

        private void SetLocked(bool locked)
        {
            if (isLocked == locked)
            {
                return;
            }

            isLocked = locked;
            CleanupNullEntries();

            for (int i = 0; i < registeredMoons.Count; i++)
            {
                ApplyLockState(registeredMoons[i]);
            }
        }

        private void CleanupNullEntries()
        {
            for (int i = registeredMoons.Count - 1; i >= 0; i--)
            {
                if (registeredMoons[i] == null)
                {
                    registeredMoons.RemoveAt(i);
                }
            }
        }
    }
}
