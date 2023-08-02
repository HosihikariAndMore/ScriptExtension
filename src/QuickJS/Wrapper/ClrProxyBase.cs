using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper;

public abstract class ClrProxyBase : ClrFunctionProxyBase
{
    /// <summary>
    /// default: false
    /// return true to disable all exotic methods
    /// if true, <see cref="GetOwnProperty"/>, <see cref="GetOwnPropertyNames"/>, <see cref="DefineOwnProperty"/>, <see cref="DeleteProperty"/> will not be called
    /// </summary>
    protected virtual bool NoExotic => false;

    protected virtual unsafe void GcMark(
        JsRuntime* rt,
        JsValue value,
        delegate* unmanaged<JsRuntime*, JsGCObjectHeader*, void> mark
    ) { }

    protected override JsValue Invoke(
        JsContextWrapper ctxInstance,
        JsValue contextThis,
        ReadOnlySpan<JsValue> argv,
        JsCallFlag flags
    )
    {
        return ctxInstance.ThrowJsError(
            NoExotic
                ? new NotImplementedException(nameof(Invoke))
                : new InvalidOperationException(nameof(Invoke))
        );
    }

    protected virtual bool GetOwnProperty(
        JsContextWrapper ctxInstance,
        out JsPropertyDescriptor data,
        JsAtom propName
    )
    {
        data = default;
        return false;
    }

    protected virtual JsPropertyEnum[] GetOwnPropertyNames(JsContextWrapper ctxInstance)
    {
        return Array.Empty<JsPropertyEnum>();
    }

    /// <summary>
    /// if found return true
    /// if not allow to delete, return false
    /// </summary>
    /// <param name="ctxInstance"></param>
    /// <param name="this"></param>
    /// <param name="prop"></param>
    /// <returns> if found return true</returns>
    protected virtual bool DeleteProperty(JsContextWrapper ctxInstance, JsValue @this, JsAtom prop)
    {
        return false;
    }

    protected virtual bool DefineOwnProperty(
        JsContextWrapper ctxInstance,
        JsValue @this,
        JsAtom prop,
        JsValue val,
        JsValue getter,
        JsValue setter,
        JsPropertyFlags flags
    )
    {
        ctxInstance.ThrowJsError(new NotImplementedException(nameof(Invoke)));
        return false;
    }

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

    #region Helper
    private static bool TryGetInstance(JsValue value, [NotNullWhen(true)] out ClrProxyBase? obj)
    {
        var opaque = Native.JS_GetOpaqueWithoutClass(value);
        if (opaque != IntPtr.Zero)
        {
            if (GCHandle.FromIntPtr(opaque).Target is ClrProxyBase instance)
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
        [NotNullWhen(false)] out ClrProxyBase? instance
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

    #endregion
}
