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

        private float angleDegrees;
        private bool visible;

        private void Awake()
        {
            gameState ??= FindFirstObjectByType<GameStateController>();
            waveSystem ??= FindFirstObjectByType<WaveSystem>();
            warningRect ??= transform as RectTransform;
            canvasGroup ??= GetComponent<CanvasGroup>();
            warningText ??= GetComponentInChildren<TMP_Text>();
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
                UpdatePosition();
            }

            float targetAlpha = visible ? 1f : 0f;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * fadeSpeed);

            float pulse = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseScale;
            warningRect.localScale = visible ? Vector3.one * pulse : Vector3.one * 0.85f;
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

            Show(stageAngleDegrees);
        }

        private void HandleStageWarningEnded()
        {
            if (gameState != null && gameState.IsGameOver)
            {
                HideImmediate();
                return;
            }

            HideImmediate();
        }

        private void RefreshForPhase(GamePhase phase)
        {
            if (phase == GamePhase.Victory || phase == GamePhase.Defeat)
            {
                HideImmediate();
                return;
            }

            if (phase == GamePhase.Wave)
            {
                HideImmediate();
                return;
            }

            if (waveSystem != null && waveSystem.TryGetNextWaveDirection(out float nextAngle))
            {
                Show(nextAngle);
                return;
            }

            HideImmediate();
        }

        private void Show(float stageAngleDegrees)
        {
            angleDegrees = stageAngleDegrees;
            visible = true;

            if (warningText != null)
            {
                warningText.text = "!";
            }

            UpdatePosition();
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
        }

        private void UpdatePosition()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return;
            }

            RectTransform canvasRect = canvas.transform as RectTransform;
            if (canvasRect == null)
            {
                return;
            }

            Vector2 direction = new Vector2(Mathf.Cos(angleDegrees * Mathf.Deg2Rad), Mathf.Sin(angleDegrees * Mathf.Deg2Rad));
            float radius = Mathf.Min(canvasRect.rect.width, canvasRect.rect.height) * edgeRadiusFactor;
            warningRect.anchoredPosition = direction * radius;
        }
    }
}
