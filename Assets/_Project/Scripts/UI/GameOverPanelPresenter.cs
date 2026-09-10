using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace OrbitalDefense
{
    public sealed class GameOverPanelPresenter : MonoBehaviour
    {
        [SerializeField] private GameStateController gameState;
        [SerializeField] private LocalizationService localization;

        private UIDocument document;
        private VisualElement panelRoot;
        private Label titleText;
        private Label bodyText;

        private void Awake()
        {
            gameState ??= FindFirstObjectByType<GameStateController>();
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
            panelRoot = root.Q<VisualElement>("gameover-panel");
            titleText = root.Q<Label>("gameover-title");
            bodyText = root.Q<Label>("gameover-body");
            root.Q<Button>("gameover-restart")?.RegisterCallback<ClickEvent>(_ => Restart());
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            bool visible = phase == GamePhase.Victory || phase == GamePhase.Defeat;
            if (panelRoot != null) panelRoot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (!visible) return;
            if (titleText != null) titleText.text = Text(phase == GamePhase.Victory ? "gameover.victory_title" : "gameover.defeat_title");
            if (bodyText != null) bodyText.text = Text(phase == GamePhase.Victory ? "gameover.victory_body" : "gameover.defeat_body");
        }

        private void RefreshCurrentPhase() => HandlePhaseChanged(gameState != null ? gameState.CurrentPhase : GamePhase.BuildPhase);
        private string Text(string key, params object[] args) => localization != null ? localization.Text(key, args) : key;
    }
}
