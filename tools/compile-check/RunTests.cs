// Minimal NUnit [Test] runner for tools/compile-check (no SetUp/TestCase support; the project tests use neither).
using System; using System.IO; using System.Linq; using System.Reflection;
class Run { static int Main(string[] a) {
  var dir = Path.GetDirectoryName(Path.GetFullPath(a[0]));
  AppDomain.CurrentDomain.AssemblyResolve += (s, e) => { var n = new AssemblyName(e.Name).Name; foreach (var d in a.Skip(1).Append(dir)) { var f = Path.Combine(d, n + ".dll"); if (File.Exists(f)) return Assembly.LoadFrom(f); } return null; };
  var asm = Assembly.LoadFrom(a[0]); int pass = 0, fail = 0;
  foreach (var t in asm.GetTypes().OrderBy(t => t.FullName))
    foreach (var m in t.GetMethods().Where(m => m.GetCustomAttributes().Any(x => x.GetType().Name == "TestAttribute")).OrderBy(m => m.Name)) {
      try { m.Invoke(Activator.CreateInstance(t), null); pass++; Console.WriteLine("PASS " + t.FullName + "." + m.Name); }
      catch (TargetInvocationException e) { fail++; Console.WriteLine("FAIL " + t.FullName + "." + m.Name + ": " + e.InnerException.GetType().Name + " " + e.InnerException.Message.Split('\n')[0]); }
    }
  Console.WriteLine($"total {pass + fail}, passed {pass}, failed {fail}"); return fail == 0 && pass > 0 ? 0 : 1; } }
