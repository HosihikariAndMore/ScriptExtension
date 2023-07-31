using Hosihikari.VanillaScript.QuickJS.Wrapper;

namespace Hosihikari.VanillaScript.Loader;

public static partial class Manager
{
    public static event Action<JsRuntimeWrapper>? OnRuntimeCreated;

    internal static void SetupRuntime(JsRuntimeWrapper ctx)
    {
        OnRuntimeCreated?.Invoke(ctx);
    }
}
