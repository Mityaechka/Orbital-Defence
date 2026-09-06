using UnityEngine;

namespace OrbitalDefense
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private GameStateController gameState;
        [SerializeField] private CoreIntegrity coreIntegrity;
        [SerializeField] private TimeScaleController timeScaleController;

        private void Awake()
        {
            gameState ??= FindFirstObjectByType<GameStateController>();
            coreIntegrity ??= FindFirstObjectByType<CoreIntegrity>();
            timeScaleController ??= FindFirstObjectByType<TimeScaleController>();
        }

        private void OnEnable()
        {
            if (coreIntegrity != null)
            {
                coreIntegrity.Depleted += HandleCoreDepleted;
            }

            if (gameState != null)
            {
                gameState.PhaseChanged += HandlePhaseChanged;
            }
        }

        private void OnDisable()
        {
            if (coreIntegrity != null)
            {
                coreIntegrity.Depleted -= HandleCoreDepleted;
            }

            if (gameState != null)
            {
                gameState.PhaseChanged -= HandlePhaseChanged;
            }
        }

        private void Start()
        {
            gameState?.EnterBuildPhase();
            ApplyTimeScaleForPhase(gameState != null ? gameState.CurrentPhase : GamePhase.BuildPhase);
        }

        private void HandleCoreDepleted()
        {
            gameState?.EnterDefeat();
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            ApplyTimeScaleForPhase(phase);
        }

        private void ApplyTimeScaleForPhase(GamePhase phase)
        {
            if (timeScaleController == null)
            {
                return;
            }

            if (phase == GamePhase.Wave)
            {
                timeScaleController.SetNormal();
                return;
            }

            timeScaleController.Pause();
        }
    }
}
