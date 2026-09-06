using UnityEngine;

namespace OrbitalDefense
{
    [RequireComponent(typeof(BuildSlot))]
    public sealed class BuildSlotSelectionFeedback : MonoBehaviour
    {
        [SerializeField] private float selectedScale = 1.55f;
        [SerializeField] private float smoothSpeed = 14f;

        private Vector3 baseScale;
        private bool isSelected;

        private void Awake()
        {
            baseScale = transform.localScale;
        }

        private void Update()
        {
            Vector3 targetScale = isSelected ? baseScale * selectedScale : baseScale;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * smoothSpeed);
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
        }
    }
}
