using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UIDocument = UnityEngine.UIElements.UIDocument;
using VisualTreeAsset = UnityEngine.UIElements.VisualTreeAsset;
using PanelSettings = UnityEngine.UIElements.PanelSettings;
using PanelScaleMode = UnityEngine.UIElements.PanelScaleMode;

namespace OrbitalDefense.EditorTools
{
    public static class PrototypeSceneBuilder
    {
        private const string GeneratedSprites = "Assets/_Project/Art/Sprites/Generated";
        private const string GeneratedMaterials = "Assets/_Project/Art/Materials";
        private const string ScenePath = "Assets/_Project/Scenes/MainGameplay.unity";
        private const string KenneySpaceShooterRoot = "Assets/_Project/ThirdParty/Kenney/SpaceShooterRedux";
        private const string KenneySciFiRtsRoot = "Assets/_Project/Art/ThirdParty/Kenney/SciFiRTS";

        [CliCommand("orbital_defense_build_prototype", "Build the first Orbital Defense prototype scene", MainThreadRequired = true)]
        public static string BuildPrototypeScene()
        {
            EnsureFolders();

            Sprite planetSprite = CreateCircleSprite("Planet", new Color(0.12f, 0.58f, 0.72f, 1f), new Color(0.33f, 0.86f, 0.72f, 1f));
            Sprite moonSprite = CreateCircleSprite("Moon", new Color(0.42f, 0.47f, 0.56f, 1f), new Color(0.74f, 0.78f, 0.84f, 1f));
            Sprite slotSprite = CreateCircleSprite("BuildSlot", new Color(0.95f, 0.86f, 0.31f, 0.78f), new Color(1f, 1f, 1f, 0.95f));
            Sprite commandCoreSprite = CreateCircleSprite("CommandCore", new Color(0.95f, 0.68f, 0.22f, 1f), new Color(1f, 0.98f, 0.72f, 1f));
            Sprite commandCoreGlowSprite = CreateCircleSprite("CommandCoreGlow", new Color(0.30f, 0.82f, 1f, 0.42f), new Color(1f, 0.92f, 0.35f, 0.22f));
            Sprite mineSprite = LoadSpriteOrFallback($"{KenneySciFiRtsRoot}/Structure/scifiStructure_12.png", 64f, () => CreateCircleSprite("Mine", new Color(0.30f, 0.78f, 0.36f, 1f), new Color(0.84f, 1f, 0.47f, 1f)));
            Sprite cannonSprite = LoadSpriteOrFallback($"{KenneySciFiRtsRoot}/Structure/scifiStructure_16.png", 64f, () => CreateCircleSprite("Cannon", new Color(0.86f, 0.28f, 0.24f, 1f), new Color(1f, 0.72f, 0.42f, 1f)));
            Sprite projectileSprite = LoadSpriteOrFallback($"{KenneySpaceShooterRoot}/PNG/Lasers/laserBlue08.png", 32f, () => CreateCircleSprite("Projectile", new Color(1f, 0.92f, 0.30f, 1f), new Color(1f, 1f, 1f, 1f)));
            Sprite asteroidSprite = LoadSpriteOrFallback($"{KenneySpaceShooterRoot}/PNG/Meteors/meteorBrown_big3.png", 80f, () => CreateCircleSprite("SmallAsteroid", new Color(0.56f, 0.43f, 0.33f, 1f), new Color(0.88f, 0.72f, 0.50f, 1f)));
            Sprite scoutSprite = LoadSpriteOrFallback($"{KenneySpaceShooterRoot}/PNG/Enemies/enemyBlue4.png", 80f, () => CreateCircleSprite("AlienScout", new Color(0.62f, 0.24f, 0.88f, 1f), new Color(0.94f, 0.62f, 1f, 1f)));
            Sprite hitEffectSprite = CreateCircleSprite("HitEffect", new Color(1f, 0.92f, 0.28f, 0.78f), new Color(1f, 1f, 1f, 0.35f));
            Sprite deathEffectSprite = CreateCircleSprite("DeathEffect", new Color(1f, 0.36f, 0.18f, 0.78f), new Color(1f, 0.84f, 0.30f, 0.20f));
            Sprite coreDamageEffectSprite = CreateCircleSprite("CoreDamageEffect", new Color(1f, 0.18f, 0.25f, 0.82f), new Color(1f, 0.78f, 0.80f, 0.22f));
            Sprite backgroundSprite = LoadSpriteOrFallback($"{KenneySpaceShooterRoot}/Backgrounds/black.png", 100f, () => null);
            AudioClip laserClip = LoadAudioClip($"{KenneySpaceShooterRoot}/Bonus/sfx_laser1.ogg");
            AudioClip coreShotClip = LoadAudioClip($"{KenneySpaceShooterRoot}/Bonus/sfx_laser2.ogg");
            AudioClip hitClip = LoadAudioClip($"{KenneySpaceShooterRoot}/Bonus/sfx_zap.ogg");
            AudioClip enemyDestroyedClip = LoadAudioClip($"{KenneySpaceShooterRoot}/Bonus/sfx_shieldDown.ogg");
            AudioClip coreDamageClip = LoadAudioClip($"{KenneySpaceShooterRoot}/Bonus/sfx_lose.ogg");
            AudioClip shieldBlockClip = LoadAudioClip($"{KenneySpaceShooterRoot}/Bonus/sfx_shieldUp.ogg");
            AudioClip uiConfirmClip = LoadAudioClip($"{KenneySpaceShooterRoot}/Bonus/sfx_twoTone.ogg");

            Projectile projectilePrefab = CreateProjectilePrefab(projectileSprite);
            GameObject hitEffectPrefab = CreateBurstEffectPrefab("HitEffect", hitEffectSprite, 0.22f, 0.10f, 0.55f, 25);
            GameObject deathEffectPrefab = CreateBurstEffectPrefab("DeathEffect", deathEffectSprite, 0.38f, 0.22f, 0.95f, 24);
            GameObject coreDamageEffectPrefab = CreateBurstEffectPrefab("CoreDamageEffect", coreDamageEffectSprite, 0.48f, 0.45f, 1.35f, 23);
            Building minePrefab = CreateBuildingPrefab("Mine", mineSprite, typeof(MineProducer));
            Building cannonPrefab = CreateCannonPrefab(cannonSprite, projectilePrefab);

            TimeScaleConfig timeScale = CreateTimeScaleConfig();
            LocalizationTable localizationTable = CreateLocalizationTable();
            BuildingConfig mineConfig = CreateBuildingConfig("Mine", "Mine", BuildingKind.Mine, mineSprite, minePrefab, 50, 3, new[] { BuildSlotType.MoonSurface });
            SetMineValues(mineConfig, 10, 5f);
            BuildingConfig cannonConfig = CreateBuildingConfig("Cannon", "Cannon", BuildingKind.Cannon, cannonSprite, cannonPrefab, 75, 3, new[] { BuildSlotType.MoonSurface });
            SetCannonValues(cannonConfig, 10, 1f, 3.5f, 6f);
            BuildCatalog catalog = CreateBuildCatalog(mineConfig, cannonConfig);

            EnemyMover asteroidPrefab = CreateEnemyPrefab("SmallAsteroid", asteroidSprite, 0.58f);
            EnemyMover scoutPrefab = CreateEnemyPrefab("AlienScout", scoutSprite, 0.46f);
            EnemyConfig asteroidConfig = CreateEnemyConfig("SmallAsteroid", "Small Asteroid", asteroidPrefab, 20, 1f, 8, 8);
            EnemyConfig scoutConfig = CreateEnemyConfig("AlienScout", "Alien Scout", scoutPrefab, 35, 1.4f, 12, 5);
            WaveConfig[] waves = CreateWaves(asteroidConfig, scoutConfig);
            UpgradeConfig[] upgrades = CreateUpgrades();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainGameplay";
            WorldConfig worldConfig = CreateWorldConfig();

            CreateCamera();
            CreateSpaceBackground(backgroundSprite);
            GameObject bootstrap = new GameObject("Bootstrap");
            GameBootstrap gameBootstrap = bootstrap.AddComponent<GameBootstrap>();
            GameStateController gameState = bootstrap.AddComponent<GameStateController>();
            ResourceWallet wallet = bootstrap.AddComponent<ResourceWallet>();
            CoreIntegrity coreIntegrity = bootstrap.AddComponent<CoreIntegrity>();
            TimeScaleController timeScaleController = bootstrap.AddComponent<TimeScaleController>();
            LocalizationService localization = bootstrap.AddComponent<LocalizationService>();
            BuildSystem buildSystem = bootstrap.AddComponent<BuildSystem>();
            WaveSystem waveSystem = bootstrap.AddComponent<WaveSystem>();
            UpgradeSystem upgradeSystem = bootstrap.AddComponent<UpgradeSystem>();
            RunModifiers runModifiers = bootstrap.AddComponent<RunModifiers>();
            VisualEffectSpawner visualEffectSpawner = bootstrap.AddComponent<VisualEffectSpawner>();
            AudioService audioService = bootstrap.AddComponent<AudioService>();
            AudioSource audioSource = bootstrap.GetComponent<AudioSource>();

            GameObject world = new GameObject("GameWorld");
            Transform planetCenter = CreateWorld(world.transform, worldConfig, planetSprite, moonSprite, slotSprite, commandCoreSprite, commandCoreGlowSprite, projectilePrefab, coreIntegrity, wallet, waveSystem, gameState, out CommandCoreSelector coreSelector, out CommandCoreUpgrade coreUpgrade, out OrbitalSetupController orbitalSetupController);
            RangePreview rangePreview = CreateRangePreview(world.transform);
            CreateGameplayUiToolkit(catalog, buildSystem, gameState, wallet, coreIntegrity, timeScaleController, localization, waveSystem, upgradeSystem, upgrades, coreSelector, rangePreview);

            SetObject(gameBootstrap, "gameState", gameState);
            SetObject(gameBootstrap, "coreIntegrity", coreIntegrity);
            SetObject(gameBootstrap, "timeScaleController", timeScaleController);
            SetObject(visualEffectSpawner, "hitEffectPrefab", hitEffectPrefab);
            SetObject(visualEffectSpawner, "deathEffectPrefab", deathEffectPrefab);
            SetObject(visualEffectSpawner, "coreDamageEffectPrefab", coreDamageEffectPrefab);
            ConfigureAudioService(audioService, audioSource, laserClip, coreShotClip, hitClip, enemyDestroyedClip, coreDamageClip, shieldBlockClip, uiConfirmClip);
            SetObject(timeScaleController, "config", timeScale);
            SetObject(localization, "table", localizationTable);
            SetObject(buildSystem, "gameState", gameState);
            SetObject(buildSystem, "wallet", wallet);
            SetObject(waveSystem, "gameState", gameState);
            SetObject(waveSystem, "wallet", wallet);
            SetObject(waveSystem, "coreIntegrity", coreIntegrity);
            SetObject(waveSystem, "planetCenter", planetCenter);
            SetObject(waveSystem, "orbitalSetupController", orbitalSetupController);
            SetObjectArray(waveSystem, "waves", waves);
            SetObject(upgradeSystem, "gameState", gameState);
            SetObject(upgradeSystem, "coreIntegrity", coreIntegrity);
            SetObject(upgradeSystem, "modifiers", runModifiers);
            SetObject(upgradeSystem, "commandCoreUpgrade", coreUpgrade);
            SetObjectArray(upgradeSystem, "availableUpgrades", upgrades);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return $"Created prototype scene at {ScenePath} with {waves.Length} waves and {GetConfiguredMoonCount(worldConfig)} configured moons.";
        }

