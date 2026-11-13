using System.IO;
using System.Text;
using NUnit.Framework;

namespace Sheytoonak.Tests
{
    /// <summary>
    /// User (2026-11-13): «موسیقی پیروزی چرا هنوز پخش نمی‌شه؟ دوبار دادم بهت که». The converted sting_win/sting_lose
    /// carried the mp3's cover picture as a Theora video stream inside the .ogg, and Unity read nothing from them: the
    /// end of a duel was silent. Every .ogg under Resources/Audio must be audio only.
    /// </summary>
    public class AudioFileTests
    {
        static string AudioDir()
        {
            var d = new DirectoryInfo(Directory.GetCurrentDirectory());
            for (int i = 0; i < 6 && d != null; i++, d = d.Parent)
            {
                var p = Path.Combine(d.FullName, "Assets", "Resources", "Audio");
                if (Directory.Exists(p)) return p;
            }
            return null;
        }

        [Test]
        public void No_ogg_carries_a_video_stream()
        {
            var dir = AudioDir();
            if (dir == null) Assert.Ignore("Assets/Resources/Audio not found from the working directory");
            var theora = Encoding.ASCII.GetBytes("theora");
            foreach (var f in Directory.GetFiles(dir, "*.ogg", SearchOption.AllDirectories))
            {
                var head = File.ReadAllBytes(f);
                int n = System.Math.Min(head.Length, 4096); // the stream headers sit at the very start
                bool found = false;
                for (int i = 0; i + theora.Length <= n && !found; i++)
                {
                    found = true;
                    for (int k = 0; k < theora.Length; k++) if (head[i + k] != theora[k]) { found = false; break; }
                }
                Assert.IsFalse(found, Path.GetFileName(f) + " has a video stream (cover picture): Unity plays nothing");
            }
        }
    }
}
