using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.Loader;

internal static partial class Manager
{
    private static readonly List<nint> _loadedScriptsContext = new();

    internal static unsafe void AddContext(JsContext* ctx)
    {
        if (!_loadedScriptsContext.Contains((nint)ctx))
        {
            _loadedScriptsContext.Add((nint)ctx);
            SetupContext(ctx);
        }
    }

    //todo impl
    internal static unsafe void FreeContext(JsContext* ctx)
    {
        _loadedScriptsContext.Remove((nint)ctx);
    }
}
