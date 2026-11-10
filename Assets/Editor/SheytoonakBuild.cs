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
        //
        // The player settings live in the committed file ProjectSettings/ProjectSettings.asset, so the
        // editor already has them when it opens the project. CI only verifies them; it never changes
        // them at the last moment.
        //
        // Active Input Handling is "Both" (activeInputHandler: 2 in that file): gameplay reads sensors
        // and touch through the Input System package, and the placeholder HUD uses IMGUI (OnGUI), which
        // the Input System package documentation says receives no input events at runtime unless the
        // setting is "Both". Unity has no public API to change this setting, and its documentation says
        // a change needs an editor restart, so it is set only in the committed file.

        public const string ProjectSettingsFile = "ProjectSettings/ProjectSettings.asset";
        public const int MinAndroidApi = 26;

        /// <summary>
        /// Editor convenience: writes the settings that have a public API and saves them. Active Input
        /// Handling is not touched here (see above); <see cref="VerifyProjectSettings"/> reports it.
        /// </summary>
        [MenuItem("Sheytoonak/Apply Player Settings")]
        public static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = "Sheytoonak";
            PlayerSettings.productName = "مدينة الفاجعة"; // the game was renamed on 2026-10-27 (user); repo and identifier keep the old name
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageId);
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            // IL2CPP + ARM64: required by Google Play, and some recent phones no longer run 32-bit apps.
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)MinAndroidApi;
            ApplyIcons();
            AssetDatabase.SaveAssets();
            foreach (var problem in VerifyProjectSettings()) Debug.LogWarning("SHEYTOONAK SETTINGS " + problem);
            Debug.Log("SHEYTOONAK SETTINGS applied and saved: " + PackageId + ", landscape, IL2CPP ARM64");
        }

        /// <summary>
        /// The app icon (user 2026-11-07: «آیکون هم بذار که روی اپ باشه بجای یونیتی»): Assets/Icons/app_icon.png becomes the
        /// default icon and every Android icon kind (legacy, round, adaptive background with no foreground layer).
        /// </summary>
        public static void ApplyIcons()
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Icons/app_icon.png");
            var clear = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Icons/transparent.png");
            if (tex == null) { Debug.LogWarning("SHEYTOONAK ICON Assets/Icons/app_icon.png not found, Unity's default icon stays"); return; }
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new[] { tex });
            var target = NamedBuildTarget.Android;
            foreach (var kind in PlayerSettings.GetSupportedIconKinds(target))
            {
                var icons = PlayerSettings.GetPlatformIcons(target, kind);
                foreach (var icon in icons)
                {
                    var layers = new Texture2D[icon.maxLayerCount];
                    layers[0] = tex;
                    // adaptive icons have two layers; an empty foreground made Unity draw its own logo over the painting
                    // (user 2026-11-07: «ایکونش هم نیاد روی آیکون بازی»), so the foreground is an explicit transparent image
                    for (int i = 1; i < layers.Length; i++) layers[i] = clear != null ? clear : tex;
                    icon.SetTextures(layers);
                }
                PlayerSettings.SetPlatformIcons(target, kind, icons);
            }
            // Unity 6 lets the Personal plan turn the "Made with Unity" splash off (user 2026-11-07: «اسپلش یونیتی نباشه»).
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            Debug.Log("SHEYTOONAK ICON set from Assets/Icons/app_icon.png; Unity splash off");
        }

        /// <summary>Returns every mismatch between the current settings and what the build needs (empty = all good).</summary>
        public static List<string> VerifyProjectSettings()
        {
            var problems = new List<string>();
            void Check(bool ok, string what) { if (!ok) problems.Add(what); }

            Check(PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android) == PackageId,
                "Android application identifier is not " + PackageId);
            Check(PlayerSettings.defaultInterfaceOrientation == UIOrientation.LandscapeLeft, "default orientation is not LandscapeLeft");
            Check(PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) == ScriptingImplementation.IL2CPP,
                "Android scripting backend is not IL2CPP");
            Check(PlayerSettings.Android.targetArchitectures == AndroidArchitecture.ARM64, "Android architecture is not ARM64 only");
            Check((int)PlayerSettings.Android.minSdkVersion == MinAndroidApi, "Android minimum API level is not " + MinAndroidApi);

            // Input handling, checked two ways: the scripting defines this editor compiled with (the
            // Input System documentation names ENABLE_INPUT_SYSTEM and ENABLE_LEGACY_INPUT_MANAGER), and
            // the value saved in the committed settings file.
#if !ENABLE_INPUT_SYSTEM
            problems.Add("ENABLE_INPUT_SYSTEM is not defined: Active Input Handling must be 'Both'");
