using Hosihikari.VanillaScript.QuickJS.Extensions;
using Hosihikari.VanillaScript.QuickJS.Helper;
using Hosihikari.VanillaScript.QuickJS.Types;
using System.Runtime.InteropServices;
using Hosihikari.VanillaScript.QuickJS;

namespace Hosihikari.VanillaScript.Loader.Modules;

internal static unsafe class TestModule
{
    public static void Bind(JsContext* ctx, JsValue instance)
    {
        instance.DefineFunction(ctx, "test", &test, 1);
    }

    [UnmanagedCallersOnly]
    private static JsValue test(JsContext* ctx, JsValue val, int argCount, JsValue* argvIn)
    {
        try
        {
            var argv = new ReadOnlySpan<JsValue>(argvIn, argCount);
            var arg = argv[0];
            Log.Logger.Debug("test" + arg.ToString(ctx));
            using var ret = JsValueCreateHelper.NewString(ctx, "test return");
            return ret.Steal();
        }
        catch (Exception ex)
        {
            return Native.JS_ThrowInternalError(ctx, ex.Message);
        }
    }
}
