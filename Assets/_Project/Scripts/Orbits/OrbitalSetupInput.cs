using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace OrbitalDefense
{
    [RequireComponent(typeof(OrbitMover))]
    public sealed class OrbitalSetupInput : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private OrbitalSetupController setupController;
        [SerializeField] private Camera worldCamera;

        private OrbitMover orbitMover;
        private Vector2 pointerDownScreenPosition;
        private int activePointerId = int.MinValue;
        private bool dragMovedMoon;
        private Coroutine clearDragFlagCoroutine;

        private static OrbitalSetupInput activeInput;

        public static bool IsDraggingMoon { get; private set; }

        private void Awake()
        {
            orbitMover = GetComponent<OrbitMover>();
            setupController ??= FindFirstObjectByType<OrbitalSetupController>();
            worldCamera ??= Camera.main;
        }

        private void Update()
        {
            ClearReleasedMouseCapture();
            TryBeginMouseCaptureFromPhysics();

            if (activeInput != this || activePointerId == int.MinValue)
            {
                return;
            }

            if (!CanHandleInput() || !TryGetActivePointerPosition(out Vector2 screenPosition))
            {
                EndPointerCapture();
                return;
            }

            if (!dragMovedMoon && !HasMovedPastDragThreshold(screenPosition))
            {
                return;
            }

            MoveToScreenPosition(screenPosition);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Moon selection is only a transient drag highlight now.
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!CanHandleInput())
            {
                return;
            }

            BeginPointerCapture(GetPointerId(eventData), eventData.position);
            setupController.SelectMoon(orbitMover);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (activePointerId != eventData.pointerId)
            {
                return;
            }

            EndPointerCapture();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (activePointerId != int.MinValue || !CanHandleInput())
            {
                return;
            }

            BeginPointerCapture(GetPointerId(eventData), eventData.position);
            setupController.SelectMoon(orbitMover);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!CanHandleInput() || (activePointerId != int.MinValue && activePointerId != eventData.pointerId))
            {
                return;
            }

            if (!dragMovedMoon && !HasMovedPastDragThreshold(eventData.position))
            {
                return;
            }

            MoveToScreenPosition(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (activePointerId != int.MinValue && activePointerId != eventData.pointerId)
            {
                return;
            }

            EndPointerCapture();
        }

        public bool IsHandlingPointer(int pointerId)
        {
            return activePointerId == pointerId;
        }

        private void BeginPointerCapture(int pointerId, Vector2 screenPosition)
        {
            if (activeInput != null && activeInput != this)
            {
                activeInput.EndPointerCapture();
            }

            activeInput = this;
            activePointerId = pointerId;
            pointerDownScreenPosition = screenPosition;
            dragMovedMoon = false;
        }

        private void EndPointerCapture()
        {
            if (dragMovedMoon)
            {
                ScheduleClearDragFlag();
            }

            if (setupController != null && setupController.SelectedMoon == orbitMover)
            {
                setupController.SelectMoon(null);
            }

            if (activeInput == this)
            {
                activeInput = null;
            }

            activePointerId = int.MinValue;
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
            activePointerId = int.MinValue;
            if (activeInput == this)
            {
                activeInput = null;
            }

            IsDraggingMoon = false;
        }

        private bool CanHandleInput()
        {
            return orbitMover != null && setupController != null && setupController.CanEditSetup && !orbitMover.IsLocked;
        }

        private bool HasMovedPastDragThreshold(Vector2 screenPosition)
        {
            return (screenPosition - pointerDownScreenPosition).sqrMagnitude >= 9f;
        }

        private static int GetPointerId(PointerEventData eventData)
        {
            return Mouse.current != null && Mouse.current.leftButton.isPressed ? -1 : eventData.pointerId;
        }

        private void TryBeginMouseCaptureFromPhysics()
        {
            if (activeInput != null || Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame || !CanHandleInput())
            {
                return;
            }

            Camera camera = worldCamera;
            camera ??= Camera.main;
            if (camera == null)
            {
                return;
            }

            Vector2 screenPosition = Mouse.current.position.ReadValue();
            Vector3 screenPoint = new(screenPosition.x, screenPosition.y, -camera.transform.position.z);
            Vector2 worldPoint = camera.ScreenToWorldPoint(screenPoint);
            Physics2D.SyncTransforms();
            Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint);

            for (int i = 0; i < hits.Length; i++)
            {
                OrbitalSetupInput hitInput = hits[i].GetComponent<OrbitalSetupInput>();
                if (hitInput != this)
                {
                    continue;
                }

                BeginPointerCapture(-1, screenPosition);
                setupController.SelectMoon(orbitMover);
                return;
            }
        }

        private static void ClearReleasedMouseCapture()
        {
            if (activeInput == null || activeInput.activePointerId >= 0 || Mouse.current == null || Mouse.current.leftButton.isPressed)
            {
                return;
            }

            activeInput.EndPointerCapture();
        }

        private void MoveToScreenPosition(Vector2 screenPosition)
        {
            if (TryGetPointerAngle(screenPosition, out float angleDegrees))
            {
                dragMovedMoon = true;
                IsDraggingMoon = true;
                orbitMover.SetAngle(angleDegrees);
            }
        }

        private bool TryGetActivePointerPosition(out Vector2 screenPosition)
        {
            if (activePointerId < 0)
            {
                if (Mouse.current == null)
                {
                    screenPosition = default;
                    return false;
                }

                screenPosition = Mouse.current.position.ReadValue();
                return Mouse.current.leftButton.isPressed;
            }

            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen == null)
            {
                screenPosition = default;
                return false;
            }

            foreach (TouchControl touch in touchscreen.touches)
            {
                if (touch.touchId.ReadValue() == activePointerId)
                {
                    screenPosition = touch.position.ReadValue();
                    UnityEngine.InputSystem.TouchPhase phase = touch.phase.ReadValue();
                    return phase != UnityEngine.InputSystem.TouchPhase.Ended && phase != UnityEngine.InputSystem.TouchPhase.Canceled;
                }
            }

            screenPosition = default;
            return false;
        }

        private bool TryGetPointerAngle(Vector2 screenPosition, out float angleDegrees)
        {
            angleDegrees = 0f;
            Transform center = orbitMover.Center;
            if (center == null)
            {
                return false;
            }

            Camera camera = worldCamera;
            camera ??= Camera.main;
            if (camera == null)
            {
                return false;
            }

            Vector3 screenPoint = new(screenPosition.x, screenPosition.y, -camera.transform.position.z);
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
