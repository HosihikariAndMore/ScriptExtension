using Hosihikari.Minecraft.Extension;
using Hosihikari.VanillaScript.QuickJS.Exceptions;
using Hosihikari.VanillaScript.QuickJS.Extensions;
using Hosihikari.VanillaScript.QuickJS.Types;
using Hosihikari.VanillaScript.QuickJS.Wrapper;

namespace Hosihikari.VanillaScript.QuickJS.Helper;

public static class JsPromiseHelper
{
    internal static AutoDropJsValue BuildErrorJsValue(
        this JsContextWrapper ctx,
        Exception exception
    )
    {
        unsafe
        {
            var errorObj = JsValueCreateHelper.NewError(ctx.Context);
            /*
                JS_DefinePropertyValue(ctx, obj, JS_ATOM_message,
                               JS_NewString(ctx, buf),
                               JS_PROP_WRITABLE | JS_PROP_CONFIGURABLE);
             */
            errorObj.DefineProperty(
                ctx.Context,
                JsAtom.BuildIn.Message,
                JsValueCreateHelper.NewString(ctx.Context, exception.Message).Steal(),
                JsPropertyFlags.Writable | JsPropertyFlags.Configurable
            );
            errorObj.DefineProperty(
                ctx.Context,
                JsAtom.BuildIn.Stack,
                JsValueCreateHelper.NewString(ctx.Context, exception.StackTrace ?? "").Steal(),
                JsPropertyFlags.Writable | JsPropertyFlags.Configurable
            );
            //todo  JsAtomConst.ToStringFunc define .toString() function for errorObj
            return errorObj;
        }
    }

    public static void AwaitTask(
        nint ctxPtr,
        JsValue thisObj,
        (SafeJsValue resolve, SafeJsValue reject) promise,
        Task tasks
    )
    {
        var safeThis = new SafeJsValue(thisObj, ctxPtr);
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
                            try
                            {
                                using var callResult = Native.JS_Call(
                                    ctx.Context,
                                    promise.resolve.Instance,
                                    safeThis.Instance,
                                    0,
                                    null
                                );
                            }
                            finally
                            {
                                safeThis.FreeThis();
                                promise.resolve.FreeThis();
                                promise.reject.FreeThis();
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LevelTick.PostTick(() =>
                {
                    unsafe
                    {
                        if (JsContextWrapper.TryGet(ctxPtr, out var ctx))
                        {
                            try
                            {
                                using var reasonObj = ctx.BuildErrorJsValue(ex);
                                var reasonValue = reasonObj.Value;
                                using var callResult = Native.JS_Call(
                                    ctx.Context,
                                    promise.reject.Instance,
                                    safeThis.Instance,
                                    1,
                                    &reasonValue
                                );
                            }
                            finally
                            {
                                safeThis.FreeThis();
                                promise.resolve.FreeThis();
                                promise.reject.FreeThis();
                            }
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
    public static void AwaitTask<T>(
        nint ctxPtr,
        JsValue thisObj,
        (SafeJsValue resolve, SafeJsValue reject) promise,
        Task<T> tasks,
        Func<T, AutoDropJsValue> fetchResult
    )
    {
        var safeThis = new SafeJsValue(thisObj, ctxPtr);
        Task.Run(async () =>
        {
            try
            {
                var result = await tasks.ConfigureAwait(false);
                LevelTick.PostTick(() =>
                {
                    using var resultObj = fetchResult(result);
                    var resultValue = resultObj.Value;
                    unsafe
                    {
                        if (JsContextWrapper.TryGet(ctxPtr, out var ctx))
                        {
                            try
                            {
                                using var callResult = Native.JS_Call(
                                    ctx.Context,
                                    promise.resolve.Instance,
                                    safeThis.Instance,
                                    1,
                                    &resultValue
                                );
                            }
                            finally
                            {
                                safeThis.FreeThis();
                                promise.resolve.FreeThis();
                                promise.reject.FreeThis();
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LevelTick.PostTick(() =>
                {
                    unsafe
                    {
                        if (JsContextWrapper.TryGet(ctxPtr, out var ctx))
                        {
                            try
                            {
                                using var reasonObj = ctx.BuildErrorJsValue(ex);
                                var reasonValue = reasonObj.Value;
                                using var callResult = Native.JS_Call(
                                    ctx.Context,
                                    promise.reject.Instance,
                                    safeThis.Instance,
                                    1,
                                    &reasonValue
                                );
                            }
                            finally
                            {
                                safeThis.FreeThis();
                                promise.resolve.FreeThis();
                                promise.reject.FreeThis();
                            }
                        }
                    }
                });
            }
        });
    }

    public static Task<object?> ConvertPromiseToTask(JsContextWrapper ctx, JsValue promiseInstance)
    {
        var tcs = new TaskCompletionSource<object?>();
        unsafe
        {
            ConvertPromiseToTask(
                ctx,
                promiseInstance,
                (_, _, argv) =>
                {
                    tcs.SetResult(argv.Length == 0 ? null : argv[0].ToClrObject(ctx));
                    return JsValueCreateHelper.Undefined;
                },
                (_, _, argv) =>
                {
                    tcs.SetException(
                        argv.Length == 0
                            ? new Exception("unknown js error")
                            : new QuickJsException(argv[0], ctx.Context)
                    );
                    return JsValueCreateHelper.Undefined;
                }
            );
        }
        return tcs.Task;
    }

    public static unsafe void ConvertPromiseToTask(
        JsContextWrapper ctx,
        JsValue promiseInstance,
        JsNativeFunctionDelegate onResolve,
        JsNativeFunctionDelegate onReject
    )
    {
        unsafe
        {
            using var resolve = ctx.NewJsFunctionObject(onResolve);
            using var reject = ctx.NewJsFunctionObject(onReject);
            fixed (JsValue* argv = new[] { resolve.Value, reject.Value })
            {
                using var result = Native.JS_Invoke(
                    ctx.Context,
                    promiseInstance,
                    JsAtom.BuildIn.Then,
                    2,
                    argv
                );
            }
        }
    }
}
