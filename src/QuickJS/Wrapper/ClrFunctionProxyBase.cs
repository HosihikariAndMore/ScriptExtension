using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Hosihikari.VanillaScript.QuickJS.Helper;
using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper;

public class ClrFunctionProxyInstance : ClrFunctionProxyBase
{
    public ClrFunctionProxyInstance(JsNativeFunctionDelegate callback)
    {
        Callback = callback;
    }

    public JsNativeFunctionDelegate Callback;

    protected override JsValue Invoke(
        JsContextWrapper ctxInstance,
        JsValue contextThis,
        ReadOnlySpan<JsValue> argv,
        JsCallFlag flags
    )
    {
        //check has JsCallFlag.Constructor
        if (!Enum.IsDefined(typeof(JsCallFlag), flags))
        {
            return ctxInstance.ThrowJsError(
                new NotImplementedException($"operation JsCallFlag {flags} unknown")
            );
        }
        if (((int)flags & (int)JsCallFlag.Constructor) == (int)JsCallFlag.Constructor)
        {
            return ctxInstance.ThrowJsError(
                new InvalidOperationException("invalid constructor call")
            );
        }
        return Callback(ctxInstance, contextThis, argv);
    }
}

public class ClrFunctionProxyDelegate<T> : ClrFunctionProxyBase
    where T : Delegate
{
    public ClrFunctionProxyDelegate() { }

    protected override JsValue Invoke(
        JsContextWrapper ctxInstance,
        JsValue contextThis,
        ReadOnlySpan<JsValue> argv,
        JsCallFlag flags
    )
    { //todo use reflect
        throw new NotImplementedException("todo");
    }
}

public abstract class ClrFunctionProxyBase
{
    protected abstract JsValue Invoke(
        JsContextWrapper ctxInstance,
        JsValue contextThis,
        ReadOnlySpan<JsValue> argv,
        JsCallFlag flags
    );

    [UnmanagedCallersOnly]
    internal static unsafe void JsClassFinalizer(JsRuntime* rt, JsValue value)
    {
        var opaque = Native.JS_GetOpaqueWithoutClass(value);
        if (opaque != IntPtr.Zero)
        {
            var handle = GCHandle.FromIntPtr(opaque);
            (handle.Target as IDisposable)?.Dispose();
            handle.Free();
            Log.Logger.Trace("JsClassFinalizer " + opaque);
        }
        else
        {
            Log.Logger.Error("JsClassFinalizer IntPtr.Zero " + value.Tag);
        }
    }

    [UnmanagedCallersOnly]
    internal static unsafe JsValue JsClassCall(
        JsContext* ctx,
        JsValue @this,
        JsValue contextThis,
        int argc,
        JsValue* argv,
        JsCallFlag flags
    )
    {
        Log.Logger.Trace($"JsClassCall {argc} {flags}");
        try
        {
            if (JsGetInstanceReturnTrueIfThrow(ctx, out var ctxInstance, @this, out var obj))
                return JsValueCreateHelper.Exception;
            return obj.Invoke(
                ctxInstance,
                contextThis,
                new ReadOnlySpan<JsValue>(argv, argc),
                flags
            );
        }
        catch (Exception ex)
        {
            return Native.JS_ThrowInternalError(ctx, ex);
        }
    }

    #region Helper
    private static bool TryGetInstance(
        JsValue value,
        [NotNullWhen(true)] out ClrFunctionProxyBase? obj
    )
    {
        var opaque = Native.JS_GetOpaqueWithoutClass(value);
        if (opaque != IntPtr.Zero)
        {
            if (GCHandle.FromIntPtr(opaque).Target is ClrFunctionProxyBase instance)
            {
                obj = instance;
                return true;
            }
        }
        obj = null;
        return false;
    }

    private static unsafe bool JsGetInstanceReturnTrueIfThrow(
        JsContext* ctx,
        [NotNullWhen(false)] out JsContextWrapper? ctxInstance,
        JsValue @this,
        [NotNullWhen(false)] out ClrFunctionProxyBase? instance
    )
    {
        if (!TryGetInstance(@this, out instance))
        {
            Native.JS_ThrowInternalError(ctx, "JsClassCall: unknown object from js.");
            ctxInstance = null;
            return true;
        }

        if (!JsContextWrapper.TryGet((nint)ctx, out ctxInstance))
        {
            Native.JS_ThrowInternalError(ctx, "JsClassCall: unknown context from js.");
            return true;
        }
        return false;
    }
    #endregion
}
