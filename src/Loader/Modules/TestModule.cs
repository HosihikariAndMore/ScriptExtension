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
            Log.Logger.Debug("测试", arg.ToString(ctx));
            using var ret = JsValueCreateHelper.NewString(ctx, "测试返回字符串", false);
            return ret.Value;
        }
        catch (Exception ex)
        {
            return Native.JS_ThrowError(ctx, ex.Message);
        }
    }
}
