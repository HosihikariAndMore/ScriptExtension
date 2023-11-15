using Hosihikari.ScriptExtension.QuickJS.Wrapper;

namespace Hosihikari.ScriptExtension.Loader;

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
