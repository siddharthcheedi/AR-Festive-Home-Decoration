using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace FestiveSpaces
{
    /// <summary>Requests camera access and reports the scan hint until at least one plane is found.</summary>
    public class ARSessionBootstrap : MonoBehaviour
    {
        public ARSession session;
        public ARPlaneManager planes;
        public AppNavigation navigation;
        IEnumerator Start()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
                while (!Permission.HasUserAuthorizedPermission(Permission.Camera)) yield return null;
            }
#endif
            if (session != null) session.enabled = true;
            yield return ARSession.CheckAvailability();
            if (ARSession.state == ARSessionState.NeedsInstall) yield return ARSession.Install();
            while (true)
            {
                if (navigation != null && navigation.IsARMode && planes != null && planes.trackables.count == 0)
                    navigation.SetStatus("Move your phone slowly to scan your surroundings.");
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
