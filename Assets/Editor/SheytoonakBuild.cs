using System;
using System.Collections.Generic;
using System.IO;
using Sheytoonak.Prototype;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Sheytoonak.EditorTools
{
    /// <summary>
    /// Reproducible project setup and Android build, runnable from the Editor menu or headless in CI
    /// (GameCI: buildMethod = Sheytoonak.EditorTools.SheytoonakBuild.BuildAndroidCI).
    /// Everything the build needs is created here from code, so no scene or asset has to be authored
    /// by hand in the Editor: player settings, placeholder materials and the prototype scene.
    /// </summary>
    public static class SheytoonakBuild
    {
        public const string GeneratedDir = "Assets/_Generated";
        public const string MaterialsDir = GeneratedDir + "/Materials";
        public const string ScenePath = GeneratedDir + "/Prototype.unity";
        public const string PackageId = "ir.sheytoonak.prototype";
        const string DefaultApkPath = "Builds/Android/Sheytoonak.apk";

        // ------------------------------------------------------------------ player settings

        [MenuItem("Sheytoonak/Apply Player Settings")]
        public static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = "Sheytoonak";
            PlayerSettings.productName = "Sheytoonak";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageId);

            // Portrait only.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

            // IL2CPP + ARM64: required by Google Play, and some recent phones no longer run 32-bit apps.
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;

            // The gameplay code reads sensors and touch through the Input System package.
            SetActiveInputHandler(1); // 0 = old Input Manager, 1 = Input System package, 2 = both

            AssetDatabase.SaveAssets();
            Debug.Log("SHEYTOONAK SETTINGS applied: " + PackageId + ", portrait, IL2CPP ARM64, Input System");
        }

        static void SetActiveInputHandler(int value)
        {
            // There is no public API for this setting; the serialized PlayerSettings property is the
            // documented-by-practice way to set it from a script.
            var playerSettings = Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings");
            var so = new SerializedObject(playerSettings);
            var prop = so.FindProperty("activeInputHandler");
            if (prop == null)
            {
                Debug.LogWarning("SHEYTOONAK SETTINGS: activeInputHandler not found; set Active Input Handling to 'Input System Package' by hand.");
                return;
            }
            prop.intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ------------------------------------------------------------------ prototype scene

        [MenuItem("Sheytoonak/Create Prototype Scene")]
        public static void CreatePrototypeScene()
        {
            EnsureFolder(GeneratedDir);
            EnsureFolder(MaterialsDir);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("Prototype");
            var boot = go.AddComponent<PrototypeBootstrap>();
            // Material assets referenced from the scene make the build include the Standard shader,
            // which Shader.Find alone would not guarantee in a player build.
            boot.brick = Mat("Brick", new Color(0.64f, 0.33f, 0.23f));
            boot.glass = Mat("Glass", new Color(0.45f, 0.65f, 0.85f), 0.9f);
            boot.broken = Mat("BrokenGlass", new Color(0.06f, 0.05f, 0.05f));
            boot.frame = Mat("Frame", new Color(0.85f, 0.80f, 0.70f));
            boot.asphalt = Mat("Asphalt", new Color(0.27f, 0.28f, 0.30f));
            boot.sidewalk = Mat("Sidewalk", new Color(0.78f, 0.75f, 0.69f));
            boot.neighbour = Mat("Neighbour", new Color(0.80f, 0.78f, 0.72f));
            boot.shop = Mat("Shop", new Color(0.95f, 0.78f, 0.45f));
            boot.awning = Mat("Awning", new Color(0.12f, 0.42f, 0.35f));
            boot.jeans = Mat("Jeans", new Color(0.17f, 0.25f, 0.47f));
            boot.shoe = Mat("Shoe", new Color(0.95f, 0.95f, 0.93f));
            boot.ball = Mat("Ball", Color.white, 0.4f);
            boot.metal = Mat("Metal", new Color(0.17f, 0.18f, 0.2f));

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new Exception("Could not save " + ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("SHEYTOONAK SCENE created: " + ScenePath);
        }

        static Material Mat(string name, Color color, float smoothness = 0.1f)
        {
            string path = MaterialsDir + "/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                var shader = Shader.Find("Standard");
                if (shader == null) throw new Exception("Standard shader not found (is a scriptable render pipeline active?)");
                m = new Material(shader);
                AssetDatabase.CreateAsset(m, path);
            }
            m.color = color;
            m.SetFloat("_Glossiness", smoothness);
            EditorUtility.SetDirty(m);
            return m;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        // ------------------------------------------------------------------ build

        [MenuItem("Sheytoonak/Build Android APK")]
        public static void BuildAndroidMenu() => BuildAndroid(DefaultApkPath, 0);

        /// <summary>
        /// Entry point for GameCI (unity-builder buildMethod). GameCI passes -customBuildPath (and
        /// -androidVersionCode); exits Unity with 0 on success, 1 on failure.
        /// </summary>
        public static void BuildAndroidCI()
        {
            try
            {
                var args = ParseArgs();
                string path = args.TryGetValue("customBuildPath", out var p) && !string.IsNullOrEmpty(p) ? p : DefaultApkPath;
                if (!path.EndsWith(".apk", StringComparison.OrdinalIgnoreCase)) path = Path.Combine(path, "Sheytoonak.apk");
                int versionCode = 0;
                if (args.TryGetValue("androidVersionCode", out var vc)) int.TryParse(vc, out versionCode);
                BuildAndroid(path, versionCode);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.LogError("SHEYTOONAK BUILD Failed with an exception");
                EditorApplication.Exit(1);
            }
        }

        static void BuildAndroid(string apkPath, int versionCode)
        {
            ApplyPlayerSettings();
            if (versionCode > 0) PlayerSettings.Android.bundleVersionCode = versionCode;
            CreatePrototypeScene();

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            EditorUserBuildSettings.buildAppBundle = false; // an .apk you can install directly on the phone

            string dir = Path.GetDirectoryName(apkPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = apkPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None
            });
            var s = report.summary;
            Debug.Log($"SHEYTOONAK BUILD {s.result}: {apkPath}, {s.totalSize / (1024 * 1024)} MB, {s.totalErrors} error(s), {s.totalTime}");
            if (Application.isBatchMode) EditorApplication.Exit(s.result == BuildResult.Succeeded ? 0 : 1);
        }

        static Dictionary<string, string> ParseArgs()
        {
            var result = new Dictionary<string, string>();
            string[] a = Environment.GetCommandLineArgs();
            for (int i = 0; i < a.Length; i++)
            {
                if (!a[i].StartsWith("-")) continue;
                string key = a[i].Substring(1);
                string value = i + 1 < a.Length && !a[i + 1].StartsWith("-") ? a[i + 1] : "";
                result[key] = value;
            }
            return result;
        }
    }
}
