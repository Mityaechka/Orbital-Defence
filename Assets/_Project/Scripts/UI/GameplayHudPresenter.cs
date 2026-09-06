using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitalDefense
{
    public sealed class GameplayHudPresenter : MonoBehaviour
    {
        [SerializeField] private ResourceWallet wallet;
        [SerializeField] private CoreIntegrity coreIntegrity;
        [SerializeField] private GameStateController gameState;
        [SerializeField] private WaveSystem waveSystem;
        [SerializeField] private TimeScaleController timeScaleController;
        [SerializeField] private LocalizationService localization;
        [SerializeField] private TMP_Text mineralsText;
        [SerializeField] private TMP_Text coreText;
        [SerializeField] private TMP_Text waveText;
        [SerializeField] private TMP_Text phaseText;
        [SerializeField] private TMP_Text speedText;
        [SerializeField] private Button startWaveButton;
        [SerializeField] private Button speedButton;

        private void Awake()
        {
            wallet ??= FindFirstObjectByType<ResourceWallet>();
            coreIntegrity ??= FindFirstObjectByType<CoreIntegrity>();
            gameState ??= FindFirstObjectByType<GameStateController>();
            waveSystem ??= FindFirstObjectByType<WaveSystem>();
            timeScaleController ??= FindFirstObjectByType<TimeScaleController>();
            localization ??= FindFirstObjectByType<LocalizationService>();
        }

        private void OnEnable()
        {
            if (wallet != null)
            {
                wallet.MineralsChanged += UpdateMinerals;
            }

            if (coreIntegrity != null)
            {
                coreIntegrity.IntegrityChanged += UpdateCore;
            }

            if (gameState != null)
            {
                gameState.PhaseChanged += UpdatePhase;
            }

            if (timeScaleController != null)
            {
                timeScaleController.TimeScaleChanged += UpdateSpeed;
            }

            if (waveSystem != null)
            {
                waveSystem.WaveStarted += UpdateWave;
                waveSystem.WaveCompleted += UpdateWave;
            }

            if (localization != null)
            {
                localization.LanguageChanged += RefreshAll;
            }
        }

        private void OnDisable()
        {
            if (wallet != null)
            {
                wallet.MineralsChanged -= UpdateMinerals;
            }

            if (coreIntegrity != null)
            {
                coreIntegrity.IntegrityChanged -= UpdateCore;
            }

            if (gameState != null)
            {
                gameState.PhaseChanged -= UpdatePhase;
            }

            if (timeScaleController != null)
            {
                timeScaleController.TimeScaleChanged -= UpdateSpeed;
            }

            if (waveSystem != null)
            {
                waveSystem.WaveStarted -= UpdateWave;
                waveSystem.WaveCompleted -= UpdateWave;
            }

            if (localization != null)
            {
                localization.LanguageChanged -= RefreshAll;
            }
        }

        private void Start()
        {
            RefreshAll();
        }

        private void RefreshAll()
        {
            if (wallet != null)
            {
                UpdateMinerals(wallet.Minerals);
            }

            if (coreIntegrity != null)
            {
                UpdateCore(coreIntegrity.CurrentIntegrity, coreIntegrity.MaxIntegrity);
            }

            if (gameState != null)
            {
                UpdatePhase(gameState.CurrentPhase);
            }

            if (waveSystem != null)
            {
                UpdateWave();
            }

            if (timeScaleController != null)
            {
                UpdateSpeed(timeScaleController.CurrentScale);
            }

            SetButtonLabel(startWaveButton, Text("button.start_wave"));
            SetButtonLabel(speedButton, Text("button.speed"));
        }

        public void OnStartWaveClicked()
        {
            waveSystem?.StartNextWave();
            UpdateWave();
        }

        public void OnSpeedClicked()
        {
            if (gameState != null && !gameState.IsWaveActive)
            {
                return;
            }

            timeScaleController?.ToggleSpeed();
        }

        private void UpdateMinerals(int minerals)
        {
            if (mineralsText != null)
            {
                mineralsText.text = Text("hud.minerals", minerals);
            }
        }

        private void UpdateCore(int current, int max)
        {
            if (coreText != null)
            {
                coreText.text = Text("hud.core", current, max);
            }
        }

        private void UpdatePhase(GamePhase phase)
        {
            if (phaseText != null)
            {
                phaseText.text = Text(GetPhaseKey(phase));
            }

            if (startWaveButton != null)
            {
                RefreshStartWaveButton(phase);
            }
        }

        private void UpdateSpeed(float scale)
        {
            if (speedText != null)
            {
                speedText.text = Text("hud.speed", scale);
            }
        }

        private void UpdateWave()
        {
            if (waveText != null && waveSystem != null)
            {
                waveText.text = Text("hud.wave", waveSystem.CurrentWaveNumber, waveSystem.TotalWaves);
            }

            RefreshStartWaveButton(gameState != null ? gameState.CurrentPhase : GamePhase.BuildPhase);
        }

        private void UpdateWave(int _, int __)
        {
            UpdateWave();
        }

        private string Text(string key, params object[] args)
        {
            return localization != null ? localization.Text(key, args) : key;
        }

        private static string GetPhaseKey(GamePhase phase)
        {
            return phase switch
            {
                GamePhase.BuildPhase => "phase.build",
                GamePhase.Wave => "phase.wave",
                GamePhase.UpgradeChoice => "phase.upgrade",
                GamePhase.Victory => "phase.victory",
                GamePhase.Defeat => "phase.defeat",
                _ => "phase.boot"
            };
        }

        private void RefreshStartWaveButton(GamePhase phase)
        {
            if (startWaveButton == null)
            {
                return;
            }

            bool shouldShow = phase == GamePhase.BuildPhase;
            startWaveButton.gameObject.SetActive(shouldShow);
            startWaveButton.interactable = shouldShow && waveSystem != null && waveSystem.CanStartNextWave;
        }

        private static void SetButtonLabel(Button button, string label)
        {
            TMP_Text text = button != null ? button.GetComponentInChildren<TMP_Text>() : null;
            if (text != null)
            {
                text.text = label;
            }
        }
    }
}
