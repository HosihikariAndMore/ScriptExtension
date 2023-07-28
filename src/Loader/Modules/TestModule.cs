using Hosihikari.VanillaScript.QuickJS.Extensions;
using Hosihikari.VanillaScript.QuickJS.Helper;
using Hosihikari.VanillaScript.QuickJS.Types;
using System.Runtime.InteropServices;

namespace Hosihikari.VanillaScript.Loader.Modules;

internal static unsafe class TestModule
{
    public static void Bind(JsContext* ctx, JsValue instance)
    {
        instance.DefineFunction(ctx, "test", &test, 1);
    }

    [UnmanagedCallersOnly]
    private static unsafe JsValue test(JsContext* ctx, JsValue val, int argCount, JsValue* argvIn)
    {
        var argv = new ReadOnlySpan<JsValue>(argvIn, argCount);
        var arg = argv[0];
        Log.Logger.Debug("测试", arg.ToString(ctx));
        return JsValueCreateHelper.True;
    }
}
