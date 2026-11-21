using UnityEngine;

namespace FestiveSpaces
{
    /// <summary>Single-finger drag, two-finger twist and pinch, plus simple panel controls.</summary>
    public class DecorationManipulator : MonoBehaviour
    {
        public Camera arCamera;
        public AppNavigation navigation;
        Transform selected;
        Plane dragPlane;
        Vector3 dragOffset;
        float previousPinch;
        float previousAngle;
        bool dragging;
        public Transform Selected => selected;

        public void Select(GameObject item) { selected = item != null ? item.transform : null; }
        public void DeleteSelected() { if (selected != null) Destroy(selected.gameObject); selected = null; }
        public void RotateSelected(float degrees = 20f) { if (selected != null) selected.Rotate(Vector3.up, degrees, Space.World); }
        public void ResizeSelected(float multiplier = 1.15f) { if (selected != null) selected.localScale = Vector3.ClampMagnitude(selected.localScale * multiplier, 2f); }

        void Update()
        {
            if (Input.touchCount == 0 || arCamera == null) return;
            if (Input.touchCount >= 2) { dragging = false; HandlePinchAndTwist(); return; }
            Touch touch = Input.GetTouch(0);
            if (navigation != null && navigation.IsPointerOverUI(touch.position)) return;
            if (touch.phase == TouchPhase.Began)
            {
                dragging = false;
                Ray ray = arCamera.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.GetComponentInParent<DecorationInstance>() != null)
                {
                    selected = hit.transform;
                    dragging = true;
                    dragPlane = new Plane(-arCamera.transform.forward, selected.position);
                    if (dragPlane.Raycast(ray, out float distance)) dragOffset = selected.position - ray.GetPoint(distance);
                }
            }
            else if (touch.phase == TouchPhase.Moved && dragging && selected != null && dragPlane.Raycast(arCamera.ScreenPointToRay(touch.position), out float distance))
            {
                selected.position = arCamera.ScreenPointToRay(touch.position).GetPoint(distance) + dragOffset;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) dragging = false;
        }

        void HandlePinchAndTwist()
        {
            if (selected == null || (navigation != null && (navigation.IsPointerOverUI(Input.GetTouch(0).position) || navigation.IsPointerOverUI(Input.GetTouch(1).position)))) return;
            Touch a = Input.GetTouch(0), b = Input.GetTouch(1);
            float distance = Vector2.Distance(a.position, b.position);
            float angle = Mathf.Atan2(b.position.y - a.position.y, b.position.x - a.position.x) * Mathf.Rad2Deg;
            if (a.phase == TouchPhase.Began || b.phase == TouchPhase.Began) { previousPinch = distance; previousAngle = angle; return; }
            selected.localScale = Vector3.ClampMagnitude(selected.localScale * (distance / Mathf.Max(1f, previousPinch)), 2f);
            selected.Rotate(Vector3.up, Mathf.DeltaAngle(previousAngle, angle), Space.World);
            previousPinch = distance; previousAngle = angle;
        }
    }
}
