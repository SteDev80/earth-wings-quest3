using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine.XR.OpenXR;
using EarthWings;

public static class QuestBuild
{
    [MenuItem("Earth Wings/Prepare Quest project")]
    public static void Prepare()
    {
        PlayerSettings.companyName = "Earth Wings"; PlayerSettings.productName = "Earth Wings Prototype";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.earthwings.prototype");
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        QualitySettings.vSyncCount = 0; QualitySettings.antiAliasing = 2;
        var playerSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        playerSettings.FindProperty("activeInputHandler").intValue = 1;
        playerSettings.ApplyModifiedPropertiesWithoutUndo();
        XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
        var perTarget = AssetDatabase.FindAssets("t:XRGeneralSettingsPerBuildTarget");
        if (perTarget.Length == 0)
        {
            var settings = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            System.IO.Directory.CreateDirectory("Assets/XR");
            AssetDatabase.CreateAsset(settings, "Assets/XR/XRGeneralSettingsPerBuildTarget.asset");
            EditorBuildSettings.AddConfigObject("com.unity.xr.management.loader_settings", settings, true);
        }
        var general = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
        if (general == null)
        {
            EditorBuildSettings.TryGetConfigObject<XRGeneralSettingsPerBuildTarget>("com.unity.xr.management.loader_settings", out var settings);
            general = ScriptableObject.CreateInstance<XRGeneralSettings>();
            AssetDatabase.AddObjectToAsset(general, settings);
            settings.SetSettingsForBuildTarget(BuildTargetGroup.Android, general);
        }
        if (general.Manager == null)
        {
            general.Manager = ScriptableObject.CreateInstance<XRManagerSettings>();
            AssetDatabase.AddObjectToAsset(general.Manager, general);
        }
        XRPackageMetadataStore.AssignLoader(general.Manager,"UnityEngine.XR.OpenXR.OpenXRLoader",BuildTargetGroup.Android);
        UnityEditor.XR.OpenXR.Features.FeatureHelpers.RefreshFeatures(BuildTargetGroup.Android);
        var openXR = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
        // Compatibility path: render each eye independently for the map materials.
        openXR.renderMode = OpenXRSettings.RenderMode.MultiPass;
        int enabledCount = 0;
        foreach (var feature in openXR.GetFeatures<UnityEngine.XR.OpenXR.Features.OpenXRFeature>())
            if (feature.GetType().Name.Contains("OculusTouchControllerProfile") || feature.GetType().Name.Contains("MetaQuestFeature"))
            { feature.enabled = true; EditorUtility.SetDirty(feature); enabledCount++; }
            else if (feature.GetType().Name.Contains("HandInteractionProfile"))
            { feature.enabled = true; EditorUtility.SetDirty(feature); }
        if (enabledCount != 2) throw new Exception("Required Quest OpenXR features missing: " + enabledCount);
        EditorUtility.SetDirty(general); EditorUtility.SetDirty(general.Manager); EditorUtility.SetDirty(openXR);
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("Earth Wings", typeof(FlightWorld));
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/Flight.unity");
        EditorBuildSettings.scenes = new[] {new EditorBuildSettingsScene("Assets/Scenes/Flight.unity",true)};
        AssetDatabase.SaveAssets();
        Debug.Log("EARTH_WINGS_PREPARED");
    }
    [MenuItem("Earth Wings/Build Quest APK")]
    public static void Build()
    {
        Prepare();
        FlightChecks.Run();
        System.IO.Directory.CreateDirectory("Builds");
        var report = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,"Builds/EarthWings.apk",BuildTarget.Android,BuildOptions.None);
        if(report.summary.result != BuildResult.Succeeded) throw new Exception("Quest build failed: " + report.summary.result);
    }
}
