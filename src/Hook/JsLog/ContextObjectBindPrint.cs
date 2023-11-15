using Hosihikari.NativeInterop.Hook.ObjectOriented;
using Hosihikari.ScriptExtension.QuickJS;
using Hosihikari.ScriptExtension.QuickJS.Types;

namespace Hosihikari.ScriptExtension.Hook.JsLog;

internal class ContextObjectBindPrint : HookBase<ContextObjectBindPrint.HookDelegate>
{
    internal unsafe delegate JsValue HookDelegate(void* contextObject, JsContext* ctx);

    public ContextObjectBindPrint()
        : base("_ZN9Scripting7QuickJS13ContextObject10_bindPrintEP9JSContext") { }

    public override unsafe HookDelegate HookedFunc =>
        (contextObject, ctx) =>
        {
            if (Config.Data.EnableLogger)
            {
                using var globalObject = Native.JS_GetGlobalObject(ctx);
                JsLog.Bind(ctx, globalObject.Value);
                return globalObject.Value;
            }
            return Original.Invoke(contextObject, ctx);
        };
}
