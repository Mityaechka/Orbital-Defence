using UnityEngine;
using UnityEngine.UIElements;

namespace OrbitalDefense
{
    public sealed class UpgradePanelPresenter : MonoBehaviour
    {
        [SerializeField] private GameStateController gameState;
        [SerializeField] private UpgradeSystem upgradeSystem;
        [SerializeField] private LocalizationService localization;

        private UIDocument document;
        private VisualElement panelRoot;
        private Label titleText;
        private Button[] choiceButtons;

        private void Awake()
        {
            gameState ??= FindFirstObjectByType<GameStateController>();
            upgradeSystem ??= FindFirstObjectByType<UpgradeSystem>();
            localization ??= FindFirstObjectByType<LocalizationService>();
            document = FindFirstObjectByType<UIDocument>();
        }

        private void OnEnable()
        {
            if (gameState != null) gameState.PhaseChanged += HandlePhaseChanged;
            if (localization != null) localization.LanguageChanged += RefreshCurrentPhase;
        }

        private void OnDisable()
        {
            if (gameState != null) gameState.PhaseChanged -= HandlePhaseChanged;
            if (localization != null) localization.LanguageChanged -= RefreshCurrentPhase;
        }

        private void Start()
        {
            BindUi();
            HandlePhaseChanged(gameState != null ? gameState.CurrentPhase : GamePhase.BuildPhase);
        }

        private void BindUi()
        {
            if (document == null || document.rootVisualElement == null) return;
            VisualElement root = document.rootVisualElement;
            panelRoot = root.Q<VisualElement>("upgrade-panel");
            titleText = root.Q<Label>("upgrade-title");
            choiceButtons = new[] { root.Q<Button>("upgrade-choice-0"), root.Q<Button>("upgrade-choice-1"), root.Q<Button>("upgrade-choice-2") };
            if (choiceButtons[0] != null) choiceButtons[0].clicked += ChooseFirst;
            if (choiceButtons[1] != null) choiceButtons[1].clicked += ChooseSecond;
            if (choiceButtons[2] != null) choiceButtons[2].clicked += ChooseThird;
        }

        public void ChooseFirst() => Choose(0);
        public void ChooseSecond() => Choose(1);
        public void ChooseThird() => Choose(2);
        private void Choose(int index) => upgradeSystem?.ChooseUpgrade(GetUpgrade(index));

        private void HandlePhaseChanged(GamePhase phase)
        {
            bool isUpgrade = phase == GamePhase.UpgradeChoice;
            if (panelRoot != null) panelRoot.style.display = isUpgrade ? DisplayStyle.Flex : DisplayStyle.None;
            if (titleText != null) titleText.text = isUpgrade ? Text("upgrade.choose_title") : string.Empty;
            RefreshChoices(isUpgrade);
        }

        private void RefreshChoices(bool isUpgrade)
        {
            if (choiceButtons == null) return;
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                Button button = choiceButtons[i];
                UpgradeConfig upgrade = GetUpgrade(i);
                bool available = isUpgrade && upgrade != null;
                if (button == null) continue;
                button.style.display = available ? DisplayStyle.Flex : DisplayStyle.None;
                button.SetEnabled(available && upgradeSystem != null && upgradeSystem.CanChooseUpgrade(upgrade));
                button.text = available ? FormatUpgrade(upgrade) : string.Empty;
            }
        }

        private UpgradeConfig GetUpgrade(int index)
        {
            UpgradeConfig[] upgrades = upgradeSystem?.AvailableUpgrades;
            return upgrades != null && index >= 0 && index < upgrades.Length ? upgrades[index] : null;
        }

        private string FormatUpgrade(UpgradeConfig upgrade)
        {
            int level = upgradeSystem != null ? upgradeSystem.GetUpgradeLevel(upgrade) : 0;
            string suffix = level > 0 ? Text("upgrade.level_suffix", level, 3) : string.Empty;
            string name = localization != null ? localization.TextOrFallback(upgrade.DisplayNameKey, upgrade.DisplayName) : upgrade.DisplayName;
            string description = localization != null ? localization.TextOrFallback(upgrade.DescriptionKey, upgrade.Description) : upgrade.Description;
            return $"{name}\n{description}{suffix}";
        }

        private void RefreshCurrentPhase() => HandlePhaseChanged(gameState != null ? gameState.CurrentPhase : GamePhase.BuildPhase);
        private string Text(string key, params object[] args) => localization != null ? localization.Text(key, args) : key;
    }
}
