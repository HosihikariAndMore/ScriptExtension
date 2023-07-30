#define JsPromise
using Hosihikari.VanillaScript.QuickJS;
using Hosihikari.VanillaScript.QuickJS.Helper;
using Hosihikari.VanillaScript.QuickJS.Types;
using Hosihikari.VanillaScript.QuickJS.Wrapper;
using System.Runtime.InteropServices;
using Hosihikari.Minecraft.Extension;
using Hosihikari.VanillaScript.QuickJS.Extensions.Check;

namespace Hosihikari.VanillaScript.Loader.Modules.IO;

internal static class FileModule
{
    private static void AwaitTask(
        nint ctxPtr,
        JsValue thisObj,
        (SafeJsValue resolve, SafeJsValue reject) promise,
        Task tasks
    )
    {
        var safeThis = new SafeJsValue(thisObj);
        Task.Run(async () =>
        {
            try
            {
                await tasks.ConfigureAwait(false);
                LevelTick.PostTick(() =>
                {
                    unsafe
                    {
                        if (JsContextWrapper.TryGet(ctxPtr, out var ctx))
                        {
                            Native.JS_Call(
                                ctx.Context,
                                promise.resolve.Instance,
                                safeThis.Instance,
                                0,
                                null
                            );
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LevelTick.PostTick(() =>
                {
                    var reason = ex.ToString();
                    unsafe
                    {
                        if (JsContextWrapper.TryGet(ctxPtr, out var ctx))
                        {
                            var reasonObj = JsValueCreateHelper
                                .NewString(ctx.Context, reason)
                                .Steal();
                            Native.JS_Call(
                                ctx.Context,
                                promise.reject.Instance,
                                safeThis.Instance,
                                1,
                                &reasonObj
                            );
                        }
                    }
                });
            }
        });
    }

    /// <summary>
    /// wrap Task to Promise
    /// </summary>
    /// <typeparam name="T"> result type </typeparam>
    /// <param name="ctxPtr"></param>
    /// <param name="thisObj"></param>
    /// <param name="promise"></param>
    /// <param name="tasks"></param>
    /// <param name="fetchResult"></param>
    private static void AwaitTask<T>(
        nint ctxPtr,
        JsValue thisObj,
        (SafeJsValue resolve, SafeJsValue reject) promise,
        Task<T> tasks,
        Func<T, JsValue> fetchResult
    )
    {
        var safeThis = new SafeJsValue(thisObj);
        Task.Run(async () =>
        {
            try
            {
                var result = await tasks.ConfigureAwait(false);
                LevelTick.PostTick(() =>
                {
                    var resultObj = fetchResult(result);
                    unsafe
                    {
                        if (JsContextWrapper.TryGet(ctxPtr, out var ctx))
                        {
                            Native.JS_Call(
                                ctx.Context,
                                promise.resolve.Instance,
                                safeThis.Instance,
                                1,
                                &resultObj
                            );
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LevelTick.PostTick(() =>
                {
                    var reason = ex.ToString();
                    unsafe
                    {
                        if (JsContextWrapper.TryGet(ctxPtr, out var ctx))
                        {
                            var reasonObj = JsValueCreateHelper
                                .NewString(ctx.Context, reason)
                                .Steal();
                            Native.JS_Call(
                                ctx.Context,
                                promise.reject.Instance,
                                safeThis.Instance,
                                1,
                                &reasonObj
                            );
                        }
                    }
                });
            }
        });
    }

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
                            AwaitTask(
                                (nint)ctx,
                                thisObj,
                                (resolve, reject),
                                File.ReadAllTextAsync(file),
                                result => JsValueCreateHelper.NewString(ctx, result).Steal()
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
                            AwaitTask(
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
