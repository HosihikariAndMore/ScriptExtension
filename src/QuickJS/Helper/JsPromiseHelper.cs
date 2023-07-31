using Hosihikari.Minecraft.Extension;
using Hosihikari.VanillaScript.QuickJS.Extensions;
using Hosihikari.VanillaScript.QuickJS.Types;
using Hosihikari.VanillaScript.QuickJS.Wrapper;

namespace Hosihikari.VanillaScript.QuickJS.Helper;

public static class JsPromiseHelper
{
    internal static JsValue BuildErrorJsValue(this JsContextWrapper ctx, Exception exception)
    {
        unsafe
        {
            var errorObj = JsValueCreateHelper.NewError(ctx.Context).Steal();
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
                            var reasonObj = ctx.BuildErrorJsValue(ex);
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
    public static void AwaitTask<T>(
        nint ctxPtr,
        JsValue thisObj,
        (SafeJsValue resolve, SafeJsValue reject) promise,
        Task<T> tasks,
        Func<T, JsValue> fetchResult
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
                            safeThis.FreeThis();
                            promise.resolve.FreeThis();
                            promise.reject.FreeThis();
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
                            var reasonObj = ctx.BuildErrorJsValue(ex);
                            Native.JS_Call(
                                ctx.Context,
                                promise.reject.Instance,
                                safeThis.Instance,
                                1,
                                &reasonObj
                            );
                            safeThis.FreeThis();
                            promise.resolve.FreeThis();
                            promise.reject.FreeThis();
                        }
                    }
                });
            }
        });
    }
}
