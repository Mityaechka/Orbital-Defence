using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OrbitalDefense
{
    public sealed class GameOverPanelPresenter : MonoBehaviour
    {
        [SerializeField] private GameStateController gameState;
        [SerializeField] private LocalizationService localization;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Button restartButton;

        private void Awake()
        {
            gameState ??= FindFirstObjectByType<GameStateController>();
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

        public void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            bool isVictory = phase == GamePhase.Victory;
            bool isDefeat = phase == GamePhase.Defeat;
            bool isGameOver = isVictory || isDefeat;

            panelRoot.SetActive(isGameOver);

            if (titleText != null)
            {
                titleText.text = isVictory ? Text("gameover.victory_title") : isDefeat ? Text("gameover.defeat_title") : string.Empty;
            }

            if (bodyText != null)
            {
                bodyText.text = isVictory ? Text("gameover.victory_body") : isDefeat ? Text("gameover.defeat_body") : string.Empty;
            }

            if (restartButton != null)
            {
                restartButton.interactable = isGameOver;
                SetButtonLabel(restartButton, Text("button.restart"));
            }
        }

        private void RefreshCurrentPhase()
        {
            HandlePhaseChanged(gameState != null ? gameState.CurrentPhase : GamePhase.BuildPhase);
        }

        private string Text(string key, params object[] args)
        {
            return localization != null ? localization.Text(key, args) : key;
        }

        private static void SetButtonLabel(Button button, string label)
        {
            TMP_Text text = button != null ? button.GetComponentInChildren<TMP_Text>() : null;
            if (text != null)
            {
                text.text = label;
            }
        }
    }
}
