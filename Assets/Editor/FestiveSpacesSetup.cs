#if UNITY_EDITOR
using FestiveSpaces;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;

public static class FestiveSpacesSetup
{
    const string ScenePath = "Assets/Scenes/FestiveSpaces.unity";
    const string PlanePrefabPath = "Assets/Prefabs/DetectedPlane.prefab";

    [MenuItem("Festive Spaces/Create Starter AR Scene")]
    public static void CreateStarterScene()
    {
        EnsureFolders();
        CreateDecorationPrefabs();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var sessionObject = new GameObject("AR Session");
        var session = sessionObject.AddComponent<ARSession>();

        var originObject = new GameObject("XR Origin (AR)");
        var origin = originObject.AddComponent<XROrigin>();
        var cameraObject = new GameObject("AR Camera", typeof(Camera), typeof(AudioListener), typeof(ARCameraManager), typeof(ARCameraBackground));
        cameraObject.tag = "MainCamera";
        var cameraOffset = new GameObject("Camera Offset");
        cameraOffset.transform.SetParent(originObject.transform, false);
        cameraObject.transform.SetParent(cameraOffset.transform, false);
        cameraObject.transform.localPosition = Vector3.zero;
        cameraObject.transform.localRotation = Quaternion.identity;
        var camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        origin.Camera = camera;
        origin.CameraFloorOffsetObject = cameraOffset;
        originObject.AddComponent<ARInputManager>();

        var plane = CreatePlanePrefab();
        var planeManager = originObject.AddComponent<ARPlaneManager>();
        planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
        planeManager.planePrefab = plane;
        var raycast = originObject.AddComponent<ARRaycastManager>();

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        var controller = originObject.AddComponent<ARPlacementController>();
        var manipulator = originObject.AddComponent<DecorationManipulator>();
        var screenshots = originObject.AddComponent<ScreenshotService>();
        var navigation = originObject.AddComponent<AppNavigation>();
        var bootstrap = originObject.AddComponent<ARSessionBootstrap>();
        controller.raycastManager = raycast; controller.arCamera = camera; controller.manipulator = manipulator; controller.navigation = navigation;
        manipulator.arCamera = camera; manipulator.navigation = navigation;
        navigation.placement = controller; navigation.manipulator = manipulator; navigation.screenshots = screenshots;
        bootstrap.session = session; bootstrap.planes = planeManager; bootstrap.navigation = navigation;

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        Debug.Log("Festive Spaces starter AR scene created at " + ScenePath);
    }

    static GameObject CreatePlanePrefab()
    {
        if (AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/DetectedPlane.mat") != null)
            AssetDatabase.DeleteAsset("Assets/Materials/DetectedPlane.mat");
        var planeShader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Standard");
        var material = new Material(planeShader);
        material.name = "Plane highlight material";
        material.color = new Color(.15f,.78f,.8f,.2f);
        material.SetOverrideTag("RenderType","Transparent");
        // Configure the selected shader's surface mode before setting its transparent render queue.
        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_AlphaClip", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }
        if (material.HasProperty("_Mode")) material.SetFloat("_Mode", 3f);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite",0); material.DisableKeyword("_ALPHATEST_ON"); material.EnableKeyword("_ALPHABLEND_ON");
        material.renderQueue=(int)UnityEngine.Rendering.RenderQueue.Transparent;
        AssetDatabase.CreateAsset(material,"Assets/Materials/DetectedPlane.mat");
        var root=new GameObject("Detected Plane",typeof(ARPlane),typeof(MeshFilter),typeof(MeshRenderer),typeof(ARPlaneMeshVisualizer));
        root.GetComponent<MeshRenderer>().sharedMaterial=material;
        var prefab=PrefabUtility.SaveAsPrefabAsset(root,PlanePrefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    static void CreateDecorationPrefabs()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Decorations")) AssetDatabase.CreateFolder("Assets/Resources", "Decorations");
        foreach (Festival festival in System.Enum.GetValues(typeof(Festival)))
        foreach (DecorationDefinition item in FestivalCatalog.Items(festival))
        {
            string key = item.displayName.Replace(" ", "");
            string path = "Assets/Resources/Decorations/" + key + ".prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) continue;
            GameObject visual = FestivalCatalog.CreatePrimitiveDecoration(item);
            string materialPath="Assets/Materials/"+key+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(material==null)
            {
                material=new Material(visual.GetComponent<Renderer>().sharedMaterial) { name=key+" material" };
                AssetDatabase.CreateAsset(material,materialPath);
            }
            visual.GetComponent<Renderer>().sharedMaterial=material;
            PrefabUtility.SaveAsPrefabAsset(visual, path);
            Object.DestroyImmediate(visual);
        }
    }

    static void EnsureFolders()
    {
        foreach(string folder in new[]{"Assets/Scenes","Assets/Prefabs","Assets/Materials","Assets/Models","Assets/Textures","Assets/UI"})
            if(!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets",folder.Substring("Assets/".Length));
    }
}
#endif
