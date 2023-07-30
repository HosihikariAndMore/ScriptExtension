using Hosihikari.VanillaScript.QuickJS.Extensions;
using Hosihikari.VanillaScript.QuickJS;
using Hosihikari.VanillaScript.QuickJS.Helper;
using Hosihikari.VanillaScript.QuickJS.Types;
using Hosihikari.VanillaScript.QuickJS.Wrapper;
using System.Runtime.InteropServices;
using Hosihikari.VanillaScript.QuickJS.Extensions.Check;

namespace Hosihikari.VanillaScript.Loader.Modules.IO;

internal static class FileModule
{
    public static void Setup(JsContextWrapper ctx)
    {
        unsafe
        {
            var module = ctx.NewModule(Config.ConfigModules.FileIoModuleName);
            module.AddExport(
                "file",
                _ =>
                {
                    using var obj = ctx.NewObject();
                    obj.DefineFunction(ctx, "exists", 1, &FileExists);
                    return obj.Steal();
                }
            );
        }
    }

    [UnmanagedCallersOnly]
    private static unsafe JsValue FileExists(
        JsContext* ctx,
        JsValue val,
        int argCount,
        JsValue* argvIn
    )
    {
        try
        {
            var argv = new ReadOnlySpan<JsValue>(argvIn, argCount);
            argv.InsureArgumentCount(1);
            argv[0].InsureTypeString(ctx, out var file);
            return JsValueCreateHelper.NewBool(File.Exists(file));
        }
        catch (Exception ex)
        {
            return Native.JS_ThrowInternalError(ctx, ex.Message);
        }
    }
}
