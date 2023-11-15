using Hosihikari.NativeInterop.Hook.ObjectOriented;
using Hosihikari.ScriptExtension.QuickJS.Types;

namespace Hosihikari.ScriptExtension.Hook.QuickJS;

internal class FreeContext : HookBase<FreeContext.HookDelegate>
{
    internal unsafe delegate void HookDelegate(JsContext* ctx);

    public FreeContext()
        : base("JS_FreeContext") { }

    public override unsafe HookDelegate HookedFunc =>
        ctx =>
        {
            if (ctx is null)
                return;
            //ref #L2278
            if (--ctx->header.ref_count > 0)
                return;
            Log.Logger.Trace(
                "JS_FreeContext ctx: " + (nint)ctx + " ctx->refCount: " + ctx->header.ref_count
            );
            Loader.Manager.FreeContext(ctx);
            Original.Invoke(ctx);
        };
}
