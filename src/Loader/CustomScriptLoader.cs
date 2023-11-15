using System.Collections;
using System.Diagnostics;
using Hosihikari.ScriptExtension.QuickJS.Wrapper;

namespace Hosihikari.ScriptExtension.Loader;

public class ScriptLoadRequestEventArgs : EventArgs, IEnumerable<string>
{
    private IEnumerable<string> _scriptsPath = Array.Empty<string>();

    public void AddScript(string path)
    {
        _scriptsPath = _scriptsPath.Append(path);
    }

    public void AddScripts(IEnumerable<string> paths)
    {
        _scriptsPath = _scriptsPath.Concat(paths);
    }

    public IEnumerator<string> GetEnumerator()
    {
        return _scriptsPath.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class ScriptLoadedEventArgs : EventArgs
{
    public ScriptLoadedEventArgs(
        string path,
        string relativePath,
        TimeSpan time,
        AutoDropJsValue ret
    )
    {
        Path = path;
        RelativePath = relativePath;
        Time = time;
        Result = new SafeJsValue(ret);
    }

    public string Path { get; }
    public string RelativePath { get; }
    public TimeSpan Time { get; }
    public SafeJsValue Result { get; }
}

public static partial class Manager
{
    public static event EventHandler<ScriptLoadRequestEventArgs>? ScriptLoadRequest;
    public static event EventHandler<ScriptLoadedEventArgs>? ScriptLoaded;

    internal static unsafe void LoadAllScripts(JsContextWrapper ctx)
    {
        var pluginsDir = Path.GetFullPath("plugins");
        void LoadScript(string path)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                var bytes = File.ReadAllText(path);
                var relativePath = Path.GetRelativePath(pluginsDir, path);
                using var ret = ctx.EvalScript(relativePath, bytes);
                ScriptLoaded?.Invoke(
                    ctx,
                    new ScriptLoadedEventArgs(
                        path,
                        relativePath,
                        TimeSpan.FromTicks(watch.ElapsedTicks),
                        ret
                    )
                );
            }
            catch (Exception ex)
            {
                Log.Logger.Error(
                    nameof(LoadScript) + "('" + Path.GetRelativePath(pluginsDir, path) + "')",
                    ex
                );
            }
        }
        if (ScriptLoadRequest is null)
        {
            foreach (
                var js in Directory.EnumerateFiles(
                    pluginsDir,
                    "*.js",
                    SearchOption.TopDirectoryOnly
                )
            )
            {
                LoadScript(js);
            }
        }
        else
        {
            // load script from custom source
            var e = new ScriptLoadRequestEventArgs();
            ScriptLoadRequest.Invoke(ctx, e);
            foreach (var js in e)
            {
                LoadScript(js);
            }
        }
    }
}
