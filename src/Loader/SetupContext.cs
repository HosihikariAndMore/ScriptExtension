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
            Modules.IO.Main.Setup(ctx);
        }
        if (Config.Data.BuildInModules.AllowClr)
        {
            Modules.Clr.Main.Setup(ctx);
        }
    }
}
