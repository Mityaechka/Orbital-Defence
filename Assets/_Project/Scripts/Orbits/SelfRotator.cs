using UnityEngine;

namespace OrbitalDefense
{
    public sealed class SelfRotator : MonoBehaviour
    {
        [SerializeField] private float degreesPerSecond = 10f;
        [SerializeField] private GameStateController gameState;

        public void Initialize(float rotationDegreesPerSecond)
        {
            degreesPerSecond = rotationDegreesPerSecond;
        }

        private void Awake()
        {
            gameState ??= FindFirstObjectByType<GameStateController>();
        }

        private void Update()
        {
            if (gameState != null && gameState.CurrentPhase != GamePhase.Wave)
            {
                return;
            }

            transform.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime);
        }
    }
}
