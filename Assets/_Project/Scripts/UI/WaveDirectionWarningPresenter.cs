using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace OrbitalDefense
{
    public sealed class WaveDirectionWarningPresenter : MonoBehaviour
    {
        [SerializeField] private GameStateController gameState;
        [SerializeField] private WaveSystem waveSystem;
        [SerializeField] private RectTransform warningRect;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text warningText;
        [SerializeField] private float edgeRadiusFactor = 0.38f;
        [SerializeField] private float pulseScale = 0.08f;
        [SerializeField] private float pulseSpeed = 4f;
        [SerializeField] private float fadeSpeed = 8f;
        [SerializeField] private Color firstDirectionColor = new(1f, 0.18f, 0.12f, 1f);
        [SerializeField] private Color otherDirectionColor = new(1f, 0.94f, 0.36f, 1f);

        private readonly List<float> activeAngles = new();
        private readonly List<TMP_Text> markers = new();
        private readonly List<float> nextWaveAngles = new();
        private bool visible;

        private void Awake()
        {
            gameState ??= FindFirstObjectByType<GameStateController>();
            waveSystem ??= FindFirstObjectByType<WaveSystem>();
            warningRect ??= transform as RectTransform;
            canvasGroup ??= GetComponent<CanvasGroup>();
            warningText ??= GetComponentInChildren<TMP_Text>();
            CacheTemplateMarker();
        }

        private void OnEnable()
        {
            if (gameState != null)
            {
                gameState.PhaseChanged += HandlePhaseChanged;
            }

            if (waveSystem != null)
            {
                waveSystem.StageWarningStarted += HandleStageWarningStarted;
                waveSystem.StageWarningEnded += HandleStageWarningEnded;
            }
        }

        private void OnDisable()
        {
            if (gameState != null)
            {
                gameState.PhaseChanged -= HandlePhaseChanged;
            }

            if (waveSystem != null)
            {
                waveSystem.StageWarningStarted -= HandleStageWarningStarted;
                waveSystem.StageWarningEnded -= HandleStageWarningEnded;
            }
        }

        private void Start()
        {
            RefreshForPhase(gameState != null ? gameState.CurrentPhase : GamePhase.BuildPhase);
        }

        private void Update()
        {
            if (warningRect == null || canvasGroup == null)
            {
                return;
            }

            if (visible)
            {
                UpdatePositions();
            }

            float targetAlpha = visible ? 1f : 0f;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * fadeSpeed);

            float pulse = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseScale;
            for (int i = 0; i < markers.Count; i++)
            {
                RectTransform markerRect = markers[i].rectTransform;
                markerRect.localScale = markers[i].gameObject.activeSelf && visible ? Vector3.one * pulse : Vector3.one * 0.85f;
            }
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            RefreshForPhase(phase);
        }

        private void HandleStageWarningStarted(float stageAngleDegrees)
        {
            if (gameState != null && gameState.IsGameOver)
            {
                HideImmediate();
                return;
            }

            ShowSingle(stageAngleDegrees, firstDirectionColor);
        }

        private void HandleStageWarningEnded()
        {
            if (gameState != null && (gameState.IsGameOver || gameState.IsWaveActive))
            {
                HideImmediate();
                return;
            }

            RefreshForPhase(gameState != null ? gameState.CurrentPhase : GamePhase.BuildPhase);
        }

        private void RefreshForPhase(GamePhase phase)
        {
            if (phase == GamePhase.Victory || phase == GamePhase.Defeat)
            {
                HideImmediate();
                return;
            }

            if (waveSystem != null && phase == GamePhase.BuildPhase && waveSystem.GetNextWaveDirections(nextWaveAngles) > 0)
            {
                ShowMany(nextWaveAngles);
                return;
            }

            HideImmediate();
        }

        private void ShowSingle(float stageAngleDegrees, Color color)
        {
            activeAngles.Clear();
            activeAngles.Add(stageAngleDegrees);
            visible = true;

            EnsureMarkerCount(1);
            ConfigureMarker(0, color);
            HideUnusedMarkers(1);
            UpdatePositions();
        }

        private void ShowMany(IReadOnlyList<float> stageAngles)
        {
            activeAngles.Clear();
            for (int i = 0; i < stageAngles.Count; i++)
            {
                activeAngles.Add(stageAngles[i]);
            }

            visible = activeAngles.Count > 0;
            EnsureMarkerCount(activeAngles.Count);
            for (int i = 0; i < activeAngles.Count; i++)
            {
                ConfigureMarker(i, i == 0 ? firstDirectionColor : otherDirectionColor);
            }

            HideUnusedMarkers(activeAngles.Count);
            UpdatePositions();
        }

        private void HideImmediate()
        {
            visible = false;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            if (warningRect != null)
            {
                warningRect.localScale = Vector3.one * 0.85f;
            }

            HideUnusedMarkers(0);
        }

        private void UpdatePositions()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null || warningRect == null)
            {
                return;
            }

            RectTransform canvasRect = canvas.transform as RectTransform;
            if (canvasRect == null)
            {
                return;
            }

            float radius = Mathf.Min(canvasRect.rect.width, canvasRect.rect.height) * edgeRadiusFactor;
            for (int i = 0; i < activeAngles.Count && i < markers.Count; i++)
            {
                Vector2 direction = new(Mathf.Cos(activeAngles[i] * Mathf.Deg2Rad), Mathf.Sin(activeAngles[i] * Mathf.Deg2Rad));
                markers[i].rectTransform.anchoredPosition = direction * radius;
            }
        }

        private void CacheTemplateMarker()
        {
            if (warningText == null || markers.Contains(warningText))
            {
                return;
            }

            markers.Add(warningText);
        }

        private void EnsureMarkerCount(int count)
        {
            CacheTemplateMarker();
            if (warningText == null)
            {
                return;
            }

            while (markers.Count < count)
            {
                TMP_Text marker = Instantiate(warningText, warningText.transform.parent);
                marker.name = $"WarningText_{markers.Count + 1:00}";
                markers.Add(marker);
            }
        }

        private void ConfigureMarker(int index, Color color)
        {
            if (index < 0 || index >= markers.Count)
            {
                return;
            }

            TMP_Text marker = markers[index];
            marker.text = "!";
            marker.color = color;
            marker.gameObject.SetActive(true);
            ConfigureMarkerRect(marker.rectTransform);
        }

        private void HideUnusedMarkers(int usedCount)
        {
            for (int i = usedCount; i < markers.Count; i++)
            {
                if (markers[i] != null)
                {
                    markers[i].gameObject.SetActive(false);
                }
            }
        }

        private static void ConfigureMarkerRect(RectTransform markerRect)
        {
            if (markerRect == null)
            {
                return;
            }

            markerRect.anchorMin = new Vector2(0.5f, 0.5f);
            markerRect.anchorMax = new Vector2(0.5f, 0.5f);
            markerRect.pivot = new Vector2(0.5f, 0.5f);
            markerRect.sizeDelta = new Vector2(96f, 96f);
        }
    }
}
