using UnityEngine;
using UnityEngine.EventSystems;

namespace OrbitalDefense
{
    [RequireComponent(typeof(BuildSlot))]
    public sealed class BuildSlotSelector : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private BuildPanelPresenter buildPanel;

        private BuildSlot slot;
        private OrbitalSetupInput orbitalSetupInput;

        private void Awake()
        {
            slot = GetComponent<BuildSlot>();
            orbitalSetupInput = GetComponentInParent<OrbitalSetupInput>();
            buildPanel ??= FindFirstObjectByType<BuildPanelPresenter>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (OrbitalSetupInput.IsDraggingMoon)
            {
                return;
            }

            buildPanel?.SelectSlot(slot);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            orbitalSetupInput?.OnBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            orbitalSetupInput?.OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            orbitalSetupInput?.OnEndDrag(eventData);
        }
    }
}