        private static void EnsureFolders()
        {
            string[] folders =
            {
                GeneratedSprites,
                GeneratedMaterials,
                "Assets/_Project/Prefabs/Buildings",
                "Assets/_Project/Prefabs/Enemies",
                "Assets/_Project/Prefabs/Projectiles",
                "Assets/_Project/Prefabs/Visuals",
                "Assets/_Project/Scenes",
                "Assets/_Project/ScriptableObjects/Buildings",
                "Assets/_Project/ScriptableObjects/Enemies",
                "Assets/_Project/ScriptableObjects/Levels",
                "Assets/_Project/ScriptableObjects/Localization",
                "Assets/_Project/ScriptableObjects/World",
                "Assets/_Project/ScriptableObjects/Upgrades",
                "Assets/_Project/ScriptableObjects/Waves"
            };

            for (int i = 0; i < folders.Length; i++)
            {
                Directory.CreateDirectory(ToAbsolute(folders[i]));
            }
        }

        private static int GetConfiguredMoonCount(WorldConfig config)
        {
            if (config == null)
            {
                return 0;
            }

            SerializedObject serialized = new SerializedObject(config);
            SerializedProperty moons = serialized.FindProperty("moons");
            return moons != null ? moons.arraySize : 0;
        }

        private static Transform CreateWorld(Transform parent, WorldConfig worldConfig, Sprite planetSprite, Sprite moonSprite, Sprite slotSprite, Sprite commandCoreSprite, Sprite commandCoreGlowSprite, Projectile projectilePrefab, CoreIntegrity coreIntegrity, ResourceWallet wallet, WaveSystem waveSystem, GameStateController gameState, out CommandCoreSelector coreSelector, out CommandCoreUpgrade coreUpgrade, out OrbitalSetupController orbitalSetupController)
        {
            GameObject planet = CreateSpriteObject("Planet", parent, planetSprite, new Vector3(2.25f, 2.25f, 1f), 0);
            planet.transform.position = Vector3.zero;
            SetFloat(planet.AddComponent<SelfRotator>(), "degreesPerSecond", 8f);

            GameObject planetCenter = new GameObject("PlanetCenter");
            planetCenter.transform.SetParent(planet.transform, false);
            Transform commandCore = CreateCommandCore(planet.transform, commandCoreSprite, commandCoreGlowSprite, projectilePrefab, coreIntegrity, wallet, waveSystem, out coreSelector, out coreUpgrade);

            GameObject orbitalSetupGo = new GameObject("OrbitalSetupController", typeof(OrbitalSetupController));
            orbitalSetupGo.transform.SetParent(parent, false);
            orbitalSetupController = orbitalSetupGo.GetComponent<OrbitalSetupController>();
            SetObject(orbitalSetupController, "gameState", gameState);

            GameObject moonsRoot = new GameObject("Moons", typeof(MoonSpawner));
            moonsRoot.transform.SetParent(parent, false);
            MoonSpawner spawner = moonsRoot.GetComponent<MoonSpawner>();
            SetObject(spawner, "worldConfig", worldConfig);
            SetObject(spawner, "orbitCenter", planet.transform);
            SetObject(spawner, "moonSprite", moonSprite);
            SetObject(spawner, "slotSprite", slotSprite);
            SetObject(spawner, "orbitalSetupController", orbitalSetupController);
            spawner.Rebuild();

            return commandCore != null ? commandCore : planetCenter.transform;
        }

        private static Transform CreateCommandCore(Transform parent, Sprite coreSprite, Sprite glowSprite, Projectile projectilePrefab, CoreIntegrity coreIntegrity, ResourceWallet wallet, WaveSystem waveSystem, out CommandCoreSelector selector, out CommandCoreUpgrade upgrade)
        {
            GameObject root = new GameObject("CommandCore");
            root.transform.SetParent(parent, false);

            GameObject glow = CreateSpriteObject("CommandCoreGlow", root.transform, glowSprite, new Vector3(0.72f, 0.72f, 1f), 2);
            GameObject body = CreateSpriteObject("CommandCoreBody", root.transform, coreSprite, new Vector3(0.36f, 0.36f, 1f), 7);
            SetFloat(body.AddComponent<SelfRotator>(), "degreesPerSecond", -42f);

            CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
            collider.radius = 0.42f;

            upgrade = root.AddComponent<CommandCoreUpgrade>();
            SetObject(upgrade, "coreIntegrity", coreIntegrity);
            SetObject(upgrade, "wallet", wallet);
            SetObject(upgrade, "waveSystem", waveSystem);

            CommandCoreWeapon weapon = root.AddComponent<CommandCoreWeapon>();
            SetObject(weapon, "coreUpgrade", upgrade);
            SetObject(weapon, "projectilePrefab", projectilePrefab);
            SetObject(weapon, "muzzle", body.transform);

            CommandCoreVisual visual = root.AddComponent<CommandCoreVisual>();
            SetObject(visual, "coreIntegrity", coreIntegrity);
            SetObject(visual, "coreBody", body.transform);
            SetObject(visual, "glowRenderer", glow.GetComponent<SpriteRenderer>());
            selector = root.AddComponent<CommandCoreSelector>();
            return root.transform;
        }

