#define JsPromise
using System.Runtime.InteropServices;
using Hosihikari.VanillaScript.QuickJS;
using Hosihikari.VanillaScript.QuickJS.Extensions.Check;
using Hosihikari.VanillaScript.QuickJS.Helper;
using Hosihikari.VanillaScript.QuickJS.Types;
using Hosihikari.VanillaScript.QuickJS.Wrapper;

namespace Hosihikari.VanillaScript.Modules.IO;

internal class DirectoryModule
{
    public static void Setup(JsContextWrapper ctx, JsModuleDefWrapper module)
    {
        unsafe
        {
            module.AddExport(
                "directory",
                _ =>
                {
                    using var obj = ctx.NewObject();
                    #region FileExists
                    obj.DefineFunction(ctx, "exists", 1, &DirectoryExists);
                    [UnmanagedCallersOnly]
                    static JsValue DirectoryExists(
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
                            argv[0].InsureTypeString(ctx, out var directory);
                            return JsValueCreateHelper.NewBool(Directory.Exists(directory));
                        }
                        catch (Exception ex)
                        {
                            return Native.JS_ThrowInternalError(ctx, ex);
                        }
                    }
                    #endregion
                    #region GetFiles
                    obj.DefineFunction(ctx, "getFiles", 1, &GetFiles);

                    [UnmanagedCallersOnly]
                    static JsValue GetFiles(
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
                            argv[0].InsureTypeString(ctx, out var directory);

#if JsPromise
                            var (promise, resolve, reject) = JsValueCreateHelper.NewPromise(ctx);
                            JsPromiseHelper.AwaitTask(
                                (nint)ctx,
                                thisObj,
                                (resolve, reject),
                                Task.Run(() => Directory.GetFiles(directory)),
                                result => JsValueCreateHelper.NewArray(ctx, result).Steal()
                            );
                            return promise.Steal();
                            //Native.JS_Call(ctx, resolve, safeThis.Instance,)
#else

                            return JsValueCreateHelper
                                .NewArray(ctx, Directory.GetFiles(directory))
                                .Steal();
#endif
                        }
                        catch (Exception ex)
                        {
                            return Native.JS_ThrowInternalError(ctx, ex);
                        }
                    }
                    #endregion
                    #region GetDirectories
                    obj.DefineFunction(ctx, "getDirectories", 1, &GetDirectories);

                    [UnmanagedCallersOnly]
                    static JsValue GetDirectories(
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
                            argv[0].InsureTypeString(ctx, out var directory);

#if JsPromise
                            var (promise, resolve, reject) = JsValueCreateHelper.NewPromise(ctx);
                            JsPromiseHelper.AwaitTask(
                                (nint)ctx,
                                thisObj,
                                (resolve, reject),
                                Task.Run(() => Directory.GetDirectories(directory)),
                                result => JsValueCreateHelper.NewArray(ctx, result).Steal()
                            );
                            return promise.Steal();
                            //Native.JS_Call(ctx, resolve, safeThis.Instance,)
#else
                            return JsValueCreateHelper
                                .NewArray(ctx, Directory.GetDirectories(directory))
                                .Steal();
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
