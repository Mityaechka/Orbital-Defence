using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace OrbitalDefense
{
    public sealed class WaveDirectionWarningPresenter : MonoBehaviour
    {
        [SerializeField] private GameStateController gameState;
        [SerializeField] private WaveSystem waveSystem;
        [SerializeField] private float edgeRadiusFactor = 0.38f;
        [SerializeField] private float pulseScale = 0.08f;
        [SerializeField] private float pulseSpeed = 4f;
        [SerializeField] private float fadeSpeed = 8f;
        [SerializeField] private Color firstDirectionColor = new(1f, 0.18f, 0.12f, 1f);
        [SerializeField] private Color otherDirectionColor = new(1f, 0.94f, 0.36f, 1f);

        private readonly List<float> activeAngles = new();
        private readonly List<Label> markers = new();
        private readonly List<float> nextWaveAngles = new();
        private UIDocument document;
        private VisualElement warningRoot;
        private bool visible;

        private void Awake()
        {
            gameState ??= FindFirstObjectByType<GameStateController>();
            waveSystem ??= FindFirstObjectByType<WaveSystem>();
            document = FindFirstObjectByType<UIDocument>();
        }

        private void OnEnable()
        {
            if (gameState != null) gameState.PhaseChanged += HandlePhaseChanged;
            if (waveSystem != null)
            {
                waveSystem.StageWarningStarted += HandleStageWarningStarted;
                waveSystem.StageWarningEnded += HandleStageWarningEnded;
            }
        }

        private void OnDisable()
        {
            if (gameState != null) gameState.PhaseChanged -= HandlePhaseChanged;
            if (waveSystem != null)
            {
                waveSystem.StageWarningStarted -= HandleStageWarningStarted;
                waveSystem.StageWarningEnded -= HandleStageWarningEnded;
            }
        }

        private void Start()
        {
            if (document != null) warningRoot = document.rootVisualElement?.Q<VisualElement>("warning-layer");
            RefreshForPhase(gameState != null ? gameState.CurrentPhase : GamePhase.BuildPhase);
        }

        private void Update()
        {
            if (warningRoot == null) return;
            float targetAlpha = visible ? 1f : 0f;
            warningRoot.style.opacity = Mathf.MoveTowards(warningRoot.resolvedStyle.opacity, targetAlpha, Time.unscaledDeltaTime * fadeSpeed);
            if (!visible) return;
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseScale;
            for (int i = 0; i < markers.Count; i++)
            {
                if (markers[i].style.display == DisplayStyle.None) continue;
                markers[i].style.scale = new Scale(Vector3.one * pulse);
            }
            UpdatePositions();
        }

        private void HandlePhaseChanged(GamePhase phase) => RefreshForPhase(phase);
        private void HandleStageWarningStarted(float angle)
        {
            if (gameState != null && gameState.IsGameOver) { HideImmediate(); return; }
            activeAngles.Clear();
            activeAngles.Add(angle);
            ShowMarkers(firstDirectionColor);
        }
        private void HandleStageWarningEnded()
        {
            if (gameState != null && (gameState.IsGameOver || gameState.IsWaveActive)) { HideImmediate(); return; }
            RefreshForPhase(gameState != null ? gameState.CurrentPhase : GamePhase.BuildPhase);
        }
        private void RefreshForPhase(GamePhase phase)
        {
            if (phase == GamePhase.Victory || phase == GamePhase.Defeat) { HideImmediate(); return; }
            if (waveSystem != null && phase == GamePhase.BuildPhase && waveSystem.GetNextWaveDirections(nextWaveAngles) > 0)
            {
                activeAngles.Clear();
                activeAngles.AddRange(nextWaveAngles);
                ShowMarkers(firstDirectionColor);
                return;
            }
            HideImmediate();
        }

        private void ShowMarkers(Color primaryColor)
        {
            visible = activeAngles.Count > 0;
            EnsureMarkerCount(activeAngles.Count);
            for (int i = 0; i < markers.Count; i++)
            {
                bool used = i < activeAngles.Count;
                markers[i].style.display = used ? DisplayStyle.Flex : DisplayStyle.None;
                if (used) markers[i].style.color = i == 0 ? primaryColor : otherDirectionColor;
            }
            UpdatePositions();
        }

        private void HideImmediate()
        {
            visible = false;
            if (warningRoot != null) warningRoot.style.opacity = 0f;
            for (int i = 0; i < markers.Count; i++) markers[i].style.display = DisplayStyle.None;
        }

        private void EnsureMarkerCount(int count)
        {
            if (warningRoot == null) return;
            while (markers.Count < count)
            {
                Label marker = new("!") { name = $"warning-marker-{markers.Count:00}" };
                marker.AddToClassList("warning-marker");
                warningRoot.Add(marker);
                markers.Add(marker);
            }
        }

        private void UpdatePositions()
        {
            if (warningRoot == null || warningRoot.layout.width <= 0f || warningRoot.layout.height <= 0f) return;
            float radius = Mathf.Min(warningRoot.layout.width, warningRoot.layout.height) * edgeRadiusFactor;
            Vector2 center = new(warningRoot.layout.width * 0.5f, warningRoot.layout.height * 0.5f);
            for (int i = 0; i < activeAngles.Count && i < markers.Count; i++)
            {
                Vector2 direction = new(Mathf.Cos(activeAngles[i] * Mathf.Deg2Rad), -Mathf.Sin(activeAngles[i] * Mathf.Deg2Rad));
                Vector2 position = center + direction * radius - new Vector2(36f, 36f);
                markers[i].style.left = position.x;
                markers[i].style.top = position.y;
            }
        }
    }
}
