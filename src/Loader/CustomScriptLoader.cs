using Hosihikari.VanillaScript.QuickJS;
using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.Loader;

internal static partial class Manager
{
    internal static unsafe void LoadAllScripts(JsContext* ctx)
    {
        var pluginsDir = Path.GetFullPath("plugins");
        foreach (
            var js in Directory.EnumerateFiles(pluginsDir, "*.js", SearchOption.AllDirectories)
        )
        {
            try
            {
                var bytes = File.ReadAllText(js);
                _ = Native.JS_Eval(ctx, js, bytes);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(nameof(LoadAllScripts), ex);
            }
        }
    }
}