#endif
#if !ENABLE_LEGACY_INPUT_MANAGER
            problems.Add("ENABLE_LEGACY_INPUT_MANAGER is not defined: Active Input Handling must be 'Both' (the IMGUI HUD needs it)");
#endif
            string saved = File.Exists(ProjectSettingsFile) ? File.ReadAllText(ProjectSettingsFile) : "";
            Check(saved.Contains("\n  activeInputHandler: 2\n") || saved.Contains("\r\n  activeInputHandler: 2\r\n"),
                ProjectSettingsFile + " does not contain 'activeInputHandler: 2' (Both)");
            return problems;
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
        public static void BuildAndroidMenu()
        {
            var problems = VerifyProjectSettings();
            foreach (var problem in problems) Debug.LogError("SHEYTOONAK SETTINGS " + problem);
            if (problems.Count == 0) BuildAndroid(DefaultApkPath, 0);
        }

        /// <summary>
        /// Entry point for GameCI (unity-builder buildMethod). GameCI starts Unity with -batchmode -quit
        /// and passes -customBuildPath and -androidVersionCode (checked in unity-builder v4.8.2). Unity's
        /// exit code is what fails the CI step, and with -quit a method that simply returns may let Unity
        /// exit with 0 even after a failed build, so every path here ends in an explicit
        /// EditorApplication.Exit: 0 only when the player build succeeded and the APK exists.
        /// </summary>
        public static void BuildAndroidCI()
        {
            int code = 1;
            try
            {
                var problems = VerifyProjectSettings();
                if (problems.Count > 0)
                {
                    foreach (var problem in problems) Debug.LogError("SHEYTOONAK SETTINGS " + problem);
                    Debug.LogError("SHEYTOONAK BUILD Failed: project settings do not match (see lines above)");
                }
                else
                {
                    ApplyIcons(); // the painted icon, not Unity's (ProjectSettings are not committed with icons; set at build time)
                    var args = ParseArgs();
                    string path = args.TryGetValue("customBuildPath", out var p) && !string.IsNullOrEmpty(p) ? p : DefaultApkPath;
                    if (!path.EndsWith(".apk", StringComparison.OrdinalIgnoreCase)) path = Path.Combine(path, "Sheytoonak.apk");
                    int versionCode = 0;
                    if (args.TryGetValue("androidVersionCode", out var vc)) int.TryParse(vc, out versionCode);
                    UseFixedKeystore(args);
                    code = BuildAndroid(path, versionCode) ? 0 : 1;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.LogError("SHEYTOONAK BUILD Failed with an exception");
                code = 1;
            }
            EditorApplication.Exit(code);
        }

        /// <summary>
        /// The fixed signing key from the repository secrets (GameCI writes the keystore file next to the project and
        /// passes the names and passwords as arguments). User 2026-11-02: every build signed with a fresh debug key
        /// would not install over the previous one ("package conflicts"). No key = Unity's debug key, as before.
        /// ⚠️ Passwords are never logged.
        /// </summary>
        static void UseFixedKeystore(Dictionary<string, string> args)
        {
            args.TryGetValue("androidKeystoreName", out var name);
            args.TryGetValue("androidKeystorePass", out var pass);
            args.TryGetValue("androidKeyaliasName", out var alias);
            args.TryGetValue("androidKeyaliasPass", out var aliasPass);
            string file = string.IsNullOrEmpty(name) ? null : Path.GetFullPath(name);
            if (file == null || !File.Exists(file) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(alias))
            {
                PlayerSettings.Android.useCustomKeystore = false;
                Debug.Log("SHEYTOONAK SIGNING debug key (no fixed keystore given)");
                return;
            }
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = file;
            PlayerSettings.Android.keystorePass = pass;
            PlayerSettings.Android.keyaliasName = alias;
            PlayerSettings.Android.keyaliasPass = string.IsNullOrEmpty(aliasPass) ? pass : aliasPass;
            Debug.Log("SHEYTOONAK SIGNING fixed keystore, alias " + alias);
        }

        const string ParkedFolder = "Assets/CIParkedOriginals"; // outside Resources: the PNGs wait here during the build

        static void EnsureAssetFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureAssetFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        static void PrecompressResources()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            int done = 0, failed = 0; long bytes = 0;
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources" });
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".png") && !path.EndsWith(".jpg")) continue;
                try
                {
                    var src = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    if (src == null) { failed++; continue; }
                    bool hero = path.Contains("/Heroes/") && !path.EndsWith("_face.png");
                    bool house = path.Contains("/Art/house_") || path.Contains("/Art/ground_");
                    int cap = hero ? 512 : house ? 2048 : 1024;
                    float k = Mathf.Min(1f, cap / (float)Mathf.Max(src.width, src.height));
                    int w = Mathf.Max(4, Mathf.RoundToInt(src.width * k)), h = Mathf.Max(4, Mathf.RoundToInt(src.height * k));
                    var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                    var prev = RenderTexture.active;
                    Graphics.Blit(src, rt);
                    RenderTexture.active = rt;
                    var copy = new Texture2D(w, h, TextureFormat.RGBA32, true, false);
                    copy.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                    copy.Apply(true, false);
                    RenderTexture.active = prev; RenderTexture.ReleaseTemporary(rt);
                    EditorUtility.CompressTexture(copy, house ? TextureFormat.ASTC_5x5 : TextureFormat.ASTC_6x6, 50);
                    copy.wrapMode = src.wrapMode; copy.filterMode = src.filterMode; copy.anisoLevel = src.anisoLevel;
                    copy.name = src.name;
                    string parked = ParkedFolder + "/" + path.Substring("Assets/Resources/".Length);
                    // ⚠️ run 109: a folder made with Directory.CreateDirectory has no GUID yet and MoveAsset refuses it
                    // («Could not find parent directory GUID»); the asset database must create the folders itself.
                    EnsureAssetFolder(Path.GetDirectoryName(parked).Replace('\\', '/'));
                    string err = AssetDatabase.MoveAsset(path, parked);
                    if (!string.IsNullOrEmpty(err)) { Debug.LogWarning("SHEYTOONAK PRECOMPRESS move failed " + path + ": " + err); failed++; continue; }
                    string assetPath = Path.ChangeExtension(path, ".asset");
                    AssetDatabase.CreateAsset(copy, assetPath);
                    bytes += copy.GetRawTextureData<byte>().Length;
                    done++;
                }
                catch (System.Exception e) { Debug.LogWarning("SHEYTOONAK PRECOMPRESS failed " + path + ": " + e.GetType().Name + " " + e.Message); failed++; }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("SHEYTOONAK PRECOMPRESS " + done + " textures to ASTC (" + (bytes / 1048576f).ToString("0.0") + " MB), " + failed + " failed, in " + sw.Elapsed.TotalSeconds.ToString("0") + " s");
            var probe = Resources.Load<Texture2D>("Art/house_11");
            Debug.Log("SHEYTOONAK PRECOMPRESS probe Art/house_11 -> " + (probe != null ? probe.format + " " + probe.width + "x" + probe.height + " mips " + probe.mipmapCount : "null"));
        }

        /// <summary>Builds the APK. Returns true only if Unity reports success and the file exists.</summary>
        static bool BuildAndroid(string apkPath, int versionCode)
        {
            if (versionCode > 0)
            {
                PlayerSettings.Android.bundleVersionCode = versionCode;
                AssetDatabase.SaveAssets(); // saved before the build starts, not only held in memory
            }
            CreatePrototypeScene();

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android &&
                !EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                Debug.LogError("SHEYTOONAK BUILD Failed: could not switch to the Android build target (is Android Build Support installed?)");
                return false;
            }
            EditorUserBuildSettings.buildAppBundle = false; // an .apk you can install directly on the phone
            // ASTC for every texture without its own override (2026-11-05: the textures went in raw, see TextureCompression)
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
            // ⚠️ run 82: target Android, override ASTC_5x5 written in the meta, and still RGBA32, each import 0.09 s (no
            // compression ran at all). The editor preference "Compress Textures on Import" is off in the batch editor, so
            // it is switched on here and every texture under Resources is imported again before the build.
            Debug.Log("SHEYTOONAK TEXTURE compress-on-import was " + EditorPrefs.GetBool("kCompressTexturesOnImport", true));
            EditorPrefs.SetBool("kCompressTexturesOnImport", true);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            int reimported = 0;
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources" }))
                {
                    AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guid), ImportAssetOptions.ForceUpdate);
                    reimported++;
                }
            }
            finally { AssetDatabase.StopAssetEditing(); }
            Debug.Log("SHEYTOONAK TEXTURE reimported " + reimported + " in " + sw.Elapsed.TotalSeconds.ToString("0") + " s");
            foreach (var probe in new[] { "Assets/Resources/Art/house_11.png", "Assets/Resources/Art/Heroes/zaatar_idle.png" })
            {
                var t = AssetDatabase.LoadAssetAtPath<Texture2D>(probe);
                var imp = AssetImporter.GetAtPath(probe) as TextureImporter;
                var ps = imp != null ? imp.GetPlatformTextureSettings("Android") : null;
                if (t != null) Debug.Log("SHEYTOONAK TEXTURE " + probe + ": " + t.format + " " + t.width + "x" + t.height + " mips " + t.mipmapCount +
                                         ", target " + EditorUserBuildSettings.activeBuildTarget + ", android override " + (ps != null ? ps.overridden + " " + ps.format : "?"));
            }

            // ⚠️ runs 81-106: still RGBA32. Is the encoder itself alive in this batch editor? One texture is compressed in
            // memory here (asset untouched) and timed; the command line and the graphics device are printed with it.
            try
            {
                var args = string.Join(" ", System.Environment.GetCommandLineArgs());
                Debug.Log("SHEYTOONAK TEXTURE cmdline nographics=" + args.Contains("-nographics") + " batchmode=" + args.Contains("-batchmode") +
                          " gfx=" + SystemInfo.graphicsDeviceType + " astc6x6 supported=" + SystemInfo.SupportsTextureFormat(TextureFormat.ASTC_6x6));
                var src = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Art/house_11.png");
                var imp2 = AssetImporter.GetAtPath("Assets/Resources/Art/house_11.png") as TextureImporter;
                if (src != null && imp2 != null)
                {
                    Debug.Log("SHEYTOONAK TEXTURE house_11 automatic format for Android = " + imp2.GetAutomaticFormat("Android") +
                              ", importer textureCompression=" + imp2.textureCompression + ", isReadable=" + src.isReadable);
                    var copy = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
                    var rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
                    Graphics.Blit(src, rt); var prev = RenderTexture.active; RenderTexture.active = rt;
                    copy.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0); copy.Apply();
                    RenderTexture.active = prev; RenderTexture.ReleaseTemporary(rt);
                    var sw2 = System.Diagnostics.Stopwatch.StartNew();
                    EditorUtility.CompressTexture(copy, TextureFormat.ASTC_6x6, 50);
                    Debug.Log("SHEYTOONAK TEXTURE CompressTexture(ASTC_6x6) -> " + copy.format + " in " + sw2.Elapsed.TotalSeconds.ToString("0.00") + " s");
                    UnityEngine.Object.DestroyImmediate(copy);
                }
            }
            catch (System.Exception e) { Debug.Log("SHEYTOONAK TEXTURE probe failed: " + e.GetType().Name + " " + e.Message); }

            // ⚠️ run 108 proved it: the ASTC encoder works in this editor (CompressTexture 1.67 s, OpenGLCore, ASTC supported)
            // but the importer never calls it (270 imports in 8 s, RGBA32, build 1.2 GB). So the build compresses the
            // pictures itself: every PNG under Resources becomes a Texture2D asset already in ASTC (5x5 for the paintings and
            // the ground, 6x6 for the rest) at the capped size, and the PNG is moved out of Resources for the build. Nothing
            // of this is committed; the CI checkout is thrown away.
            if (System.Environment.GetEnvironmentVariable("SHEYTOONAK_NO_PRECOMPRESS") != "1") PrecompressResources();

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
            bool exists = File.Exists(apkPath) && new FileInfo(apkPath).Length > 0;
            bool ok = s.result == BuildResult.Succeeded && exists;
            string line = $"SHEYTOONAK BUILD {(ok ? "Succeeded" : "Failed")}: result {s.result}, apk {(exists ? "present" : "missing")} at {apkPath}, " +
                          $"{s.totalSize / (1024 * 1024)} MB, {s.totalErrors} error(s), {s.totalTime}";
            if (ok) Debug.Log(line); else Debug.LogError(line);
            return ok;
        }

        /// <summary>
        /// GameCI entry point for the Linux player used only for automatic screenshots (see
        /// AutoScreenshot). Mono, not IL2CPP: the base editor image has no Linux IL2CPP toolchain.
        /// </summary>
        public static void BuildLinuxCI()
        {
            int code = 1;
            try
            {
                var args = ParseArgs();
                string path = args.TryGetValue("customBuildPath", out var p) && !string.IsNullOrEmpty(p) ? p : "Builds/Linux/Sheytoonak";
                if (Directory.Exists(path) || !Path.HasExtension(path)) path = Path.Combine(path, "Sheytoonak.x86_64");
                CreatePrototypeScene();
                if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneLinux64 &&
                    !EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64))
                    throw new Exception("could not switch to StandaloneLinux64");
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath },
                    locationPathName = path,
                    target = BuildTarget.StandaloneLinux64,
                    targetGroup = BuildTargetGroup.Standalone,
                    options = BuildOptions.None
                });
                bool ok = report.summary.result == BuildResult.Succeeded && File.Exists(path);
                string line = $"SHEYTOONAK LINUX BUILD {(ok ? "Succeeded" : "Failed")}: {report.summary.result}, {path}, {report.summary.totalErrors} error(s)";
                if (ok) Debug.Log(line); else Debug.LogError(line);
                code = ok ? 0 : 1;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.LogError("SHEYTOONAK LINUX BUILD Failed with an exception");
            }
            EditorApplication.Exit(code);
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
