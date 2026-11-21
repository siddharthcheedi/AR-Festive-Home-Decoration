# Festive Spaces (Unity 6 AR MVP)

This is a Unity 6 cross-platform AR Foundation project for Android (ARCore) and iPhone/iPad (ARKit). It contains a runtime UI, three festival catalogues, generated decoration prefabs, horizontal plane visualization, AR raycast placement, touch manipulation, and screenshot capture. The generated primitive models are placeholders intended to be replaced by imported meshes.

## First launch

1. Open this folder in Unity **6000.0.32f1** or a compatible Unity 6 editor and let Package Manager resolve the manifest.
2. Wait for scripts to compile, then choose **Festive Spaces → Create Starter AR Scene**. This creates the launch scene, plane prefab, and all 21 decoration prefabs, and adds the scene to Build Settings.
3. In **File → Build Profiles**, choose the target device. Install the corresponding Unity platform module in Unity Hub: Android Build Support (SDK/NDK and OpenJDK) or iOS Build Support.
4. In **Edit → Project Settings → XR Plug-in Management**, enable **ARCore** for Android or **ARKit** for iOS. Unity requires an XR provider enabled for each build target. See [Unity XR project configuration](https://docs.unity3d.com/Manual/configuring-project-for-xr.html).
5. Set the app's package/bundle identifier. For Android, choose **ARM64** and build to an ARCore-supported phone. For iPhone, build an Xcode project, open it on a Mac with Xcode, set your Apple development team for signing, then run it on an ARKit-supported iPhone. App Store/TestFlight distribution requires Apple's signing setup.

## Play flow

- Start Decorating → choose Diwali, Christmas, or Halloween → select a decoration → Enter AR.
- Move the phone slowly until a translucent plane appears. Tap a plane to place multiple copies.
- Tap an existing decoration to select it. Drag with one finger to move; twist or pinch with two fingers to rotate and resize. The controls also offer rotate, resize, delete, and Done.
- Done keeps the live camera view for final preview. Capture screenshot saves a PNG to Android Pictures/FestiveSpaces or iOS Photos, and also keeps a private app copy. Saved Designs lists captures stored by the app.

## Project layout

- `Assets/Scripts`: navigation, catalogue, AR session, placement, manipulation, and screenshot code.
- `Assets/Editor/FestiveSpacesSetup.cs`: one-click scene and prefab generator.
- `Assets/Plugins/iOS/FestivePhotoSaver.mm` and `Assets/Editor/IOSBuildSettings.cs`: iOS Photos export and privacy strings.
- `Assets/Prefabs`, `Assets/Resources/Decorations`, `Assets/Materials`, `Assets/Models`, `Assets/Textures`, `Assets/UI`, `Assets/Scenes`: generated or ready-to-fill asset folders.
- `Packages/manifest.json`: AR Foundation, ARCore, ARKit, XR Management, and UGUI dependencies.

To use real art, replace the generated prefabs in `Assets/Resources/Decorations` with models that keep a root collider and `DecorationInstance` component, or update the catalogue entries in `FestivalCatalog.cs` to reference your preferred prefab assets.

## Publish the project to GitHub

The `.gitignore` excludes Unity's generated `Library`, build, and IDE files. From this project folder, after creating an empty GitHub repository, run:

```sh
git init
git add .gitignore README.md Assets Packages ProjectSettings
git commit -m "Initial Festive Spaces AR MVP"
git branch -M main
git remote add origin https://github.com/siddharthcheedi/AR-Festive-Home-Decoration.git
git push -u origin main
```
