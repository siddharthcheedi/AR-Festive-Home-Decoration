using System;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace FestiveSpaces
{
    public class ScreenshotService : MonoBehaviour
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void SaveFestiveSpacesImage(string imagePath);
#endif
        public Action<string> Completed;
        public Canvas uiCanvas;
        public void Capture() { StartCoroutine(CaptureRoutine()); }

        IEnumerator CaptureRoutine()
        {
            if (uiCanvas != null) uiCanvas.enabled = false;
            yield return new WaitForEndOfFrame();
            Texture2D capture = UnityEngine.ScreenCapture.CaptureScreenshotAsTexture();
            byte[] png = UnityEngine.ImageConversion.EncodeToPNG(capture);
            Destroy(capture);
            if (uiCanvas != null) uiCanvas.enabled = true;
            string name = "FestiveSpaces_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
#if UNITY_ANDROID && !UNITY_EDITOR
            string result = SaveToGallery(name, png);
#else
            string result = Path.Combine(Application.persistentDataPath, "Screenshots", name);
            Directory.CreateDirectory(Path.GetDirectoryName(result)); File.WriteAllBytes(result, png);
#endif
            string appCopy = Path.Combine(Application.persistentDataPath, "Screenshots", name);
            Directory.CreateDirectory(Path.GetDirectoryName(appCopy)); File.WriteAllBytes(appCopy, png);
#if UNITY_IOS && !UNITY_EDITOR
            SaveFestiveSpacesImage(appCopy);
            result = "Photos and app storage";
#endif
            Completed?.Invoke(result);
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        string SaveToGallery(string name, byte[] png)
        {
            try
            {
                using (var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unity.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var resolver = activity.Call<AndroidJavaObject>("getContentResolver"))
                using (var values = new AndroidJavaObject("android.content.ContentValues"))
                using (var media = new AndroidJavaClass("android.provider.MediaStore$Images$Media"))
                {
                    values.Call("put", "_display_name", name);
                    values.Call("put", "mime_type", "image/png");
                    if (new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT") >= 29)
                        values.Call("put", "relative_path", "Pictures/FestiveSpaces");
                    AndroidJavaObject uri = resolver.Call<AndroidJavaObject>("insert", media.GetStatic<AndroidJavaObject>("EXTERNAL_CONTENT_URI"), values);
                    if (uri == null) throw new IOException("Gallery returned no image location.");
                    using (AndroidJavaObject stream = resolver.Call<AndroidJavaObject>("openOutputStream", uri)) stream.Call("write", png);
                    return uri.Call<string>("toString");
                }
            }
            catch (Exception error)
            {
                string fallback = Path.Combine(Application.persistentDataPath, "Screenshots", name);
                Directory.CreateDirectory(Path.GetDirectoryName(fallback)); File.WriteAllBytes(fallback, png);
                Debug.LogWarning("Gallery save failed; screenshot stored in app files: " + error.Message);
                return fallback;
            }
        }
#endif
    }
}
