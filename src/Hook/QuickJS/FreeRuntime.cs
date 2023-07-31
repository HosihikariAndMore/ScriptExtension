using Hosihikari.NativeInterop.Hook.ObjectOriented;
using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.Hook.QuickJS;

internal class FreeRuntime : HookBase<FreeRuntime.HookDelegate>
{
    internal unsafe delegate void HookDelegate(JsRuntime* rt);

    public FreeRuntime()
        : base("JS_FreeRuntime") { }

    public override unsafe HookDelegate HookedFunc =>
        rt =>
        {
            Loader.Manager.FreeRuntime(rt);
            Original.Invoke(rt);
        };
}
