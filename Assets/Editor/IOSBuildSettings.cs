#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;

/// <summary>Adds the privacy strings required by the AR camera and Photos export.</summary>
public class IOSBuildSettings : IPostprocessBuildWithReport
{
    public int callbackOrder => 1000;

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.iOS) return;
        string plistPath = Path.Combine(report.summary.outputPath, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        plist.root.SetString("NSCameraUsageDescription", "Festive Spaces uses the camera to scan your room and place decorations in AR.");
        plist.root.SetString("NSPhotoLibraryAddUsageDescription", "Festive Spaces saves your AR decoration screenshots to Photos.");
        plist.WriteToFile(plistPath);
    }
}
#endif
