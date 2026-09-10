using UnityEngine;
using UnityEngine.UIElements;

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

        private UIDocument document;
        private Label mineralsText, coreText, waveText, phaseText, speedText;
        private Button startWaveButton, speedButton;

        private void Awake()
        {
            wallet ??= FindFirstObjectByType<ResourceWallet>();
            coreIntegrity ??= FindFirstObjectByType<CoreIntegrity>();
            gameState ??= FindFirstObjectByType<GameStateController>();
            waveSystem ??= FindFirstObjectByType<WaveSystem>();
            timeScaleController ??= FindFirstObjectByType<TimeScaleController>();
            localization ??= FindFirstObjectByType<LocalizationService>();
            document = FindFirstObjectByType<UIDocument>();
        }

        private void OnEnable()
        {
            if (wallet != null) wallet.MineralsChanged += UpdateMinerals;
            if (coreIntegrity != null) coreIntegrity.IntegrityChanged += UpdateCore;
            if (gameState != null) gameState.PhaseChanged += UpdatePhase;
            if (timeScaleController != null) timeScaleController.TimeScaleChanged += UpdateSpeed;
            if (waveSystem != null)
            {
                waveSystem.WaveStarted += UpdateWave;
                waveSystem.WaveCompleted += UpdateWave;
            }
            if (localization != null) localization.LanguageChanged += RefreshAll;
        }

        private void OnDisable()
        {
            if (wallet != null) wallet.MineralsChanged -= UpdateMinerals;
            if (coreIntegrity != null) coreIntegrity.IntegrityChanged -= UpdateCore;
            if (gameState != null) gameState.PhaseChanged -= UpdatePhase;
            if (timeScaleController != null) timeScaleController.TimeScaleChanged -= UpdateSpeed;
            if (waveSystem != null)
            {
                waveSystem.WaveStarted -= UpdateWave;
                waveSystem.WaveCompleted -= UpdateWave;
            }
            if (localization != null) localization.LanguageChanged -= RefreshAll;
        }

        private void Start()
        {
            BindUi();
            RefreshAll();
        }

        private void BindUi()
        {
            if (document == null || document.rootVisualElement == null) return;
            VisualElement root = document.rootVisualElement;
            mineralsText = root.Q<Label>("hud-minerals");
            coreText = root.Q<Label>("hud-core");
            waveText = root.Q<Label>("hud-wave");
            phaseText = root.Q<Label>("hud-phase");
            speedText = root.Q<Label>("hud-speed");
            startWaveButton = root.Q<Button>("hud-start-wave");
            speedButton = root.Q<Button>("hud-speed-button");
            startWaveButton?.RegisterCallback<ClickEvent>(_ => OnStartWaveClicked());
            speedButton?.RegisterCallback<ClickEvent>(_ => OnSpeedClicked());
        }

        public void OnStartWaveClicked()
        {
            waveSystem?.StartNextWave();
            UpdateWave();
        }

        public void OnSpeedClicked()
        {
            if (gameState != null && !gameState.IsWaveActive) return;
            timeScaleController?.ToggleSpeed();
        }

        private void RefreshAll()
        {
            if (wallet != null) UpdateMinerals(wallet.Minerals);
            if (coreIntegrity != null) UpdateCore(coreIntegrity.CurrentIntegrity, coreIntegrity.MaxIntegrity);
            if (gameState != null) UpdatePhase(gameState.CurrentPhase);
            if (waveSystem != null) UpdateWave();
            if (timeScaleController != null) UpdateSpeed(timeScaleController.CurrentScale);
            if (startWaveButton != null) startWaveButton.text = Text("button.start_wave");
            if (speedButton != null)
            {
                speedButton.text = "⏩";
                speedButton.tooltip = Text("button.speed");
            }
        }

        private void UpdateMinerals(int value) { if (mineralsText != null) mineralsText.text = Text("hud.minerals", value); }
        private void UpdateCore(int current, int max) { if (coreText != null) coreText.text = Text("hud.core", current, max); }
        private void UpdatePhase(GamePhase phase)
        {
            if (phaseText != null) phaseText.text = Text(GetPhaseKey(phase));
            RefreshStartWaveButton(phase);
        }
        private void UpdateSpeed(float scale) { if (speedText != null) speedText.text = Text("hud.speed", scale); }
        private void UpdateWave()
        {
            if (waveText != null && waveSystem != null) waveText.text = Text("hud.wave", waveSystem.CurrentWaveNumber, waveSystem.TotalWaves);
            RefreshStartWaveButton(gameState != null ? gameState.CurrentPhase : GamePhase.BuildPhase);
        }
        private void UpdateWave(int _, int __) => UpdateWave();
        private void RefreshStartWaveButton(GamePhase phase)
        {
            if (startWaveButton == null) return;
            bool show = phase == GamePhase.BuildPhase;
            startWaveButton.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            startWaveButton.SetEnabled(show && waveSystem != null && waveSystem.CanStartNextWave);
        }
        private string Text(string key, params object[] args) => localization != null ? localization.Text(key, args) : key;
        private static string GetPhaseKey(GamePhase phase) => phase switch
        {
            GamePhase.BuildPhase => "phase.build",
            GamePhase.Wave => "phase.wave",
            GamePhase.UpgradeChoice => "phase.upgrade",
            GamePhase.Victory => "phase.victory",
            GamePhase.Defeat => "phase.defeat",
            _ => "phase.boot"
        };
    }
}
