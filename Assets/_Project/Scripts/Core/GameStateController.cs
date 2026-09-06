using System;
using UnityEngine;

namespace OrbitalDefense
{
    public sealed class GameStateController : MonoBehaviour
    {
        [SerializeField] private GamePhase initialPhase = GamePhase.BuildPhase;

        public event Action<GamePhase> PhaseChanged;

        public GamePhase CurrentPhase { get; private set; }

        public bool CanBuild => CurrentPhase == GamePhase.BuildPhase;
        public bool IsWaveActive => CurrentPhase == GamePhase.Wave;
        public bool IsGameOver => CurrentPhase == GamePhase.Victory || CurrentPhase == GamePhase.Defeat;

        private void Awake()
        {
            CurrentPhase = initialPhase;
        }

        private void Start()
        {
            PhaseChanged?.Invoke(CurrentPhase);
        }

        public void EnterBuildPhase()
        {
            SetPhase(GamePhase.BuildPhase);
        }

        public void EnterWave()
        {
            SetPhase(GamePhase.Wave);
        }

        public void EnterUpgradeChoice()
        {
            SetPhase(GamePhase.UpgradeChoice);
        }

        public void EnterVictory()
        {
            SetPhase(GamePhase.Victory);
        }

        public void EnterDefeat()
        {
            SetPhase(GamePhase.Defeat);
        }

        public void SetPhase(GamePhase phase)
        {
            if (CurrentPhase == phase)
            {
                return;
            }

            CurrentPhase = phase;
            PhaseChanged?.Invoke(CurrentPhase);
        }
    }
}

