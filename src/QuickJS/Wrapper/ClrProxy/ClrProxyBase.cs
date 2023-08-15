using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.Json;
using Hosihikari.VanillaScript.QuickJS.Extensions.Check;
using Hosihikari.VanillaScript.QuickJS.Helper;
using Hosihikari.VanillaScript.QuickJS.Types;
using Hosihikari.VanillaScript.QuickJS.Wrapper.ClrProxy.Generic;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper.ClrProxy;

public abstract class ClrProxyBase
{
#if DEBUG
    private static int activeCount = 0;
#endif

    public static T GetFromIntPtr<T>(nint ptr)
    {
        var handle = GCHandle.FromIntPtr(ptr);
        return (T)handle.Target!;
    }

    public static nint PinAndGetIntPtr<T>(T item)
        where T : ClrProxyBase
    {
        var handle = GCHandle.Alloc(item);
#if DEBUG
        activeCount++;
#endif
        return GCHandle.ToIntPtr(handle);
    }

    protected abstract JsValue Invoke(
        JsContextWrapper ctxInstance,
        JsValue contextThis,
        ReadOnlySpan<JsValue> argv,
        JsCallFlag flags
    );

    protected abstract bool GetOwnProperty(
        JsContextWrapper ctxInstance,
        out JsPropertyDescriptor data,
        JsAtom propName
    );

    protected abstract JsPropertyEnum[] GetOwnPropertyNames(JsContextWrapper ctxInstance);

    /// <summary>
    /// if found return true
    /// if not allow to delete, return false
    /// </summary>
    /// <param name="ctxInstance"></param>
    /// <param name="this"></param>
    /// <param name="prop"></param>
    /// <returns> if found return true</returns>
    protected abstract bool DeleteProperty(
        JsContextWrapper ctxInstance,
        JsValue @this,
        JsAtom prop
    );

    protected abstract bool DefineOwnProperty(
        JsContextWrapper ctxInstance,
        JsValue @this,
        JsAtom prop,
        JsValue val,
        JsValue getter,
        JsValue setter,
        JsPropertyFlags flags
    );

    protected unsafe void GcMark(
        JsRuntime* rt,
        JsValue value,
        delegate* unmanaged<JsRuntime*, JsGCObjectHeader*, void> mark
    ) { }

    #region StaticFunction
    #region Exotic
    internal static unsafe Lazy<nint> JsClassExoticMethods =
        new(() =>
        {
            // this will only init once in program run, only to pin an JsClassDef.JsClassExoticMethods object
            // because of direct `class->exotic = class_def->exotic;`in JS_NewClass, the object must be pinned
            var methods = new JsClassDef.JsClassExoticMethods
            {
                GetOwnProperty = &JsClassGetOwnProperty,
                GetOwnPropertyNames = &JsClassGetOwnPropertyNames,
                DefineOwnProperty = &JsClassDefineOwnProperty,
                DeleteProperty = &JsClassDeleteProperty
            };
            return GCHandle.Alloc(methods, GCHandleType.Pinned).AddrOfPinnedObject();
        });

    /// <summary>
    /// Return -1 if exception (can only happen in case of Proxy object), FALSE if the property does not exists, TRUE if it exists.
    /// If 1 is returned, the property descriptor 'desc' is filled if != NULL.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="desc"></param>
    /// <param name="this"></param>
    /// <param name="prop"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly]
    static unsafe int JsClassGetOwnProperty(
        JsContext* ctx,
        JsPropertyDescriptor* desc,
        JsValue @this,
        JsAtom prop
    )
    {
        try
        {
            if (JsGetInstanceReturnTrueIfThrow(ctx, out var ctxInstance, @this, out var instance))
            {
                return -1; //Exception
            }
            if (desc is not null)
            {
                if (instance.GetOwnProperty(ctxInstance, out var descCopy, prop))
                {
                    *desc = descCopy;
                    return 1;
                }
            }
            return 0; //FALSE
        }
        catch (Exception ex)
        {
            Native.JS_ThrowInternalError(ctx, ex);
            return -1;
        }
    }

