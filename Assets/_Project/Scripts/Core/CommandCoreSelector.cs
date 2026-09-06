using UnityEngine;
using UnityEngine.EventSystems;

namespace OrbitalDefense
{
    [RequireComponent(typeof(CommandCoreUpgrade))]
    public sealed class CommandCoreSelector : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private CommandCorePanelPresenter corePanel;
        [SerializeField] private BuildPanelPresenter buildPanel;

        private CommandCoreUpgrade coreUpgrade;

        private void Awake()
        {
            coreUpgrade = GetComponent<CommandCoreUpgrade>();
            corePanel ??= FindAnyObjectByType<CommandCorePanelPresenter>();
            buildPanel ??= FindAnyObjectByType<BuildPanelPresenter>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            buildPanel?.Hide();
            corePanel?.SelectCore(coreUpgrade);
        }
    }
}
