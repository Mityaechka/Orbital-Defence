using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace OrbitalDefense
{
    public sealed class BuildPanelPresenter : MonoBehaviour
    {
        [SerializeField] private BuildSystem buildSystem;
        [SerializeField] private BuildCatalog catalog;
        [SerializeField] private LocalizationService localization;
        [SerializeField] private RangePreview rangePreview;
        [SerializeField] private float feedbackDuration = 1.6f;

        private UIDocument document;
        private VisualElement panelRoot;
        private Label selectedSlotText, statsText, feedbackText;
        private Button mineButton, cannonButton, boosterButton, upgradeButton, sellButton;
        private BuildSlot selectedSlot;
        private BuildSlotSelectionFeedback selectedFeedback;
        private float feedbackTimer;
        private string feedbackKey;
        private object[] feedbackArgs;

        private void Awake()
        {
            buildSystem ??= FindFirstObjectByType<BuildSystem>();
            localization ??= FindFirstObjectByType<LocalizationService>();
            document = FindFirstObjectByType<UIDocument>();
        }

        private void OnEnable()
        {
            if (localization != null) localization.LanguageChanged += Refresh;
        }

        private void OnDisable()
        {
            if (localization != null) localization.LanguageChanged -= Refresh;
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
            panelRoot = root.Q<VisualElement>("build-panel");
            selectedSlotText = root.Q<Label>("build-selected-slot");
            statsText = root.Q<Label>("build-stats");
            feedbackText = root.Q<Label>("build-feedback");
            mineButton = root.Q<Button>("build-mine");
            cannonButton = root.Q<Button>("build-cannon");
            upgradeButton = root.Q<Button>("build-upgrade");
            sellButton = root.Q<Button>("build-sell");
            mineButton?.RegisterCallback<ClickEvent>(_ => BuildMine());
            cannonButton?.RegisterCallback<ClickEvent>(_ => BuildCannon());
            upgradeButton?.RegisterCallback<ClickEvent>(_ => UpgradeSelected());
            sellButton?.RegisterCallback<ClickEvent>(_ => SellSelected());
        }

        private void Update()
        {
            if (feedbackText == null || feedbackTimer <= 0f) return;
            feedbackTimer -= Time.unscaledDeltaTime;
            if (feedbackTimer <= 0f) feedbackText.text = string.Empty;
        }

        public void SelectSlot(BuildSlot slot)
        {
            SetSelectedFeedback(false);
            selectedSlot = slot;
            selectedFeedback = selectedSlot != null ? selectedSlot.GetComponent<BuildSlotSelectionFeedback>() : null;
            SetSelectedFeedback(true);
            ClearFeedback();
            Refresh();
            SetVisible(true);
        }

        public void BuildMine() => TryBuild(BuildingKind.Mine);
        public void BuildCannon() => TryBuild(BuildingKind.Cannon);
        public void BuildBooster() => TryBuild(BuildingKind.OrbitalBooster);

        public void UpgradeSelected()
        {
            if (buildSystem != null && buildSystem.TryUpgrade(selectedSlot))
            {
                ClearFeedback();
                Refresh();
                return;
            }

            Building building = selectedSlot != null ? selectedSlot.CurrentBuilding : null;
            if (building != null && !building.CanUpgrade()) ShowFeedback("feedback.max_level");
            else if (building != null && buildSystem != null && !buildSystem.CanAffordUpgrade(building)) ShowFeedback("feedback.need_minerals", buildSystem.GetUpgradeCost(building));
            else ShowFeedback("feedback.cannot_upgrade");
        }

        public void SellSelected()
        {
            if (buildSystem != null && buildSystem.TrySell(selectedSlot)) Hide();
        }

        public void Hide()
        {
            SetSelectedFeedback(false);
            selectedSlot = null;
            selectedFeedback = null;
            ClearFeedback();
            UpdateRangePreview();
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            if (panelRoot != null) panelRoot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
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
            if (config != null && buildSystem != null && !buildSystem.CanAffordBuild(config)) ShowFeedback("feedback.need_minerals", buildSystem.GetBuildCost(config));
            else ShowFeedback("feedback.cannot_build");
            Refresh();
        }

        private BuildingConfig FindConfig(BuildingKind kind)
        {
            if (catalog?.Buildings == null) return null;
            for (int i = 0; i < catalog.Buildings.Length; i++)
            {
                BuildingConfig config = catalog.Buildings[i];
                if (config != null && config.Kind == kind) return config;
            }
            return null;
        }

        private void Refresh()
        {
            if (selectedSlotText != null) selectedSlotText.text = GetSelectedSlotText();
            RefreshStatsText();
            RefreshButton(mineButton, BuildingKind.Mine);
            RefreshButton(cannonButton, BuildingKind.Cannon);
            RefreshManagementButtons();
            UpdateRangePreview();
            RefreshFeedback();
        }

        private void RefreshButton(Button button, BuildingKind kind)
        {
            if (button == null) return;
            BuildingConfig config = FindConfig(kind);
            if (config == null) { button.style.display = DisplayStyle.None; return; }
            button.SetEnabled(selectedSlot != null && selectedSlot.CanAccept(config));
            button.style.display = selectedSlot == null || !selectedSlot.IsOccupied ? DisplayStyle.Flex : DisplayStyle.None;
            string name = Localize(config.DisplayNameKey, config.DisplayName);
            button.text = buildSystem != null ? Text("button.build_cost", name, buildSystem.GetBuildCost(config)) : name;
        }

        private void RefreshManagementButtons()
        {
            Building building = selectedSlot != null ? selectedSlot.CurrentBuilding : null;
            bool hasBuilding = building != null;
            if (upgradeButton != null)
            {
                upgradeButton.style.display = hasBuilding ? DisplayStyle.Flex : DisplayStyle.None;
                upgradeButton.SetEnabled(hasBuilding && building.CanUpgrade());
                int cost = hasBuilding && buildSystem != null ? buildSystem.GetUpgradeCost(building) : 0;
                upgradeButton.text = hasBuilding ? Text("button.upgrade_cost", cost) : Text("button.upgrade");
            }
            if (sellButton != null)
            {
                sellButton.style.display = hasBuilding ? DisplayStyle.Flex : DisplayStyle.None;
                sellButton.SetEnabled(hasBuilding);
                int refund = hasBuilding && building.Config != null ? Mathf.FloorToInt(building.Config.BuildCost * 0.5f) : 0;
                sellButton.text = Text("button.sell_refund", refund);
            }
        }

        private void ShowFeedback(string key, params object[] args)
        {
            if (feedbackText == null) return;
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
            if (feedbackText != null) feedbackText.text = string.Empty;
        }

        private void SetSelectedFeedback(bool selected) { if (selectedFeedback != null) selectedFeedback.SetSelected(selected); }

        private void UpdateRangePreview()
        {
            if (rangePreview == null) return;
            if (selectedSlot == null) { rangePreview.Hide(); return; }
            Building building = selectedSlot.CurrentBuilding;
            if (building != null && building.Config != null && building.Config.Kind == BuildingKind.Cannon)
            {
                rangePreview.Show(building.transform, building.Config.Range * building.LevelMultiplier);
                return;
            }
            BuildingConfig cannonConfig = FindConfig(BuildingKind.Cannon);
            if (building == null && cannonConfig != null && selectedSlot.CanAccept(cannonConfig)) rangePreview.Show(selectedSlot.PlacementPoint, cannonConfig.Range);
            else rangePreview.Hide();
        }

        private void RefreshStatsText() { if (statsText != null) statsText.text = GetStatsText(); }
        private string GetStatsText()
        {
            if (selectedSlot == null) return Text("build.stats.empty");
            Building building = selectedSlot.CurrentBuilding;
            return building != null && building.Config != null ? BuildBuildingStats(building.Config, building.Level) : BuildAvailableStats();
        }

        private string BuildAvailableStats()
        {
            if (catalog?.Buildings == null || selectedSlot == null) return Text("build.stats.none");
            StringBuilder builder = new();
            builder.AppendLine(Text("build.stats.available"));
            bool wroteAny = false;
            for (int i = 0; i < catalog.Buildings.Length; i++)
            {
                BuildingConfig config = catalog.Buildings[i];
                if (config == null || !selectedSlot.CanAccept(config)) continue;
                if (wroteAny) builder.AppendLine();
                AppendBuildingSummary(builder, config);
                wroteAny = true;
            }
            return wroteAny ? builder.ToString().TrimEnd() : Text("build.stats.none");
        }

        private string BuildBuildingStats(BuildingConfig config, int level)
        {
            StringBuilder builder = new();
            AppendBuildingStats(builder, config, level);
            return builder.ToString().TrimEnd();
        }

        private void AppendBuildingStats(StringBuilder builder, BuildingConfig config, int level)
        {
            if (builder == null || config == null) return;
            builder.AppendLine(Localize(config.DisplayNameKey, config.DisplayName));
            builder.AppendLine(Text("build.stats.level", level, config.MaxLevel));
            switch (config.Kind)
            {
                case BuildingKind.Mine:
                    builder.AppendLine(Text("build.stats.production", Mathf.RoundToInt(config.ProductionAmount * GetLevelMultiplier(level)), Mathf.Max(0.1f, config.ProductionInterval)));
                    break;
                case BuildingKind.Cannon:
                    builder.AppendLine(Text("build.stats.damage", Mathf.RoundToInt(config.Damage * GetLevelMultiplier(level))));
                    builder.AppendLine(Text("build.stats.fire_rate", config.FireRate));
                    builder.AppendLine(Text("build.stats.range", config.Range * GetLevelMultiplier(level)));
                    builder.AppendLine(Text("build.stats.projectile_speed", config.ProjectileSpeed));
                    break;
                case BuildingKind.OrbitalBooster:
                    builder.AppendLine(Text("build.stats.boost_radius", config.BoostRadius * GetLevelMultiplier(level)));
                    builder.AppendLine(Text("build.stats.mining_boost", config.MiningBoostPercent));
                    builder.AppendLine(Text("build.stats.fire_rate_boost", config.FireRateBoostPercent));
                    break;
            }
        }

        private void AppendBuildingSummary(StringBuilder builder, BuildingConfig config)
        {
            string name = Localize(config.DisplayNameKey, config.DisplayName);
            switch (config.Kind)
            {
                case BuildingKind.Mine: builder.AppendLine(Text("build.stats.summary_mine", name, config.ProductionAmount, config.ProductionInterval)); break;
                case BuildingKind.Cannon: builder.AppendLine(Text("build.stats.summary_cannon", name, config.Damage, config.FireRate, config.Range)); break;
                case BuildingKind.OrbitalBooster: builder.AppendLine(Text("build.stats.summary_booster", name, config.BoostRadius, config.MiningBoostPercent, config.FireRateBoostPercent)); break;
            }
        }

        private static float GetLevelMultiplier(int level) => 1f + Mathf.Max(0, level - 1) * 0.25f;
        private string GetSelectedSlotText()
        {
            if (selectedSlot == null) return Text("build.select_slot");
            Building building = selectedSlot.CurrentBuilding;
            if (building == null || building.Config == null) return Text("build.moon_slot");
            return Text("build.occupied_slot", Localize(building.Config.DisplayNameKey, building.Config.DisplayName), building.Level, building.Config.MaxLevel);
        }
        private string Text(string key, params object[] args) => localization != null ? localization.Text(key, args) : key;
        private string Localize(string key, string fallback) => localization != null ? localization.TextOrFallback(key, fallback) : fallback;
        private void RefreshFeedback() { if (feedbackText != null && feedbackTimer > 0f && !string.IsNullOrEmpty(feedbackKey)) feedbackText.text = Text(feedbackKey, feedbackArgs); }
    }
}
