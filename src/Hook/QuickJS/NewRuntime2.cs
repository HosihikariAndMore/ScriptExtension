using Hosihikari.NativeInterop.Hook.ObjectOriented;
using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.Hook.QuickJS;

internal class NewRuntime2 : HookBase<NewRuntime2.HookDelegate>
{
    internal unsafe delegate JsRuntime* HookDelegate(JsMallocFunctions* mf, void* opaque);

    public NewRuntime2()
        : base("JS_NewRuntime2") { }

    public override unsafe HookDelegate HookedFunc =>
        (mf, opaque) =>
        {
            var runtime = Original(mf, opaque);
            Loader.Manager.AddRuntime(runtime);
            return runtime;
        };
}
//JS_FreeRuntime
