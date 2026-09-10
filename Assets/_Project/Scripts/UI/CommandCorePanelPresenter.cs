using UnityEngine;
using UnityEngine.UIElements;

namespace OrbitalDefense
{
    public sealed class CommandCorePanelPresenter : MonoBehaviour
    {
        [SerializeField] private CoreIntegrity coreIntegrity;
        [SerializeField] private ResourceWallet wallet;
        [SerializeField] private GameStateController gameState;
        [SerializeField] private LocalizationService localization;
        [SerializeField] private float feedbackDuration = 1.6f;

        private UIDocument document;
        private VisualElement panelRoot;
        private Label titleText, statusText, feedbackText;
        private CommandCoreUpgrade selectedCore;
        private float feedbackTimer;

        private void Awake()
        {
            coreIntegrity ??= FindAnyObjectByType<CoreIntegrity>();
            wallet ??= FindAnyObjectByType<ResourceWallet>();
            gameState ??= FindAnyObjectByType<GameStateController>();
            localization ??= FindAnyObjectByType<LocalizationService>();
            document = FindFirstObjectByType<UIDocument>();
        }

        private void OnEnable()
        {
            if (coreIntegrity != null) coreIntegrity.IntegrityChanged += HandleCoreChanged;
            if (wallet != null) wallet.MineralsChanged += HandleMineralsChanged;
            if (gameState != null) gameState.PhaseChanged += HandlePhaseChanged;
            if (localization != null) localization.LanguageChanged += Refresh;
        }

        private void OnDisable()
        {
            if (coreIntegrity != null) coreIntegrity.IntegrityChanged -= HandleCoreChanged;
            if (wallet != null) wallet.MineralsChanged -= HandleMineralsChanged;
            if (gameState != null) gameState.PhaseChanged -= HandlePhaseChanged;
            if (localization != null) localization.LanguageChanged -= Refresh;
            if (selectedCore != null) selectedCore.Changed -= Refresh;
        }

        private void Start()
        {
            BindUi();
            Hide();
        }

        private void BindUi()
        {
            if (document == null || document.rootVisualElement == null) return;
            VisualElement root = document.rootVisualElement;
            panelRoot = root.Q<VisualElement>("core-panel");
            titleText = root.Q<Label>("core-title");
            statusText = root.Q<Label>("core-status");
            feedbackText = root.Q<Label>("core-feedback");
            root.Q<Button>("core-close")?.RegisterCallback<ClickEvent>(_ => Hide());
        }

        public void SelectCore(CommandCoreUpgrade core)
        {
            if (selectedCore != null) selectedCore.Changed -= Refresh;
            selectedCore = core;
            if (selectedCore != null) selectedCore.Changed += Refresh;
            ClearFeedback();
            Refresh();
            SetVisible(true);
        }

        public void Hide()
        {
            if (selectedCore != null) selectedCore.Changed -= Refresh;
            selectedCore = null;
            ClearFeedback();
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            if (panelRoot != null) panelRoot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void Update()
        {
            if (feedbackText == null || feedbackTimer <= 0f) return;
            feedbackTimer -= Time.unscaledDeltaTime;
            if (feedbackTimer <= 0f) feedbackText.text = string.Empty;
        }

        private void Refresh()
        {
            if (titleText != null) titleText.text = Text("core.title");
            if (statusText != null)
            {
                int current = coreIntegrity != null ? coreIntegrity.CurrentIntegrity : 0;
                int max = coreIntegrity != null ? coreIntegrity.MaxIntegrity : 0;
                int mining = selectedCore != null ? selectedCore.MiningLevel : 0;
                int defense = selectedCore != null ? selectedCore.DefenseLevel : 0;
                int shield = selectedCore != null ? selectedCore.ShieldLevel : 0;
                int maxLevel = selectedCore != null ? selectedCore.MaxBranchLevel : 3;
                int shieldBlocks = selectedCore != null ? selectedCore.ShieldBlocksRemaining : 0;
                statusText.text = Text("core.status", current, max, mining, maxLevel, defense, maxLevel, shield, maxLevel, shieldBlocks);
            }
        }

        private void HandleCoreChanged(int current, int max) => Refresh();
        private void HandleMineralsChanged(int minerals) => Refresh();
        private void HandlePhaseChanged(GamePhase phase) => Refresh();
        private void ShowFeedback(string message)
        {
            if (feedbackText == null) return;
            feedbackText.text = message;
            feedbackTimer = feedbackDuration;
        }
        private void ClearFeedback()
        {
            feedbackTimer = 0f;
            if (feedbackText != null) feedbackText.text = string.Empty;
        }
        private string Text(string key, params object[] args) => localization != null ? localization.Text(key, args) : key;
    }
}