        private static void CreateGameplayUiToolkit(BuildCatalog catalog, BuildSystem buildSystem, GameStateController gameState, ResourceWallet wallet, CoreIntegrity coreIntegrity, TimeScaleController timeScaleController, LocalizationService localization, WaveSystem waveSystem, UpgradeSystem upgradeSystem, UpgradeConfig[] upgrades, CommandCoreSelector coreSelector, RangePreview rangePreview)
        {
            const string visualTreePath = "Assets/_Project/UI/GameplayUI.uxml";
            const string panelSettingsPath = "Assets/_Project/UI/GameplayPanelSettings.asset";
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(visualTreePath);
            if (visualTree == null)
            {
                Debug.LogError($"Missing UI Toolkit document at {visualTreePath}.");
                return;
            }

            PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(panelSettingsPath);
            if (panelSettings == null)
            {
                panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(panelSettings, panelSettingsPath);
            }
            panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panelSettings.referenceResolution = new Vector2Int(1080, 1920);
            EditorUtility.SetDirty(panelSettings);

            GameObject uiRoot = new GameObject("GameplayUI", typeof(UIDocument));
            UIDocument document = uiRoot.GetComponent<UIDocument>();
            document.panelSettings = panelSettings;
            document.visualTreeAsset = visualTree;

            GameplayHudPresenter hud = uiRoot.AddComponent<GameplayHudPresenter>();
            BuildPanelPresenter buildPresenter = uiRoot.AddComponent<BuildPanelPresenter>();
            CommandCorePanelPresenter corePresenter = uiRoot.AddComponent<CommandCorePanelPresenter>();
            UpgradePanelPresenter upgradePresenter = uiRoot.AddComponent<UpgradePanelPresenter>();
            GameOverPanelPresenter gameOverPresenter = uiRoot.AddComponent<GameOverPanelPresenter>();
            WaveDirectionWarningPresenter warningPresenter = uiRoot.AddComponent<WaveDirectionWarningPresenter>();

            SetObject(hud, "wallet", wallet);
            SetObject(hud, "coreIntegrity", coreIntegrity);
            SetObject(hud, "gameState", gameState);
            SetObject(hud, "waveSystem", waveSystem);
            SetObject(hud, "timeScaleController", timeScaleController);
            SetObject(hud, "localization", localization);
            SetObject(buildPresenter, "buildSystem", buildSystem);
            SetObject(buildPresenter, "catalog", catalog);
            SetObject(buildPresenter, "localization", localization);
            SetObject(buildPresenter, "rangePreview", rangePreview);
            SetObject(corePresenter, "coreIntegrity", coreIntegrity);
            SetObject(corePresenter, "wallet", wallet);
            SetObject(corePresenter, "gameState", gameState);
            SetObject(corePresenter, "localization", localization);
            SetObject(upgradePresenter, "gameState", gameState);
            SetObject(upgradePresenter, "upgradeSystem", upgradeSystem);
            SetObject(upgradePresenter, "localization", localization);
            SetObject(gameOverPresenter, "gameState", gameState);
            SetObject(gameOverPresenter, "localization", localization);
            SetObject(warningPresenter, "gameState", gameState);
            SetObject(warningPresenter, "waveSystem", waveSystem);

            SetObjectArray(upgradeSystem, "availableUpgrades", upgrades);
            if (coreSelector != null)
            {
                SetObject(coreSelector, "corePanel", corePresenter);
                SetObject(coreSelector, "buildPanel", buildPresenter);
            }
        }