    /// <summary>
    /// '*ptab' should hold the '*plen' property keys. Return 0 if OK,
    /// -1 if exception. The 'is_enumerable' field is ignored.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="propertyTable"></param>
    /// <param name="propertyLength"></param>
    /// <param name="this"></param>
    /// <returns> Return 0 if OK!!! </returns>
    [UnmanagedCallersOnly]
    static unsafe int JsClassGetOwnPropertyNames(
        JsContext* ctx,
        JsPropertyEnum** propertyTable,
        uint* propertyLength,
        JsValue @this
    )
    {
        Log.Logger.Trace($"call {nameof(JsClassGetOwnPropertyNames)}");
        try
        {
            if (JsGetInstanceReturnTrueIfThrow(ctx, out var ctxInstance, @this, out var instance))
            {
                return -1; //Exception
            }
            var names = instance.GetOwnPropertyNames(ctxInstance);
            var size = (nuint)int.Max(sizeof(JsPropertyEnum) * names.Length, 1);
            //at least one byte even if no element, because the `js_free(ctx, tab_exotic);` is always called by engine
            *propertyTable = (JsPropertyEnum*)Native.js_malloc(ctx, size);
            if (names.Any()) //if no element, the `*propertyTable` is not used
            {
                fixed (JsPropertyEnum* p = names)
                {
                    NativeMemory.Copy(p, *propertyTable, size);
                }
            }
            *propertyLength = (uint)names.Length;
            return 0;
        }
        catch (Exception ex)
        {
            Native.JS_ThrowInternalError(ctx, ex);
            return -1;
        }
    }

    //     /* return < 0 if exception, or TRUE/FALSE */
    //     int(*delete_property)(JSContext * ctx, JSValueConst obj, JSAtom prop);
    [UnmanagedCallersOnly]
    static unsafe int JsClassDeleteProperty(JsContext* ctx, JsValue @this, JsAtom prop)
    {
        Log.Logger.Trace($"call {nameof(JsClassDeleteProperty)}");
        try
        {
            if (JsGetInstanceReturnTrueIfThrow(ctx, out var ctxInstance, @this, out var instance))
            {
                return -1; //Exception
            }
            return instance.DeleteProperty(ctxInstance, @this, prop) ? 1 : 0;
        }
        catch (Exception ex)
        {
            Native.JS_ThrowInternalError(ctx, ex);
            return -1;
        }
    }

    //     /* return < 0 if exception or TRUE/FALSE */
    [UnmanagedCallersOnly]
    static unsafe int JsClassDefineOwnProperty(
        JsContext* ctx,
        JsValue @this,
        JsAtom prop,
        JsValue val,
        JsValue getter,
        JsValue setter,
        JsPropertyFlags flags
    )
    {
        Log.Logger.Trace($"call {nameof(JsClassDefineOwnProperty)}");
        try
        {
            if (JsGetInstanceReturnTrueIfThrow(ctx, out var ctxInstance, @this, out var instance))
            {
                return -1; //Exception
            }
            return instance.DefineOwnProperty(ctxInstance, @this, prop, val, getter, setter, flags)
                ? 1
                : 0;
        }
        catch (Exception ex)
        {
            Native.JS_ThrowInternalError(ctx, ex);
            return -1;
        }
    }

    #endregion
    [UnmanagedCallersOnly]
    internal static unsafe void JsClassGcMark(
        JsRuntime* rt,
        JsValue value,
        delegate* unmanaged<JsRuntime*, JsGCObjectHeader*, void> mark
    )
    {
        Log.Logger.Trace($"call {nameof(JsClassGcMark)}");
        if (TryGetInstance(value, out var obj))
        {
            obj.GcMark(rt, value, mark);
        }
    }

