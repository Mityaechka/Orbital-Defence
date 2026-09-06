using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitalDefense
{
    public sealed class BuildPanelPresenter : MonoBehaviour
    {
        [SerializeField] private BuildSystem buildSystem;
        [SerializeField] private BuildCatalog catalog;
        [SerializeField] private LocalizationService localization;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text selectedSlotText;
        [SerializeField] private Button mineButton;
        [SerializeField] private Button cannonButton;
        [SerializeField] private Button boosterButton;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button sellButton;
        [SerializeField] private TMP_Text feedbackText;
        [SerializeField] private RangePreview rangePreview;
        [SerializeField] private float feedbackDuration = 1.6f;

        private BuildSlot selectedSlot;
        private BuildSlotSelectionFeedback selectedFeedback;
        private float feedbackTimer;
        private string feedbackKey;
        private object[] feedbackArgs;

        private void Awake()
        {
            buildSystem ??= FindFirstObjectByType<BuildSystem>();
            localization ??= FindFirstObjectByType<LocalizationService>();
            panelRoot ??= gameObject;
        }

        private void OnEnable()
        {
            if (localization != null)
            {
                localization.LanguageChanged += Refresh;
            }
        }

        private void OnDisable()
        {
            if (localization != null)
            {
                localization.LanguageChanged -= Refresh;
            }
        }

        private void Start()
        {
            Hide();
        }

        private void Update()
        {
            if (feedbackText == null || feedbackTimer <= 0f)
            {
                return;
            }

            feedbackTimer -= Time.unscaledDeltaTime;
            if (feedbackTimer <= 0f)
            {
                feedbackText.text = string.Empty;
            }
        }

        public void SelectSlot(BuildSlot slot)
        {
            SetSelectedFeedback(false);
            selectedSlot = slot;
            selectedFeedback = selectedSlot != null ? selectedSlot.GetComponent<BuildSlotSelectionFeedback>() : null;
            SetSelectedFeedback(true);
            ClearFeedback();
            Refresh();
            panelRoot.SetActive(true);
        }

        public void BuildMine()
        {
            TryBuild(BuildingKind.Mine);
        }

        public void BuildCannon()
        {
            TryBuild(BuildingKind.Cannon);
        }

        public void BuildBooster()
        {
            TryBuild(BuildingKind.OrbitalBooster);
        }

        public void UpgradeSelected()
        {
            if (buildSystem != null && buildSystem.TryUpgrade(selectedSlot))
            {
                ClearFeedback();
                Refresh();
                return;
            }

            Building building = selectedSlot != null ? selectedSlot.CurrentBuilding : null;
            if (building != null && !building.CanUpgrade())
            {
                ShowFeedback("feedback.max_level");
            }
            else if (building != null && buildSystem != null && !buildSystem.CanAffordUpgrade(building))
            {
                ShowFeedback("feedback.need_minerals", buildSystem.GetUpgradeCost(building));
            }
            else
            {
                ShowFeedback("feedback.cannot_upgrade");
            }
        }

        public void SellSelected()
        {
            if (buildSystem != null && buildSystem.TrySell(selectedSlot))
            {
                Hide();
            }
        }

        public void Hide()
        {
            SetSelectedFeedback(false);
            selectedSlot = null;
            selectedFeedback = null;
            ClearFeedback();
            UpdateRangePreview();
            panelRoot.SetActive(false);
        }

        private void TryBuild(BuildingKind kind)
        {
            BuildingConfig config = FindConfig(kind);
            if (buildSystem != null && buildSystem.TryBuild(selectedSlot, config))
            {
                ClearFeedback();
                Hide();
                return;
            }

            if (config != null && buildSystem != null && !buildSystem.CanAffordBuild(config))
            {
                ShowFeedback("feedback.need_minerals", buildSystem.GetBuildCost(config));
            }
            else
            {
                ShowFeedback("feedback.cannot_build");
            }

            Refresh();
        }

        private BuildingConfig FindConfig(BuildingKind kind)
        {
            if (catalog == null || catalog.Buildings == null)
            {
                return null;
            }

            for (int i = 0; i < catalog.Buildings.Length; i++)
            {
                BuildingConfig config = catalog.Buildings[i];
                if (config != null && config.Kind == kind)
                {
                    return config;
                }
            }

            return null;
        }

        private void Refresh()
        {
            if (selectedSlotText != null)
            {
                selectedSlotText.text = GetSelectedSlotText();
            }

            RefreshButton(mineButton, BuildingKind.Mine);
            RefreshButton(cannonButton, BuildingKind.Cannon);
            RefreshButton(boosterButton, BuildingKind.OrbitalBooster);
            RefreshManagementButtons();
            UpdateRangePreview();
            RefreshFeedback();
        }

        private void RefreshButton(Button button, BuildingKind kind)
        {
            if (button == null)
            {
                return;
            }

            BuildingConfig config = FindConfig(kind);
            if (config == null)
            {
                button.gameObject.SetActive(false);
                return;
            }

            button.interactable = selectedSlot != null && selectedSlot.CanAccept(config);
            button.gameObject.SetActive(selectedSlot == null || !selectedSlot.IsOccupied);
            string name = config != null ? Localize(config.DisplayNameKey, config.DisplayName) : kind.ToString();
            SetButtonLabel(button, config != null && buildSystem != null ? Text("button.build_cost", name, buildSystem.GetBuildCost(config)) : name);
        }

        private void RefreshManagementButtons()
        {
            Building building = selectedSlot != null ? selectedSlot.CurrentBuilding : null;
            bool hasBuilding = building != null;

            if (upgradeButton != null)
            {
                upgradeButton.gameObject.SetActive(hasBuilding);
                upgradeButton.interactable = hasBuilding && building.CanUpgrade();
                int upgradeCost = hasBuilding && buildSystem != null ? buildSystem.GetUpgradeCost(building) : 0;
                SetButtonLabel(upgradeButton, hasBuilding ? Text("button.upgrade_cost", upgradeCost) : Text("button.upgrade"));
            }

            if (sellButton != null)
            {
                sellButton.gameObject.SetActive(hasBuilding);
                sellButton.interactable = hasBuilding;
                int refund = hasBuilding && building.Config != null ? Mathf.FloorToInt(building.Config.BuildCost * 0.5f) : 0;
                SetButtonLabel(sellButton, Text("button.sell_refund", refund));
            }
        }

        private void ShowFeedback(string key, params object[] args)
        {
            if (feedbackText == null)
            {
                return;
            }

            feedbackKey = key;
            feedbackArgs = args;
            feedbackText.text = Text(key, args);
            feedbackTimer = feedbackDuration;
        }

        private void ClearFeedback()
        {
            feedbackTimer = 0f;
            feedbackKey = null;
            feedbackArgs = null;
            if (feedbackText != null)
            {
                feedbackText.text = string.Empty;
            }
        }

        private void SetSelectedFeedback(bool selected)
        {
            if (selectedFeedback != null)
            {
                selectedFeedback.SetSelected(selected);
            }
        }

        private void UpdateRangePreview()
        {
            if (rangePreview == null)
            {
                return;
            }

            if (selectedSlot == null)
            {
                rangePreview.Hide();
                return;
            }

            Building building = selectedSlot.CurrentBuilding;
            if (building != null && building.Config != null && building.Config.Kind == BuildingKind.Cannon)
            {
                rangePreview.Show(building.transform, building.Config.Range * building.LevelMultiplier);
                return;
            }

            BuildingConfig cannonConfig = FindConfig(BuildingKind.Cannon);
            if (building == null && selectedSlot.CanAccept(cannonConfig))
            {
                rangePreview.Show(selectedSlot.PlacementPoint, cannonConfig.Range);
                return;
            }

            rangePreview.Hide();
        }

        private string GetSelectedSlotText()
        {
            if (selectedSlot == null)
            {
                return Text("build.select_slot");
            }

            Building building = selectedSlot.CurrentBuilding;
            if (building == null || building.Config == null)
            {
                return Text("build.moon_slot");
            }

            string name = Localize(building.Config.DisplayNameKey, building.Config.DisplayName);
            return Text("build.occupied_slot", name, building.Level, building.Config.MaxLevel);
        }

        private static void SetButtonLabel(Button button, string label)
        {
            TMP_Text text = button.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = label;
            }
        }

        private string Text(string key, params object[] args)
        {
            return localization != null ? localization.Text(key, args) : key;
        }

        private string Localize(string key, string fallback)
        {
            return localization != null ? localization.TextOrFallback(key, fallback) : fallback;
        }

        private void RefreshFeedback()
        {
            if (feedbackText != null && feedbackTimer > 0f && !string.IsNullOrEmpty(feedbackKey))
            {
                feedbackText.text = Text(feedbackKey, feedbackArgs);
            }
        }
    }
}
