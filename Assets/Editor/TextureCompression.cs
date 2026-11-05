using UnityEditor;
using UnityEngine;

namespace Sheytoonak.EditorTools
{
    /// <summary>
    /// User (2026-11-05): «همین الان فشرده‌سازی رو شروع کن». The build report of run 78 showed every picture stored raw:
    /// a 1536x1024 house took 8.0 MB (RGBA32 + mipmaps) and textures were 439 MB of a 245 MB APK. Every texture under
    /// Assets/Resources now gets an explicit Android override: ASTC 6x6 (about 3.6 bits per pixel instead of 32), so
    /// the build no longer depends on what the project-wide default happens to fall back to.
    /// Raising Version makes Unity re-import every texture once (also on CI with a cached Library).
    /// </summary>
    public class TextureCompression : AssetPostprocessor
    {
        const uint Version = 2;
        public override uint GetVersion() => Version;

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Resources/")) return;
            var imp = (TextureImporter)assetImporter;
            var s = imp.GetPlatformTextureSettings("Android");
            s.overridden = true;
            // ⚠️ runs 81-83: the batch editor on CI never ran the ASTC compressor (each import 0.07 s, RGBA32 even with the
            // override and Compress on Import on). Size is honoured anyway, so pictures are capped at what the phone shows:
            // a hero stands ~300 px tall on screen (sheet 1024x1492 -> 512), props/UI 1024, house paintings stay 2048.
            bool hero = assetPath.Contains("/Heroes/") && !assetPath.EndsWith("_face.png");
            bool house = assetPath.Contains("/Art/house_") || assetPath.Contains("/Art/ground_");
            s.maxTextureSize = hero ? 512 : house ? 2048 : 1024;
            // the house paintings are seen large and fill the screen: a finer block keeps their lines clean
            bool painting = assetPath.Contains("/Art/house_") || assetPath.Contains("/Art/wall_") || assetPath.Contains("/Art/ground_");
            s.format = painting ? TextureImporterFormat.ASTC_5x5 : TextureImporterFormat.ASTC_6x6;
            s.compressionQuality = 50;
            imp.SetPlatformTextureSettings(s);
        }
    }
}
