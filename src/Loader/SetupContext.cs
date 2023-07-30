using Hosihikari.VanillaScript.Loader.Modules.IO;
using Hosihikari.VanillaScript.QuickJS.Wrapper;

namespace Hosihikari.VanillaScript.Loader;

/// <summary>
/// add js module to JsContext
/// </summary>
public static partial class Manager
{
    public static event Action<JsContextWrapper>? OnContextCreated;

    internal static void SetupContext(JsContextWrapper ctx)
    {
        OnContextCreated?.Invoke(ctx);
        if (Config.Data.BuildInModules.FileIo)
        {
            FileModule.Setup(ctx);
        }
    }
}
