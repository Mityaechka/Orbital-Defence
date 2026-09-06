using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitalDefense
{
    public sealed class UpgradePanelPresenter : MonoBehaviour
    {
        [SerializeField] private GameStateController gameState;
        [SerializeField] private UpgradeSystem upgradeSystem;
        [SerializeField] private LocalizationService localization;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button[] choiceButtons;
        [SerializeField] private TMP_Text[] choiceLabels;

        private void Awake()
        {
            gameState ??= FindFirstObjectByType<GameStateController>();
            upgradeSystem ??= FindFirstObjectByType<UpgradeSystem>();
            localization ??= FindFirstObjectByType<LocalizationService>();
            panelRoot ??= gameObject;
        }

        private void OnEnable()
        {
            if (gameState != null)
            {
                gameState.PhaseChanged += HandlePhaseChanged;
            }

            if (localization != null)
            {
                localization.LanguageChanged += RefreshCurrentPhase;
            }
        }

        private void OnDisable()
        {
            if (gameState != null)
            {
                gameState.PhaseChanged -= HandlePhaseChanged;
            }

            if (localization != null)
            {
                localization.LanguageChanged -= RefreshCurrentPhase;
            }
        }

        private void Start()
        {
            HandlePhaseChanged(gameState != null ? gameState.CurrentPhase : GamePhase.BuildPhase);
        }

        public void ChooseFirst()
        {
            Choose(0);
        }

        public void ChooseSecond()
        {
            Choose(1);
        }

        public void ChooseThird()
        {
            Choose(2);
        }

        private void Choose(int index)
        {
            UpgradeConfig upgrade = GetUpgrade(index);
            upgradeSystem?.ChooseUpgrade(upgrade);
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            bool isUpgrade = phase == GamePhase.UpgradeChoice;
            panelRoot.SetActive(isUpgrade);

            if (titleText != null)
            {
                titleText.text = isUpgrade ? Text("upgrade.choose_title") : string.Empty;
            }

            RefreshChoices(isUpgrade);
        }

        private void RefreshChoices(bool isUpgrade)
        {
            int buttonCount = choiceButtons == null ? 0 : choiceButtons.Length;
            for (int i = 0; i < buttonCount; i++)
            {
                Button button = choiceButtons[i];
                if (button == null)
                {
                    continue;
                }

                UpgradeConfig upgrade = GetUpgrade(i);
                bool hasUpgrade = isUpgrade && upgrade != null;
                button.gameObject.SetActive(hasUpgrade);
                button.interactable = hasUpgrade && upgradeSystem != null && upgradeSystem.CanChooseUpgrade(upgrade);

                TMP_Text label = GetLabel(i);
                if (label != null)
                {
                    label.text = hasUpgrade ? FormatUpgrade(upgrade) : string.Empty;
                }
            }
        }

        private UpgradeConfig GetUpgrade(int index)
        {
            if (upgradeSystem == null || upgradeSystem.AvailableUpgrades == null)
            {
                return null;
            }

            UpgradeConfig[] upgrades = upgradeSystem.AvailableUpgrades;
            return index >= 0 && index < upgrades.Length ? upgrades[index] : null;
        }

        private TMP_Text GetLabel(int index)
        {
            if (choiceLabels == null || index < 0 || index >= choiceLabels.Length)
            {
                return null;
            }

            return choiceLabels[index];
        }

        private string FormatUpgrade(UpgradeConfig upgrade)
        {
            int level = upgradeSystem != null ? upgradeSystem.GetUpgradeLevel(upgrade) : 0;
            string suffix = level > 0 ? Text("upgrade.level_suffix", level, 3) : string.Empty;
            string name = localization != null ? localization.TextOrFallback(upgrade.DisplayNameKey, upgrade.DisplayName) : upgrade.DisplayName;
            string description = localization != null ? localization.TextOrFallback(upgrade.DescriptionKey, upgrade.Description) : upgrade.Description;
            return $"{name}\n{description}{suffix}";
        }

        private void RefreshCurrentPhase()
        {
            HandlePhaseChanged(gameState != null ? gameState.CurrentPhase : GamePhase.BuildPhase);
        }

        private string Text(string key, params object[] args)
        {
            return localization != null ? localization.Text(key, args) : key;
        }
    }
}