        private static void CreateGameplayUi(BuildCatalog catalog, BuildSystem buildSystem, GameStateController gameState, ResourceWallet wallet, CoreIntegrity coreIntegrity, TimeScaleController timeScaleController, LocalizationService localization, WaveSystem waveSystem, UpgradeSystem upgradeSystem, UpgradeConfig[] upgrades, CommandCoreSelector coreSelector, RangePreview rangePreview)
        {
            GameObject canvasGo = new GameObject("GameplayCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 5f;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            GameObject hudGo = CreatePanel("GameplayHud", canvasGo.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -150f), new Vector2(-24f, -24f), new Color(0.04f, 0.06f, 0.11f, 0.72f));
            GameplayHudPresenter hud = hudGo.AddComponent<GameplayHudPresenter>();
            TMP_Text minerals = CreateText("MineralsText", hudGo.transform, "Minerals: 100", 34, TextAlignmentOptions.Left, new Vector2(0.02f, 0.55f), new Vector2(0.35f, 0.95f));
            TMP_Text core = CreateText("CoreText", hudGo.transform, "Command Core: 100/100", 34, TextAlignmentOptions.Left, new Vector2(0.36f, 0.55f), new Vector2(0.68f, 0.95f));
            TMP_Text wave = CreateText("WaveText", hudGo.transform, "Wave: 0/10", 34, TextAlignmentOptions.Left, new Vector2(0.69f, 0.55f), new Vector2(0.98f, 0.95f));
            TMP_Text phase = CreateText("PhaseText", hudGo.transform, "BuildPhase", 30, TextAlignmentOptions.Left, new Vector2(0.02f, 0.05f), new Vector2(0.35f, 0.45f));
            TMP_Text speed = CreateText("SpeedText", hudGo.transform, "x1", 30, TextAlignmentOptions.Center, new Vector2(0.78f, 0.05f), new Vector2(0.98f, 0.45f));
            Button startWave = CreateButton("StartWaveButton", hudGo.transform, "Start Wave", new Vector2(0.36f, 0.04f), new Vector2(0.62f, 0.46f), new Color(0.12f, 0.72f, 0.52f, 0.95f));
            Button speedButton = CreateButton("SpeedButton", hudGo.transform, "Speed", new Vector2(0.63f, 0.04f), new Vector2(0.77f, 0.46f), new Color(0.24f, 0.36f, 0.82f, 0.95f));

            SetObject(hud, "wallet", wallet);
            SetObject(hud, "coreIntegrity", coreIntegrity);
            SetObject(hud, "gameState", gameState);
            SetObject(hud, "waveSystem", waveSystem);
            SetObject(hud, "timeScaleController", timeScaleController);
            SetObject(hud, "localization", localization);
            SetObject(hud, "mineralsText", minerals);
            SetObject(hud, "coreText", core);
            SetObject(hud, "waveText", wave);
            SetObject(hud, "phaseText", phase);
            SetObject(hud, "speedText", speed);
            SetObject(hud, "startWaveButton", startWave);
            SetObject(hud, "speedButton", speedButton);
            UnityEventTools.AddPersistentListener(startWave.onClick, hud.OnStartWaveClicked);
            UnityEventTools.AddPersistentListener(speedButton.onClick, hud.OnSpeedClicked);

            GameObject buildPanel = CreatePanel("BuildPanel", canvasGo.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(24f, 24f), new Vector2(-24f, 190f), new Color(0.05f, 0.07f, 0.13f, 0.86f));
            BuildPanelPresenter buildPresenter = buildPanel.AddComponent<BuildPanelPresenter>();
            TMP_Text slotText = CreateText("SelectedSlotText", buildPanel.transform, "Select slot", 32, TextAlignmentOptions.Left, new Vector2(0.03f, 0.66f), new Vector2(0.36f, 0.92f));
            TMP_Text statsText = CreateText("SelectedStatsText", buildPanel.transform, "Available stats", 24, TextAlignmentOptions.Left, new Vector2(0.03f, 0.24f), new Vector2(0.36f, 0.62f));
            TMP_Text feedbackText = CreateText("BuildFeedbackText", buildPanel.transform, string.Empty, 24, TextAlignmentOptions.Left, new Vector2(0.03f, 0.08f), new Vector2(0.36f, 0.22f));
            feedbackText.color = new Color(1f, 0.55f, 0.42f, 1f);
            Button mineButton = CreateButton("MineButton", buildPanel.transform, "Mine", new Vector2(0.38f, 0.18f), new Vector2(0.56f, 0.82f), new Color(0.22f, 0.66f, 0.34f, 0.95f));
            Button cannonButton = CreateButton("CannonButton", buildPanel.transform, "Cannon", new Vector2(0.58f, 0.18f), new Vector2(0.76f, 0.82f), new Color(0.82f, 0.26f, 0.24f, 0.95f));
            Button upgradeButton = CreateButton("UpgradeButton", buildPanel.transform, "Upgrade", new Vector2(0.38f, 0.18f), new Vector2(0.66f, 0.82f), new Color(0.90f, 0.68f, 0.20f, 0.95f));
            Button sellButton = CreateButton("SellButton", buildPanel.transform, "Sell", new Vector2(0.68f, 0.18f), new Vector2(0.96f, 0.82f), new Color(0.72f, 0.20f, 0.24f, 0.95f));
            SetObject(buildPresenter, "buildSystem", buildSystem);
            SetObject(buildPresenter, "catalog", catalog);
            SetObject(buildPresenter, "localization", localization);
            SetObject(buildPresenter, "panelRoot", buildPanel);
            SetObject(buildPresenter, "selectedSlotText", slotText);
            SetObject(buildPresenter, "statsText", statsText);
            SetObject(buildPresenter, "mineButton", mineButton);
            SetObject(buildPresenter, "cannonButton", cannonButton);
            SetObject(buildPresenter, "upgradeButton", upgradeButton);
            SetObject(buildPresenter, "sellButton", sellButton);
            SetObject(buildPresenter, "feedbackText", feedbackText);
            SetObject(buildPresenter, "rangePreview", rangePreview);
            UnityEventTools.AddPersistentListener(mineButton.onClick, buildPresenter.BuildMine);
            UnityEventTools.AddPersistentListener(cannonButton.onClick, buildPresenter.BuildCannon);
            UnityEventTools.AddPersistentListener(upgradeButton.onClick, buildPresenter.UpgradeSelected);
            UnityEventTools.AddPersistentListener(sellButton.onClick, buildPresenter.SellSelected);

            CommandCorePanelPresenter corePresenter = CreateCommandCorePanel(canvasGo.transform, coreIntegrity, wallet, gameState, localization);
            if (coreSelector != null)
            {
                SetObject(coreSelector, "corePanel", corePresenter);
                SetObject(coreSelector, "buildPanel", buildPresenter);
            }

            GameObject upgradeHost = new GameObject("UpgradePanelPresenter", typeof(RectTransform));
            upgradeHost.transform.SetParent(canvasGo.transform, false);
            StretchToParent(upgradeHost.GetComponent<RectTransform>());
            UpgradePanelPresenter upgradePresenter = upgradeHost.AddComponent<UpgradePanelPresenter>();
            GameObject upgradePanel = CreatePanel("UpgradePanel", upgradeHost.transform, new Vector2(0.08f, 0.30f), new Vector2(0.92f, 0.70f), Vector2.zero, Vector2.zero, new Color(0.05f, 0.07f, 0.13f, 0.94f));
            TMP_Text upgradeTitle = CreateText("UpgradeTitle", upgradePanel.transform, "Choose upgrade", 42, TextAlignmentOptions.Center, new Vector2(0.05f, 0.76f), new Vector2(0.95f, 0.94f));
            Button firstUpgrade = CreateButton("FirstUpgradeButton", upgradePanel.transform, "Upgrade 1", new Vector2(0.06f, 0.14f), new Vector2(0.32f, 0.70f), new Color(0.23f, 0.40f, 0.88f, 0.95f));
            Button secondUpgrade = CreateButton("SecondUpgradeButton", upgradePanel.transform, "Upgrade 2", new Vector2(0.37f, 0.14f), new Vector2(0.63f, 0.70f), new Color(0.22f, 0.66f, 0.34f, 0.95f));
            Button thirdUpgrade = CreateButton("ThirdUpgradeButton", upgradePanel.transform, "Upgrade 3", new Vector2(0.68f, 0.14f), new Vector2(0.94f, 0.70f), new Color(0.90f, 0.68f, 0.20f, 0.95f));
            ConfigureButtonLabel(firstUpgrade, 24f, 14f);
            ConfigureButtonLabel(secondUpgrade, 24f, 14f);
            ConfigureButtonLabel(thirdUpgrade, 24f, 14f);
            SetObject(upgradePresenter, "gameState", gameState);
            SetObject(upgradePresenter, "upgradeSystem", upgradeSystem);
            SetObject(upgradePresenter, "localization", localization);
            SetObject(upgradePresenter, "panelRoot", upgradePanel);
            SetObject(upgradePresenter, "titleText", upgradeTitle);
            SetObjectArray(upgradePresenter, "choiceButtons", new Object[] { firstUpgrade, secondUpgrade, thirdUpgrade });
            SetObjectArray(upgradePresenter, "choiceLabels", new Object[]
            {
                firstUpgrade.GetComponentInChildren<TMP_Text>(),
                secondUpgrade.GetComponentInChildren<TMP_Text>(),
                thirdUpgrade.GetComponentInChildren<TMP_Text>()
            });
            SetObjectArray(upgradeSystem, "availableUpgrades", upgrades);
            UnityEventTools.AddPersistentListener(firstUpgrade.onClick, upgradePresenter.ChooseFirst);
            UnityEventTools.AddPersistentListener(secondUpgrade.onClick, upgradePresenter.ChooseSecond);
            UnityEventTools.AddPersistentListener(thirdUpgrade.onClick, upgradePresenter.ChooseThird);

            GameObject gameOverHost = new GameObject("GameOverPanelPresenter", typeof(RectTransform));
            gameOverHost.transform.SetParent(canvasGo.transform, false);
            StretchToParent(gameOverHost.GetComponent<RectTransform>());
            GameOverPanelPresenter gameOverPresenter = gameOverHost.AddComponent<GameOverPanelPresenter>();
            GameObject gameOverPanel = CreatePanel("GameOverPanel", gameOverHost.transform, new Vector2(0.16f, 0.36f), new Vector2(0.84f, 0.66f), Vector2.zero, Vector2.zero, new Color(0.04f, 0.05f, 0.10f, 0.95f));
            TMP_Text gameOverTitle = CreateText("GameOverTitle", gameOverPanel.transform, "Victory", 52, TextAlignmentOptions.Center, new Vector2(0.08f, 0.58f), new Vector2(0.92f, 0.88f));
            TMP_Text gameOverBody = CreateText("GameOverBody", gameOverPanel.transform, "The Command Core survived all waves.", 30, TextAlignmentOptions.Center, new Vector2(0.08f, 0.38f), new Vector2(0.92f, 0.58f));
            Button restartButton = CreateButton("RestartButton", gameOverPanel.transform, "Restart", new Vector2(0.32f, 0.12f), new Vector2(0.68f, 0.34f), new Color(0.12f, 0.72f, 0.52f, 0.95f));
            SetObject(gameOverPresenter, "gameState", gameState);
            SetObject(gameOverPresenter, "localization", localization);
            SetObject(gameOverPresenter, "panelRoot", gameOverPanel);
            SetObject(gameOverPresenter, "titleText", gameOverTitle);
            SetObject(gameOverPresenter, "bodyText", gameOverBody);
            SetObject(gameOverPresenter, "restartButton", restartButton);
            UnityEventTools.AddPersistentListener(restartButton.onClick, gameOverPresenter.Restart);

            CreateWaveDirectionWarning(canvasGo.transform, gameState, waveSystem);
        }

        private static CommandCorePanelPresenter CreateCommandCorePanel(Transform parent, CoreIntegrity coreIntegrity, ResourceWallet wallet, GameStateController gameState, LocalizationService localization)
        {
            GameObject panel = CreatePanel("CommandCorePanel", parent, new Vector2(0.05f, 0.18f), new Vector2(0.46f, 0.39f), Vector2.zero, Vector2.zero, new Color(0.05f, 0.07f, 0.13f, 0.90f));
            CommandCorePanelPresenter presenter = panel.AddComponent<CommandCorePanelPresenter>();
            TMP_Text title = CreateText("CommandCoreTitle", panel.transform, "Command Core", 30, TextAlignmentOptions.Left, new Vector2(0.06f, 0.66f), new Vector2(0.70f, 0.92f));
            TMP_Text status = CreateText("CommandCoreStatus", panel.transform, "Integrity: 100/100\nMining: 0/3\nDefense: 0/3\nShield: 0/3 (0)", 22, TextAlignmentOptions.Left, new Vector2(0.06f, 0.18f), new Vector2(0.64f, 0.64f));
            TMP_Text feedback = CreateText("CommandCoreFeedback", panel.transform, string.Empty, 20, TextAlignmentOptions.Left, new Vector2(0.06f, 0.06f), new Vector2(0.64f, 0.16f));
            feedback.color = new Color(1f, 0.55f, 0.42f, 1f);
            Button close = CreateButton("CommandCoreCloseButton", panel.transform, "Close", new Vector2(0.72f, 0.66f), new Vector2(0.94f, 0.90f), new Color(0.24f, 0.30f, 0.42f, 0.95f));

            ConfigureButtonLabel(close, 18f, 11f);
            SetObject(presenter, "panelRoot", panel);
            SetObject(presenter, "coreIntegrity", coreIntegrity);
            SetObject(presenter, "wallet", wallet);
            SetObject(presenter, "gameState", gameState);
            SetObject(presenter, "localization", localization);
            SetObject(presenter, "titleText", title);
            SetObject(presenter, "statusText", status);
            SetObject(presenter, "feedbackText", feedback);
            SetObject(presenter, "closeButton", close);
            UnityEventTools.AddPersistentListener(close.onClick, presenter.Hide);
            return presenter;
        }

        private static WaveDirectionWarningPresenter CreateWaveDirectionWarning(Transform parent, GameStateController gameState, WaveSystem waveSystem)
        {
            GameObject warningGo = new GameObject("WaveDirectionWarning", typeof(RectTransform), typeof(CanvasGroup));
            warningGo.transform.SetParent(parent, false);

            RectTransform rect = warningGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(180f, 180f);

            CanvasGroup canvasGroup = warningGo.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            WaveDirectionWarningPresenter presenter = warningGo.AddComponent<WaveDirectionWarningPresenter>();
            TMP_Text warningText = CreateText("WarningText", warningGo.transform, "!", 92f, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
            warningText.enableAutoSizing = true;
            warningText.fontSizeMin = 54f;
            warningText.fontSizeMax = 92f;
            warningText.fontStyle = FontStyles.Bold;
            warningText.color = new Color(1f, 0.94f, 0.36f, 1f);
            warningText.raycastTarget = false;

            SetObject(presenter, "gameState", gameState);
            SetObject(presenter, "waveSystem", waveSystem);
            SetObject(presenter, "warningRect", rect);
            SetObject(presenter, "canvasGroup", canvasGroup);
            SetObject(presenter, "warningText", warningText);
            return presenter;
        }

        private static void CreateCamera()
        {
            GameObject cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(Physics2DRaycaster));
            cameraGo.tag = "MainCamera";
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);
            Camera camera = cameraGo.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.2f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.015f, 0.018f, 0.035f, 1f);
        }