    [UnmanagedCallersOnly]
    internal static unsafe void JsClassFinalizer(JsRuntime* rt, JsValue value)
    {
        var opaque = Native.JS_GetOpaqueWithoutClass(value);
        if (opaque != nint.Zero)
        {
            var handle = GCHandle.FromIntPtr(opaque);
            try
            {
                (handle.Target as IDisposable)?.Dispose();
                handle.Free();
            }
            catch (NullReferenceException e)
            {
                Log.Logger.Warning("JsClassFinalizer " + e);
            }

            if (--activeCount == 0)
            {
                //Log.Logger.Trace("JsClassFinalizer " + opaque + " Current Active: " +  activeCount);
                Log.Logger.Trace("JsClassFinalizer " + opaque + " All Active ClrProxy Cleared.");
            }
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
    public static bool TryGetInstance(JsValue value, [NotNullWhen(true)] out ClrProxyBase? obj)
    {
        var opaque = Native.JS_GetOpaqueWithoutClass(value);
        if (opaque != nint.Zero)
        {
            try
            {
                if (GCHandle.FromIntPtr(opaque).Target is ClrProxyBase instance)
                {
                    obj = instance;
                    return true;
                }
            }
            catch (NullReferenceException e)
            {
                Log.Logger.Warning("TryGetInstance " + e);
            }
        }
        obj = null;
        return false;
    }

    private protected static unsafe bool JsGetInstanceReturnTrueIfThrow(
        JsContext* ctx,
        [NotNullWhen(false)] out JsContextWrapper? ctxInstance,
        JsValue @this,
        [NotNullWhen(false)] out ClrProxyBase? instance
    )
    {
        if (!TryGetInstance(@this, out instance))
        {
            Native.JS_ThrowInternalError(
                ctx,
                new NullReferenceException("JsClassCall: unknown object from js.")
            );
            ctxInstance = null;
            return true;
        }

        if (!JsContextWrapper.TryGet((nint)ctx, out ctxInstance))
        {
            Native.JS_ThrowInternalError(
                ctx,
                new NullReferenceException("JsClassCall: unknown context from js.")
            );
            return true;
        }
        return false;
    }
    #endregion
    #endregion


    [UnmanagedCallersOnly]
    internal static unsafe JsValue ToJson(
        JsContext* ctx,
        JsValue thisObj,
        int argc,
        JsValue* argvPtr
    )
    {
        try
        {
            //var argv = new ReadOnlySpan<JsValue>(argvPtr, argc);
            //argv.InsureArgumentCount(1);
            //var arg = argv[0];
            var val = thisObj;
            if (
                TryGetInstance(val, out var proxy)
                && proxy is ClrInstanceProxy { Instance: var instance }
            )
            {
                var json = JsonSerializer.Serialize(instance);
                using var obj = JsValueCreateHelper.FromJson(ctx, json);
                return obj.Steal();
            }
            else
            {
                using var obj = JsValueCreateHelper.NewObject(ctx); //`{}`
                return obj.Steal();
            }
        }
        catch (Exception ex)
        {
            return Native.JS_ThrowInternalError(ctx, ex);
        }
    }

    [UnmanagedCallersOnly]
    internal static unsafe JsValue ProtoTypeToString(
        JsContext* ctx,
        JsValue thisObj,
        int argc,
        JsValue* argvPtr
    )
    {
        try
        {
            var argv = new ReadOnlySpan<JsValue>(argvPtr, argc);
            if (!TryGetInstance(thisObj, out var data))
            {
                return Native.JS_ThrowInternalError(
                    ctx,
                    new NullReferenceException("could not find thisObj calling toString")
                );
            }
            string? format = null;
            if (argv.InsureArgumentCount(0, 1) == 1)
            {
                argv[0].InsureTypeString(ctx, out format);
            }
            var str = data is IFormattable fmtData
                ? fmtData.ToString(format, null)
                : data.ToString();
            if (str is null)
                return JsValueCreateHelper.Null;
            return JsValueCreateHelper.NewString(ctx, str).Steal();
        }
        catch (Exception ex)
        {
            return Native.JS_ThrowInternalError(ctx, ex);
        }
    }
}
