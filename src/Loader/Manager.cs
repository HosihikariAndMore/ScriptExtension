using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.Loader;

public static partial class Manager
{
    private static readonly List<nint> _loadedScriptsContext = new();

    internal static unsafe void AddContext(JsContext* ctx, bool isLoaderContext)
    {
        if (!_loadedScriptsContext.Contains((nint)ctx))
        {
            _loadedScriptsContext.Add((nint)ctx);
            SetupContext(ctx);
            if (isLoaderContext)
            {
                LoadAllScripts(ctx);
            }
        }
    }

    //todo impl
    internal static unsafe void FreeContext(JsContext* ctx)
    {
        _loadedScriptsContext.Remove((nint)ctx);
    }
}
