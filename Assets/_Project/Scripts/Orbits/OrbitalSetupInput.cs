using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OrbitalDefense
{
    [RequireComponent(typeof(OrbitMover))]
    public sealed class OrbitalSetupInput : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private OrbitalSetupController setupController;
        [SerializeField] private Camera worldCamera;

        private OrbitMover orbitMover;
        private bool dragMovedMoon;
        private Coroutine clearDragFlagCoroutine;

        public static bool IsDraggingMoon { get; private set; }

        private void Awake()
        {
            orbitMover = GetComponent<OrbitMover>();
            setupController ??= FindFirstObjectByType<OrbitalSetupController>();
            worldCamera ??= Camera.main;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (CanHandleInput())
            {
                setupController.SelectMoon(orbitMover);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragMovedMoon = false;
            if (!CanHandleInput())
            {
                return;
            }

            setupController.SelectMoon(orbitMover);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!CanHandleInput())
            {
                return;
            }

            if (TryGetPointerAngle(eventData, out float angleDegrees))
            {
                dragMovedMoon = true;
                IsDraggingMoon = true;
                orbitMover.SetAngle(angleDegrees);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragMovedMoon)
            {
                ScheduleClearDragFlag();
            }

            dragMovedMoon = false;
        }

        private void OnDisable()
        {
            if (clearDragFlagCoroutine != null)
            {
                StopCoroutine(clearDragFlagCoroutine);
                clearDragFlagCoroutine = null;
            }

            dragMovedMoon = false;
            IsDraggingMoon = false;
        }

        private bool CanHandleInput()
        {
            return orbitMover != null && setupController != null && setupController.CanEditSetup && !orbitMover.IsLocked;
        }

        private bool TryGetPointerAngle(PointerEventData eventData, out float angleDegrees)
        {
            angleDegrees = 0f;
            Transform center = orbitMover.Center;
            if (center == null)
            {
                return false;
            }

            Camera camera = eventData.pressEventCamera != null ? eventData.pressEventCamera : worldCamera;
            camera ??= Camera.main;
            if (camera == null)
            {
                return false;
            }

            Vector3 screenPoint = new(eventData.position.x, eventData.position.y, -camera.transform.position.z);
            Vector3 worldPoint = camera.ScreenToWorldPoint(screenPoint);
            Vector2 direction = worldPoint - center.position;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            angleDegrees = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return true;
        }

        private void ScheduleClearDragFlag()
        {
            if (clearDragFlagCoroutine != null)
            {
                StopCoroutine(clearDragFlagCoroutine);
            }

            clearDragFlagCoroutine = StartCoroutine(ClearDragFlagAtEndOfFrame());
        }

        private IEnumerator ClearDragFlagAtEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            IsDraggingMoon = false;
            clearDragFlagCoroutine = null;
        }
    }
}
