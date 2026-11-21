using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace FestiveSpaces
{
    /// <summary>Places the currently selected catalogue item only when a tracked plane is hit.</summary>
    public class ARPlacementController : MonoBehaviour
    {
        public ARRaycastManager raycastManager;
        public Camera arCamera;
        public DecorationManipulator manipulator;
        public AppNavigation navigation;
        public GameObject planeMarker;
        public bool placementEnabled = true;
        static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();
        GameObject currentMarker;
        GameObject selectedObject;
        readonly List<GameObject> placedObjects = new List<GameObject>();

        public bool HasSelectedObject => selectedObject != null;
        public void SetSelectedObject(GameObject prefab) { selectedObject = prefab; }

        void Update()
        {
            if (!placementEnabled || Input.touchCount == 0) return;
            Touch touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began || navigation == null || navigation.IsPointerOverUI(touch.position)) return;

            // A tap on an existing decoration selects it instead of placing a duplicate beneath it.
            if (arCamera != null && Physics.Raycast(arCamera.ScreenPointToRay(touch.position), out RaycastHit objectHit) &&
                objectHit.transform.GetComponentInParent<DecorationInstance>() != null)
            { manipulator.Select(objectHit.transform.GetComponentInParent<DecorationInstance>().gameObject); return; }

            if (raycastManager.Raycast(touch.position, Hits, TrackableType.PlaneWithinPolygon))
            {
                Pose pose = Hits[0].pose;
                if (selectedObject != null)
                {
                    var placed = Instantiate(selectedObject, pose.position, pose.rotation);
                    placed.name = selectedObject.name;
                    placed.hideFlags = HideFlags.None;
                    placed.SetActive(true);
                    placedObjects.Add(placed);
                    manipulator.Select(placed);
                }
                if (currentMarker == null && planeMarker != null) currentMarker = Instantiate(planeMarker);
                if (currentMarker != null) currentMarker.transform.SetPositionAndRotation(pose.position, pose.rotation);
                navigation.SetStatus("Decoration placed! Tap to add another.");
            }
            else navigation.SetStatus("No surface here yet. Move your phone slowly to scan your surroundings.");
        }

        public void ClearMarker() { if (currentMarker != null) Destroy(currentMarker); }
        public void ClearPlacedObjects()
        {
            foreach (GameObject item in placedObjects) if (item != null) Destroy(item);
            placedObjects.Clear();
            if (manipulator != null) manipulator.Select(null);
        }
    }
}
