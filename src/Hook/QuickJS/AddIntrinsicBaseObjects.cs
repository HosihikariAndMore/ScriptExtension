using Hosihikari.NativeInterop.Hook.ObjectOriented;
using Hosihikari.ScriptExtension.QuickJS.Types;

namespace Hosihikari.ScriptExtension.Hook.QuickJS;

internal class AddIntrinsicBaseObjects : HookBase<AddIntrinsicBaseObjects.HookDelegate>
{
    internal unsafe delegate void HookDelegate(JsContext* ctx, uint enableEval);

    public AddIntrinsicBaseObjects()
        : base("JS_AddIntrinsicBaseObjects") { }

    public override unsafe HookDelegate HookedFunc =>
        (ctx, _) =>
        {
            Original.Invoke(ctx, Config.Data.EnableEval ? 1u : 0u);
        };
}
