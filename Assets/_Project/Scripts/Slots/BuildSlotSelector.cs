using UnityEngine;
using UnityEngine.EventSystems;

namespace OrbitalDefense
{
    [RequireComponent(typeof(BuildSlot))]
    public sealed class BuildSlotSelector : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private BuildPanelPresenter buildPanel;

        private BuildSlot slot;

        private void Awake()
        {
            slot = GetComponent<BuildSlot>();
            buildPanel ??= FindFirstObjectByType<BuildPanelPresenter>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            buildPanel?.SelectSlot(slot);
        }
    }
}