        private static void CreateSpaceBackground(Sprite sprite)
        {
            if (sprite == null)
            {
                return;
            }

            GameObject background = CreateSpriteObject("Starfield", null, sprite, new Vector3(5.2f, 5.2f, 1f), -100);
            background.transform.position = new Vector3(0f, 0f, 2f);
        }

        private static GameObject CreateSpriteObject(string name, Transform parent, Sprite sprite, Vector3 scale, int sortingOrder)
        {
            GameObject go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            go.transform.localScale = scale;
            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            return go;
        }

        private static RangePreview CreateRangePreview(Transform parent)
        {
            GameObject go = new GameObject("CannonRangePreview", typeof(LineRenderer), typeof(RangePreview));
            go.transform.SetParent(parent, false);
            LineRenderer line = go.GetComponent<LineRenderer>();
            line.loop = true;
            line.useWorldSpace = true;
            line.positionCount = 128;
            line.startWidth = 0.025f;
            line.endWidth = 0.025f;
            line.startColor = new Color(0.30f, 0.82f, 1f, 0.72f);
            line.endColor = new Color(0.30f, 0.82f, 1f, 0.72f);
            line.sortingOrder = 30;
            line.material = GetRangePreviewMaterial();
            line.enabled = false;
            RangePreview preview = go.GetComponent<RangePreview>();
            SetObject(preview, "line", line);
            SetInt(preview, "segments", 128);
            SetFloat(preview, "lineWidth", 0.025f);
            SetColor(preview, "previewColor", new Color(0.30f, 0.82f, 1f, 0.72f));
            return preview;
        }

