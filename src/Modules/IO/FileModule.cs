#define JsPromise
using Hosihikari.ScriptExtension.QuickJS;
using Hosihikari.ScriptExtension.QuickJS.Helper;
using Hosihikari.ScriptExtension.QuickJS.Types;
using Hosihikari.ScriptExtension.QuickJS.Wrapper;
using System.Runtime.InteropServices;
using Hosihikari.ScriptExtension.QuickJS.Extensions.Check;

namespace Hosihikari.ScriptExtension.Modules.IO;

internal static class FileModule
{
    public static void Setup(JsContextWrapper ctx, JsModuleDefWrapper module)
    {
        unsafe
        {
            module.AddExportValue(
                "file",
                _ =>
                {
                    using var obj = ctx.NewObject();
                    #region FileExists
                    obj.DefineFunction(ctx, "exists", 1, &FileExists);
                    [UnmanagedCallersOnly]
                    static JsValue FileExists(
                        JsContext* ctx,
                        JsValue thisObj,
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
                            return Native.JS_ThrowInternalError(ctx, ex);
                        }
                    }
                    #endregion
                    #region FileReadAllText
                    obj.DefineFunction(ctx, "readAllText", 1, &FileReadAllText);

                    [UnmanagedCallersOnly]
                    static JsValue FileReadAllText(
                        JsContext* ctx,
                        JsValue thisObj,
                        int argCount,
                        JsValue* argvIn
                    )
                    {
                        try
                        {
                            var argv = new ReadOnlySpan<JsValue>(argvIn, argCount);
                            argv.InsureArgumentCount(1);
                            argv[0].InsureTypeString(ctx, out var file);

#if JsPromise
                            var (promise, resolve, reject) = JsValueCreateHelper.NewPromise(ctx);
                            JsPromiseHelper.AwaitTask(
                                (nint)ctx,
                                thisObj,
                                (resolve, reject),
                                File.ReadAllTextAsync(file),
                                result => JsValueCreateHelper.NewString(ctx, result)
                            );
                            return promise.Steal();
                            //Native.JS_Call(ctx, resolve, safeThis.Instance,)
#else
                            return JsValueCreateHelper
                                .NewString(ctx, File.ReadAllText(file))
                                .Steal();
#endif
                        }
                        catch (Exception ex)
                        {
                            return Native.JS_ThrowInternalError(ctx, ex);
                        }
                    }
                    #endregion
                    #region FileWriteAllText
                    obj.DefineFunction(ctx, "writeAllText", 1, &FileWriteAllText);
                    [UnmanagedCallersOnly]
                    static JsValue FileWriteAllText(
                        JsContext* ctx,
                        JsValue thisObj,
                        int argCount,
                        JsValue* argvIn
                    )
                    {
                        try
                        {
                            var argv = new ReadOnlySpan<JsValue>(argvIn, argCount);
                            argv.InsureArgumentCount(2);
                            argv[0].InsureTypeString(ctx, out var file);
                            argv[1].InsureTypeString(ctx, out var content);
#if JsPromise
                            var (promise, resolve, reject) = JsValueCreateHelper.NewPromise(ctx);
                            JsPromiseHelper.AwaitTask(
                                (nint)ctx,
                                thisObj,
                                (resolve, reject),
                                File.WriteAllTextAsync(file, content)
                            );
                            return promise.Steal();
#else
                            File.WriteAllText(file, content);
                            return JsValueCreateHelper.Undefined;
#endif
                        }
                        catch (Exception ex)
                        {
                            return Native.JS_ThrowInternalError(ctx, ex);
                        }
                    }
                    #endregion
                    return obj.Steal();
                }
            );
        }
    }
}
