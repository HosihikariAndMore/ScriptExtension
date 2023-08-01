using Hosihikari.VanillaScript.QuickJS.Wrapper;

namespace Hosihikari.VanillaScript.Loader;

public static partial class Manager
{
    public static event Action<JsRuntimeWrapper>? OnRuntimeCreated;

    internal static void SetupRuntime(JsRuntimeWrapper rt)
    {
        RegisterClass(rt);
        OnRuntimeCreated?.Invoke(rt);
    }

    private static void RegisterClass(JsRuntimeWrapper rt)
    {
        // rt.NewClass();
    }
}