        private static Material GetRangePreviewMaterial()
        {
            string path = $"{GeneratedMaterials}/RangePreviewLine.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = new Color(0.30f, 0.82f, 1f, 0.72f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Projectile CreateProjectilePrefab(Sprite sprite)
        {
            string path = "Assets/_Project/Prefabs/Projectiles/Projectile.prefab";
            GameObject go = CreateSpriteObject("Projectile", null, sprite, new Vector3(0.12f, 0.12f, 1f), 20);
            Projectile projectile = go.AddComponent<Projectile>();
            SavePrefab(go, path);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<Projectile>();
        }

        private static GameObject CreateBurstEffectPrefab(string name, Sprite sprite, float lifetime, float startScale, float endScale, int sortingOrder)
        {
            string path = $"Assets/_Project/Prefabs/Visuals/{name}.prefab";
            GameObject go = CreateSpriteObject(name, null, sprite, Vector3.one * startScale, sortingOrder);
            SpriteBurstEffect effect = go.AddComponent<SpriteBurstEffect>();
            SetFloat(effect, "lifetime", lifetime);
            SetFloat(effect, "startScale", startScale);
            SetFloat(effect, "endScale", endScale);
            SavePrefab(go, path);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static Building CreateBuildingPrefab(string name, Sprite sprite, System.Type behaviorType)
        {
            string path = $"Assets/_Project/Prefabs/Buildings/{name}.prefab";
            // Building slots are scaled down to sit on the moon surface, so compensate here
            // to keep the placed building readable after inheriting the slot transform.
            GameObject go = CreateSpriteObject(name, null, sprite, new Vector3(2.4f, 2.4f, 1f), 10);
            Building building = go.AddComponent<Building>();
            Component behavior = go.AddComponent(behaviorType);
            SetObject(behavior, "building", building);
            SavePrefab(go, path);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<Building>();
        }

        private static Building CreateCannonPrefab(Sprite sprite, Projectile projectilePrefab)
        {
            string path = "Assets/_Project/Prefabs/Buildings/Cannon.prefab";
            GameObject go = CreateSpriteObject("Cannon", null, sprite, new Vector3(0.46f, 0.46f, 1f), 10);
            Building building = go.AddComponent<Building>();
            TurretWeapon turret = go.AddComponent<TurretWeapon>();
            GameObject muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(go.transform, false);
            muzzle.transform.localPosition = new Vector3(0.28f, 0f, 0f);
            SetObject(turret, "building", building);
            SetObject(turret, "projectilePrefab", projectilePrefab);
            SetObject(turret, "muzzle", muzzle.transform);
            SavePrefab(go, path);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<Building>();
        }

        private static EnemyMover CreateEnemyPrefab(string name, Sprite sprite, float scale)
        {
            string path = $"Assets/_Project/Prefabs/Enemies/{name}.prefab";
            GameObject go = CreateSpriteObject(name, null, sprite, new Vector3(scale, scale, 1f), 15);
            go.AddComponent<CircleCollider2D>().radius = 0.5f;
            go.AddComponent<Health>();
            go.AddComponent<DamageFlash>();
            EnemyMover mover = go.AddComponent<EnemyMover>();
            go.AddComponent<CoreDamageOnReach>();
            go.AddComponent<EnemyReward>();
            SavePrefab(go, path);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<EnemyMover>();
        }

        private static void ConfigureAudioService(AudioService audioService, AudioSource source, AudioClip laserClip, AudioClip coreShotClip, AudioClip hitClip, AudioClip enemyDestroyedClip, AudioClip coreDamageClip, AudioClip shieldBlockClip, AudioClip uiConfirmClip)
        {
            if (audioService == null)
            {
                return;
            }

            if (source != null)
            {
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                source.volume = 1f;
            }

            SetObject(audioService, "source", source);
            SetObject(audioService, "cannonShotClip", laserClip);
            SetObject(audioService, "coreShotClip", coreShotClip);
            SetObject(audioService, "hitClip", hitClip);
            SetObject(audioService, "enemyDestroyedClip", enemyDestroyedClip);
            SetObject(audioService, "coreDamageClip", coreDamageClip);
            SetObject(audioService, "shieldBlockClip", shieldBlockClip);
            SetObject(audioService, "resourceCollectedClip", uiConfirmClip);
            SetObject(audioService, "buildClip", uiConfirmClip);
            SetObject(audioService, "upgradeClip", uiConfirmClip);
            SetObject(audioService, "sellClip", shieldBlockClip);
        }

        private static void SavePrefab(GameObject go, string path)
        {
            AssetDatabase.DeleteAsset(path);
            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
        }

        private static Sprite CreateCircleSprite(string name, Color fill, Color rim)
        {
            const int size = 128;
            string assetPath = $"{GeneratedSprites}/{name}.png";
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.47f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    if (distance > radius)
                    {
                        texture.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    float rimMix = Mathf.InverseLerp(radius * 0.55f, radius, distance);
                    Color color = Color.Lerp(fill, rim, rimMix);
                    color.a *= Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(radius * 0.94f, radius, distance));
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            File.WriteAllBytes(ToAbsolute(assetPath), texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 128f;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static Sprite LoadSpriteOrFallback(string assetPath, float pixelsPerUnit, System.Func<Sprite> fallback)
        {
            if (!File.Exists(ToAbsolute(assetPath)))
            {
                return fallback();
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
            }

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            return sprite != null ? sprite : fallback();
        }

        private static AudioClip LoadAudioClip(string assetPath)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
        }

        private static TimeScaleConfig CreateTimeScaleConfig()
        {
            TimeScaleConfig config = CreateAsset<TimeScaleConfig>("Assets/_Project/ScriptableObjects/Levels/TimeScale_Default.asset");
            SetFloat(config, "normalSpeed", 1f);
            SetFloat(config, "fastSpeed", 2f);
            return config;
        }

        private static WorldConfig CreateWorldConfig()
        {
            WorldConfig config = CreateAsset<WorldConfig>("Assets/_Project/ScriptableObjects/World/World_Default.asset");
            SerializedObject serialized = new SerializedObject(config);
            SerializedProperty moons = serialized.FindProperty("moons");
            if (moons.arraySize > 0)
            {
                return config;
            }

            moons.arraySize = 2;

            ConfigureMoon(
                moons.GetArrayElementAtIndex(0),
                "Moon",
                "MoonOrbitRing",
                new Vector3(0.70f, 0.70f, 1f),
                2.65f,
                18f,
                35f,
                -22f,
                3,
                0.48f,
                45f,
                new Color(0.80f, 0.86f, 1f, 0.18f));

            ConfigureMoon(
                moons.GetArrayElementAtIndex(1),
                "SmallMoon",
                "SmallMoonOrbitRing",
                new Vector3(0.48f, 0.48f, 1f),
                3.55f,
                -13f,
                150f,
                28f,
                2,
                0.34f,
                90f,
                new Color(0.35f, 0.73f, 1f, 0.24f));

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            return config;
        }

        private static void ConfigureMoon(SerializedProperty moon, string objectName, string orbitRingName, Vector3 scale, float orbitRadius, float orbitDegreesPerSecond, float startAngleDegrees, float selfRotationDegreesPerSecond, int slotCount, float slotRadius, float slotAngleOffsetDegrees, Color orbitRingColor)
        {
            moon.FindPropertyRelative("objectName").stringValue = objectName;
            moon.FindPropertyRelative("orbitRingName").stringValue = orbitRingName;
            moon.FindPropertyRelative("scale").vector3Value = scale;
            moon.FindPropertyRelative("orbitRadius").floatValue = orbitRadius;
            moon.FindPropertyRelative("orbitDegreesPerSecond").floatValue = orbitDegreesPerSecond;
            moon.FindPropertyRelative("startAngleDegrees").floatValue = startAngleDegrees;
            moon.FindPropertyRelative("selfRotationDegreesPerSecond").floatValue = selfRotationDegreesPerSecond;
            moon.FindPropertyRelative("slotCount").intValue = slotCount;
            moon.FindPropertyRelative("slotRadius").floatValue = slotRadius;
            moon.FindPropertyRelative("slotAngleOffsetDegrees").floatValue = slotAngleOffsetDegrees;
            moon.FindPropertyRelative("orbitRingColor").colorValue = orbitRingColor;
        }

        private static BuildingConfig CreateBuildingConfig(string id, string displayName, BuildingKind kind, Sprite icon, Building prefab, int cost, int maxLevel, BuildSlotType[] allowedSlots)
        {
            BuildingConfig config = CreateAsset<BuildingConfig>($"Assets/_Project/ScriptableObjects/Buildings/Building_{id}.asset");
            SetString(config, "id", id);
            SetString(config, "displayName", displayName);
            SetString(config, "displayNameKey", $"building.{id}.name");
            SetEnum(config, "kind", (int)kind);
            SetObject(config, "icon", icon);
            SetObject(config, "prefab", prefab);
            SetInt(config, "buildCost", cost);
            SetInt(config, "maxLevel", maxLevel);
            SetEnumArray(config, "allowedSlots", allowedSlots);
            return config;
        }

        private static BuildCatalog CreateBuildCatalog(params BuildingConfig[] buildings)
        {
            BuildCatalog catalog = CreateAsset<BuildCatalog>("Assets/_Project/ScriptableObjects/Buildings/BuildCatalog.asset");
            SetObjectArray(catalog, "buildings", buildings);
            return catalog;
        }

        private static EnemyConfig CreateEnemyConfig(string id, string displayName, EnemyMover prefab, int health, float speed, int coreDamage, int reward)
        {
            EnemyConfig config = CreateAsset<EnemyConfig>($"Assets/_Project/ScriptableObjects/Enemies/Enemy_{id}.asset");
            SetString(config, "id", id);
            SetString(config, "displayName", displayName);
            SetObject(config, "prefab", prefab);
            SetInt(config, "health", health);
            SetFloat(config, "speed", speed);
            SetInt(config, "coreDamage", coreDamage);
            SetInt(config, "reward", reward);
            return config;
        }

        private static WaveConfig[] CreateWaves(EnemyConfig asteroid, EnemyConfig scout)
        {
            WaveConfig[] waves = new WaveConfig[10];
            int[] asteroidCounts = { 5, 8, 6, 8, 13, 8, 11, 14, 18, 22 };
            int[] scoutCounts = { 0, 0, 3, 4, 0, 7, 6, 8, 8, 12 };

            for (int i = 0; i < waves.Length; i++)
            {
                int waveNumber = i + 1;
                WaveConfig wave = CreateAsset<WaveConfig>($"Assets/_Project/ScriptableObjects/Waves/Wave_{waveNumber:00}.asset");
                SetInt(wave, "waveNumber", waveNumber);
                SetInt(wave, "rewardOnComplete", 20 + waveNumber * 5);
                SetFloat(wave, "spawnRadius", 7f);
                SetWaveGroups(wave, asteroid, asteroidCounts[i], scout, scoutCounts[i], waveNumber);
                waves[i] = wave;
            }

            return waves;
        }

        private static UpgradeConfig[] CreateUpgrades()
        {
            UpgradeConfig mine = CreateUpgrade("MineYield", "Mine Yield", "Mines produce more minerals.", UpgradeEffectType.MineProductionPercent, 15f, 100);
            UpgradeConfig cannon = CreateUpgrade("CannonDamage", "Cannon Damage", "Cannons deal more damage.", UpgradeEffectType.CannonDamagePercent, 10f, 100);
            UpgradeConfig repair = CreateUpgrade("CoreRepair", "Core Repair", "Restore Command Core integrity.", UpgradeEffectType.CoreRepairPercent, 15f, 100);
            UpgradeConfig mining = CreateUpgrade("CommandCoreMining", "Mining Core", "Command Core gains minerals after each wave.", UpgradeEffectType.CommandCoreMining, 1f, 35);
            UpgradeConfig defense = CreateUpgrade("CommandCoreDefense", "Defense Core", "Command Core shoots nearby enemies.", UpgradeEffectType.CommandCoreDefense, 1f, 35);
            UpgradeConfig shield = CreateUpgrade("CommandCoreShield", "Shield Core", "Command Core blocks enemy impacts each wave.", UpgradeEffectType.CommandCoreShield, 1f, 35);
            return new[] { mine, cannon, repair, mining, defense, shield };
        }

        private static UpgradeConfig CreateUpgrade(string id, string displayName, string description, UpgradeEffectType effectType, float value, int selectionWeight)
        {
            UpgradeConfig config = CreateAsset<UpgradeConfig>($"Assets/_Project/ScriptableObjects/Upgrades/Upgrade_{id}.asset");
            SetString(config, "id", id);
            SetString(config, "displayName", displayName);
            SetString(config, "description", description);
            SetString(config, "displayNameKey", $"upgrade.{id}.name");
            SetString(config, "descriptionKey", $"upgrade.{id}.desc");
            SetEnum(config, "effectType", (int)effectType);
            SetFloat(config, "value", value);
            SetInt(config, "selectionWeight", selectionWeight);
            return config;
        }

        private static LocalizationTable CreateLocalizationTable()
        {
            LocalizationTable table = CreateAsset<LocalizationTable>("Assets/_Project/ScriptableObjects/Localization/Localization_RU.asset");
            SetEnum(table, "defaultLanguage", (int)GameLanguage.Russian);

            SerializedObject serialized = new SerializedObject(table);
            SerializedProperty entries = serialized.FindProperty("entries");
            entries.arraySize = 63;

            int index = 0;
            SetLocalizationEntry(entries, index++, "hud.minerals", "Минералы: {0}", "Minerals: {0}");
            SetLocalizationEntry(entries, index++, "hud.core", "Ядро: {0}/{1}", "Core: {0}/{1}");
            SetLocalizationEntry(entries, index++, "hud.wave", "Волна: {0}/{1}", "Wave: {0}/{1}");
            SetLocalizationEntry(entries, index++, "hud.speed", "x{0:0.#}", "x{0:0.#}");

            SetLocalizationEntry(entries, index++, "phase.boot", "Загрузка", "Boot");
            SetLocalizationEntry(entries, index++, "phase.build", "Строительство", "Build");
            SetLocalizationEntry(entries, index++, "phase.wave", "Волна", "Wave");
            SetLocalizationEntry(entries, index++, "phase.upgrade", "Выбор улучшения", "Choose Upgrade");
            SetLocalizationEntry(entries, index++, "phase.victory", "Победа", "Victory");
            SetLocalizationEntry(entries, index++, "phase.defeat", "Поражение", "Defeat");

            SetLocalizationEntry(entries, index++, "button.build_cost", "{0} {1}", "{0} {1}");
            SetLocalizationEntry(entries, index++, "button.upgrade", "Улучшить", "Upgrade");
            SetLocalizationEntry(entries, index++, "button.upgrade_cost", "Улучшить {0}", "Upgrade {0}");
            SetLocalizationEntry(entries, index++, "button.sell_refund", "Продать {0}", "Sell {0}");
            SetLocalizationEntry(entries, index++, "button.start_wave", "Начать волну", "Start Wave");
            SetLocalizationEntry(entries, index++, "button.speed", "Скорость", "Speed");
            SetLocalizationEntry(entries, index++, "button.restart", "Заново", "Restart");
            SetLocalizationEntry(entries, index++, "button.close", "Закрыть", "Close");

            SetLocalizationEntry(entries, index++, "build.select_slot", "Выберите слот", "Select slot");
            SetLocalizationEntry(entries, index++, "build.moon_slot", "Слот луны", "Moon slot");
            SetLocalizationEntry(entries, index++, "build.occupied_slot", "{0} ур. {1}/{2}", "{0} Lv {1}/{2}");
            SetLocalizationEntry(entries, index++, "build.stats.empty", "Выберите слот, чтобы увидеть статы здания.", "Select a slot to see building stats.");
            SetLocalizationEntry(entries, index++, "build.stats.available", "Доступные здания:", "Available buildings:");
            SetLocalizationEntry(entries, index++, "build.stats.none", "Для этого слота нет доступных зданий.", "No buildings can be placed here.");
            SetLocalizationEntry(entries, index++, "build.stats.level", "Уровень {0}/{1}", "Level {0}/{1}");
            SetLocalizationEntry(entries, index++, "build.stats.production", "Добыча: {0} минералов каждые {1:0.#} с", "Production: {0} minerals every {1:0.#} s");
            SetLocalizationEntry(entries, index++, "build.stats.damage", "Урон: {0}", "Damage: {0}");
            SetLocalizationEntry(entries, index++, "build.stats.fire_rate", "Скорострельность: {0:0.#} выстр./с", "Fire rate: {0:0.#} shots/s");
            SetLocalizationEntry(entries, index++, "build.stats.range", "Дальность: {0:0.#}", "Range: {0:0.#}");
            SetLocalizationEntry(entries, index++, "build.stats.projectile_speed", "Скорость снаряда: {0:0.#}", "Projectile speed: {0:0.#}");
            SetLocalizationEntry(entries, index++, "build.stats.boost_radius", "Радиус: {0:0.#}", "Radius: {0:0.#}");
            SetLocalizationEntry(entries, index++, "build.stats.mining_boost", "+{0:0.#}% к добыче", "+{0:0.#}% mining");
            SetLocalizationEntry(entries, index++, "build.stats.fire_rate_boost", "+{0:0.#}% к скорострельности", "+{0:0.#}% fire rate");
            SetLocalizationEntry(entries, index++, "build.stats.summary_mine", "{0}: +{1} минералов / {2:0.#} с", "{0}: +{1} minerals / {2:0.#} s");
            SetLocalizationEntry(entries, index++, "build.stats.summary_cannon", "{0}: {1} урон, {2:0.#}/с, {3:0.#} дальн.", "{0}: {1} damage, {2:0.#}/s, {3:0.#} range");
            SetLocalizationEntry(entries, index++, "build.stats.summary_booster", "{0}: радиус {1:0.#}, +{2:0.#}% добыча, +{3:0.#}% скорострельность", "{0}: radius {1:0.#}, +{2:0.#}% mining, +{3:0.#}% fire rate");
            SetLocalizationEntry(entries, index++, "feedback.need_minerals", "Нужно {0} минералов", "Need {0} minerals");
            SetLocalizationEntry(entries, index++, "feedback.cannot_build", "Здесь нельзя строить", "Cannot build here");
            SetLocalizationEntry(entries, index++, "feedback.cannot_upgrade", "Сейчас нельзя улучшить", "Cannot upgrade now");
            SetLocalizationEntry(entries, index++, "feedback.max_level", "Максимальный уровень", "Max level reached");

            SetLocalizationEntry(entries, index++, "core.title", "Командное ядро", "Command Core");
            SetLocalizationEntry(entries, index++, "core.status", "Прочность: {0}/{1}\nДобыча: {2}/{3}\nОборона: {4}/{5}\nЩит: {6}/{7} ({8})", "Integrity: {0}/{1}\nMining: {2}/{3}\nDefense: {4}/{5}\nShield: {6}/{7} ({8})");

            SetLocalizationEntry(entries, index++, "gameover.victory_title", "Победа", "Victory");
            SetLocalizationEntry(entries, index++, "gameover.defeat_title", "Поражение", "Defeat");
            SetLocalizationEntry(entries, index++, "gameover.victory_body", "Командное ядро пережило все волны.", "The Command Core survived all waves.");
            SetLocalizationEntry(entries, index++, "gameover.defeat_body", "Командное ядро разрушено.", "Core integrity collapsed.");

            SetLocalizationEntry(entries, index++, "upgrade.choose_title", "Выберите улучшение", "Choose upgrade");
            SetLocalizationEntry(entries, index++, "upgrade.level_suffix", "\nур. {0}/{1}", "\nLv {0}/{1}");

            SetLocalizationEntry(entries, index++, "building.Mine.name", "Шахта", "Mine");
            SetLocalizationEntry(entries, index++, "building.Cannon.name", "Пушка", "Cannon");

            SetLocalizationEntry(entries, index++, "upgrade.MineYield.name", "Добыча шахт", "Mine Yield");
            SetLocalizationEntry(entries, index++, "upgrade.MineYield.desc", "Шахты добывают больше минералов.", "Mines produce more minerals.");
            SetLocalizationEntry(entries, index++, "upgrade.CannonDamage.name", "Урон пушек", "Cannon Damage");
            SetLocalizationEntry(entries, index++, "upgrade.CannonDamage.desc", "Пушки наносят больше урона.", "Cannons deal more damage.");
            SetLocalizationEntry(entries, index++, "upgrade.CoreRepair.name", "Ремонт ядра", "Core Repair");
            SetLocalizationEntry(entries, index++, "upgrade.CoreRepair.desc", "Восстановить прочность командного ядра.", "Restore Command Core integrity.");
            SetLocalizationEntry(entries, index++, "upgrade.CommandCoreMining.name", "Добывающее ядро", "Mining Core");
            SetLocalizationEntry(entries, index++, "upgrade.CommandCoreMining.desc", "Командное ядро получает минералы после каждой волны.", "Command Core gains minerals after each wave.");
            SetLocalizationEntry(entries, index++, "upgrade.CommandCoreDefense.name", "Оборонное ядро", "Defense Core");
            SetLocalizationEntry(entries, index++, "upgrade.CommandCoreDefense.desc", "Командное ядро стреляет по ближайшим врагам.", "Command Core shoots nearby enemies.");
            SetLocalizationEntry(entries, index++, "upgrade.CommandCoreShield.name", "Щитовое ядро", "Shield Core");
            SetLocalizationEntry(entries, index++, "upgrade.CommandCoreShield.desc", "Командное ядро блокирует столкновения врагов в каждой волне.", "Command Core blocks enemy impacts each wave.");

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(table);
            return table;
        }

        private static void SetLocalizationEntry(SerializedProperty entries, int index, string key, string russian, string english)
        {
            SerializedProperty entry = entries.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("key").stringValue = key;
            entry.FindPropertyRelative("russian").stringValue = russian;
            entry.FindPropertyRelative("english").stringValue = english;
        }

        private static T CreateAsset<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }

            return asset;
        }

        private static void SetMineValues(Object target, int amount, float interval)
        {
            SetInt(target, "productionAmount", amount);
            SetFloat(target, "productionInterval", interval);
        }

        private static void SetCannonValues(Object target, int damage, float fireRate, float range, float projectileSpeed)
        {
            SetInt(target, "damage", damage);
            SetFloat(target, "fireRate", fireRate);
            SetFloat(target, "range", range);
            SetFloat(target, "projectileSpeed", projectileSpeed);
        }

        private static void SetBoosterValues(Object target, float radius, float miningBoost, float fireRateBoost)
        {
            SetFloat(target, "boostRadius", radius);
            SetFloat(target, "miningBoostPercent", miningBoost);
            SetFloat(target, "fireRateBoostPercent", fireRateBoost);
        }

        private static void SetWaveGroups(WaveConfig wave, EnemyConfig asteroid, int asteroidCount, EnemyConfig scout, int scoutCount, int waveNumber)
        {
            SerializedObject serialized = new SerializedObject(wave);
            SerializedProperty groups = serialized.FindProperty("spawnGroups");
            int groupCount = scoutCount > 0 ? 2 : 1;
            groups.arraySize = groupCount;
            ConfigureGroup(groups.GetArrayElementAtIndex(0), asteroid, asteroidCount, 0.45f, waveNumber * 31f, 24f);

            if (groupCount > 1)
            {
                ConfigureGroup(groups.GetArrayElementAtIndex(1), scout, scoutCount, 0.65f, 180f + waveNumber * 27f, 18f);
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(wave);
        }

        private static void ConfigureGroup(SerializedProperty group, EnemyConfig enemy, int count, float delay, float angle, float spread)
        {
            group.FindPropertyRelative("enemy").objectReferenceValue = enemy;
            group.FindPropertyRelative("count").intValue = count;
            group.FindPropertyRelative("delayBetweenSpawns").floatValue = delay;
            group.FindPropertyRelative("angleDegrees").floatValue = angle;
            group.FindPropertyRelative("angleSpreadDegrees").floatValue = spread;
        }

        private static TMP_Text CreateText(string name, Transform parent, string value, float size, TextAlignmentOptions alignment, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = size;
            text.enableAutoSizing = true;
            text.fontSizeMin = 18f;
            text.fontSizeMax = size;
            text.alignment = alignment;
            text.color = new Color(0.90f, 0.96f, 1f, 1f);
            return text;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = go.GetComponent<Image>();
            image.color = color;
            Button button = go.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.20f);
            colors.disabledColor = new Color(0.22f, 0.24f, 0.30f, 0.45f);
            button.colors = colors;

            TMP_Text text = CreateText("Label", go.transform, label, 28f, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
            text.color = Color.white;
            return button;
        }

        private static void ConfigureButtonLabel(Button button, float maxSize, float minSize)
        {
            TMP_Text text = button.GetComponentInChildren<TMP_Text>();
            if (text == null)
            {
                return;
            }

            text.fontSize = maxSize;
            text.fontSizeMax = maxSize;
            text.fontSizeMin = minSize;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.margin = new Vector4(12f, 8f, 12f, 8f);
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private static BuildSlotSelector[] Combine(BuildSlotSelector[] first, BuildSlotSelector[] second, BuildSlotSelector[] third)
        {
            BuildSlotSelector[] combined = new BuildSlotSelector[first.Length + second.Length + third.Length];
            first.CopyTo(combined, 0);
            second.CopyTo(combined, first.Length);
            third.CopyTo(combined, first.Length + second.Length);
            return combined;
        }

        private static BuildSlotSelector[] Combine(BuildSlotSelector[] first, BuildSlotSelector[] second)
        {
            BuildSlotSelector[] combined = new BuildSlotSelector[first.Length + second.Length];
            first.CopyTo(combined, 0);
            second.CopyTo(combined, first.Length);
            return combined;
        }

        private static void SetObject(Object target, string propertyName, Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetObjectArray(Object target, string propertyName, Object[] values)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetEnumArray(Object target, string propertyName, BuildSlotType[] values)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).enumValueIndex = (int)values[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetString(Object target, string propertyName, string value)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetInt(Object target, string propertyName, int value)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).intValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetFloat(Object target, string propertyName, float value)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetColor(Object target, string propertyName, Color value)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).colorValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetEnum(Object target, string propertyName, int value)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).enumValueIndex = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static string ToAbsolute(string assetPath)
        {
            return Path.Combine(Directory.GetParent(Application.dataPath).FullName, assetPath);
        }
    }
}
