using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Hosihikari.VanillaScript.QuickJS.Exceptions;
using Hosihikari.VanillaScript.QuickJS.Types;
using Hosihikari.VanillaScript.QuickJS.Wrapper;
using Hosihikari.VanillaScript.QuickJS.Wrapper.ClrProxy;

namespace Hosihikari.VanillaScript.QuickJS.Extensions;

public static class JsValueExtension
{
    public static int GetRefCount(this JsValue @this)
    {
        if (@this.HasRefCount())
        {
            unsafe
            {
                JsRefCountHeader* p = (JsRefCountHeader*)@this.ptr;
                return p->RefCount;
            }
        }

        return -1;
    }

    //ref #L252
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasRefCount(this JsValue @this)
    {
        unchecked
        {
            return ((uint)@this.Tag >= (uint)JsTag.First);
        }
    }

    //typedef struct JSRefCountHeader
    //{
    //    int ref_count;
    //}
    //JSRefCountHeader;
    private struct JsRefCountHeader
    {
        public int RefCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //ref #L667 JS_DupValue
    public static void UnsafeAddRefCount(this JsValue @this)
    {
        if (@this.HasRefCount())
        {
            unsafe
            { //ref #216 #define JS_VALUE_GET_PTR(v) ((v).u.ptr)
                JsRefCountHeader* p = (JsRefCountHeader*)@this.ptr;
                p->RefCount++;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //ref #L643 JS_FreeValue
    public static unsafe void UnsafeRemoveRefCount(this JsValue @this, JsContext* ctx)
    { /*
      void __JS_FreeValue(JSContext *ctx, JSValue v);
    static inline void JS_FreeValue(JSContext *ctx, JSValue v)
    {
        if (JS_VALUE_HAS_REF_COUNT(v)) {
            JSRefCountHeader *p = (JSRefCountHeader *)JS_VALUE_GET_PTR(v);
            if (--p->ref_count <= 0) {
                __JS_FreeValue(ctx, v);
            }
        }
    }
      */
#if DEBUG
        if (@this.HasRefCount())
        {
            //Log.Logger.Trace(
            //    "UnsafeRemoveRefCount ctx: 0x"
            //        + ((nint)ctx).ToString("X")
            //        + " tag:"
            //        + @this.Tag
            //        + " refCount:"
            //        + @this.GetRefCount()
            //);
        }
        if (!Enum.IsDefined(@this.Tag))
        {
            Log.Logger.Error(
                "UnsafeRemoveRefCount ctx: 0x"
                    + ((nint)ctx).ToString("X")
                    + " tag:"
                    + @this.Tag
                    + " refCount:"
                    + @this.GetRefCount()
            );
            Log.Logger.Error(
                "tag not define. may already freed by GC. please check." + Environment.StackTrace
            );
        }
#endif
        if (@this.HasRefCount())
        {
            JsRefCountHeader* p = (JsRefCountHeader*)@this.ptr;
            if (--p->RefCount <= 0)
            {
                //call __JS_FreeValue
                Native.__JS_FreeValue(ctx, @this);
            }
        }
    }

    //GetStringProperty
    public static unsafe string GetStringProperty(
        this JsValue @this,
        JsContext* ctx,
        string propertyName
    )
    {
        using var val = Native.JS_GetPropertyStr(ctx, @this, propertyName);
        return val.ToString();
    }

    public static unsafe string GetStringProperty(this JsValue @this, JsContext* ctx, JsAtom atom)
    {
        using var val = Native.JS_GetPropertyInternal(ctx, @this, atom);
        return val.ToString();
    }

    public static unsafe AutoDropJsValue GetProperty(
        this JsValue @this,
        JsContext* ctx,
        JsAtom atom
    )
    {
        return Native.JS_GetPropertyInternal(ctx, @this, atom);
    }

    public static unsafe AutoDropJsValue GetProperty(
        this JsValue @this,
        JsContext* ctx,
        string propertyName
    )
    {
        return Native.JS_GetPropertyStr(ctx, @this, propertyName);
    }

    public static unsafe bool HasProperty(this JsValue @this, JsContext* ctx, JsAtom atom)
    {
        return Native.JS_HasProperty(ctx, @this, atom);
    }

    public static unsafe bool HasProperty(this JsValue @this, JsContext* ctx, string propertyName)
    {
        using var atom = Native.JS_NewAtom(ctx, propertyName);
        return HasProperty(@this, ctx, atom.Value);
    }

    //JS_ToString
    public static unsafe string ToString(this JsValue @this, JsContext* ctx)
    {
        return Native.JS_ToCString(ctx, @this);
    }

    public static unsafe string ToString(this JsValue @this, JsContextWrapper ctx) =>
        ToString(@this, ctx.Context);

    public static unsafe bool SetProperty(
        this JsValue @this,
        JsContext* ctx,
        JsValue propertyKey,
        JsValue value,
        JsPropertyFlags flags = JsPropertyFlags.CWE
    )
    {
        return Native.JS_SetPropertyValue(ctx, @this, propertyKey, value, flags);
    }

    public static unsafe bool SetProperty(
        this JsValue @this,
        JsContext* ctx,
        string propertyName,
        JsValue value
    )
    {
        return Native.JS_SetPropertyStr(ctx, @this, propertyName, value);
    }

    public static unsafe bool SetProperty(
        this JsValue @this,
        JsContext* ctx,
        JsAtom atom,
        JsValue value,
        JsPropertyFlags flags = JsPropertyFlags.CWE
    )
    {
        return Native.JS_SetPropertyInternal(ctx, @this, atom, value, flags);
    }

    public static unsafe bool DefineProperty(
        this JsValue @this,
        JsContext* ctx,
        string propertyName,
        JsValue value,
        JsPropertyFlags flags = JsPropertyFlags.CWE
    )
    {
        return Native.JS_DefinePropertyValueStr(ctx, @this, propertyName, value, flags);
    }

    public static unsafe bool DefineProperty(
        this JsValue @this,
        JsContext* ctx,
        JsAtom propertyAtom,
        JsValue value,
        JsPropertyFlags flags = JsPropertyFlags.CWE
    )
    {
        return Native.JS_DefinePropertyValue(ctx, @this, propertyAtom, value, flags);
    }

    public static unsafe bool DefineFunction(
        this JsValue @this,
        JsContext* ctx,
        string funcName,
        delegate* unmanaged<JsContext*, JsValue, int, JsValue*, JsValue> func,
        int argumentLength,
        JsCFunctionEnum cproto = JsCFunctionEnum.Generic,
        int magic = 0,
        JsPropertyFlags flags = JsPropertyFlags.CWE
    )
    {
        using var value = Native.JS_NewCFunction2(
            ctx,
            func,
            funcName,
            argumentLength,
            cproto,
            magic
        );
        return Native.JS_DefinePropertyValueStr(ctx, @this, funcName, value.Steal(), flags);
    }

    public static unsafe string ToJson(this JsValue @this, JsContext* ctx)
    {
        return Native.JS_JSONStringify(ctx, @this);
    }

    public static unsafe string GetClassName(this JsValue @this, JsContext* ctx)
    {
        return Native.JS_GetClassName(ctx, @this);
    }

    #region Converter



    public static bool ToBoolean(this JsValue @this)
    {
        if (TryGetBoolean(@this, out var result))
        {
            return result;
        }
        throw new InvalidCastException(@this.Tag.ToString());
    }

    public static bool TryGetBoolean(this JsValue @this, out bool value)
    {
        if (@this.Tag == JsTag.Bool)
        {
            value = @this.int32 != 0;
            return true;
        }
        value = default;
        return false;
    }

    public static int ToInt32(this JsValue @this)
    {
        if (TryGetInt32(@this, out var result))
        {
            return result;
        }
        throw new InvalidCastException(@this.Tag.ToString());
    }

    public static bool TryGetInt32(this JsValue @this, out int value)
    {
        if (@this.Tag == JsTag.Int)
        {
            value = @this.int32;
            return true;
        }
        value = default;
        return false;
    }

    public static object? ToClrObject(this JsValue @this, JsContextWrapper ctx, Type type)
    {
        unsafe
        {
            return ToClrObject(@this, ctx.Context, type);
        }
    }

    public static double ToDouble(this JsValue @this)
    {
        if (TryGetDouble(@this, out var result))
        {
            return result;
        }
        throw new InvalidCastException(@this.Tag.ToString());
    }

    public static bool TryGetDouble(this JsValue @this, out double value)
    {
        if (@this.Tag == JsTag.Int)
        {
            value = @this.int32;
            return true;
        }
        if (@this.Tag == JsTag.Float64)
        {
            value = @this.float64;
            return true;
        }
        value = default;
        return false;
    }

    public static unsafe object? ToClrObject(this JsValue @this, JsContext* ctx, Type type)
    {
        var value = ToClrObject(@this, ctx);
        if (value is null)
        {
            return null;
        }
        if (value is JsonNode json)
        {
            return json.Deserialize(type);
        }
        if (type.IsInstanceOfType(value))
        {
            return value;
        }
        if (type.IsEnum)
        {
            return Enum.ToObject(type, value);
        }
        if (type == typeof(string))
            return value.ToString();
        if (type == typeof(bool))
            return Convert.ToBoolean(value);
        if (type == typeof(byte))
            return Convert.ToByte(value);
        if (type == typeof(sbyte))
            return Convert.ToSByte(value);
        if (type == typeof(char))
            return Convert.ToChar(value);
        if (type == typeof(decimal))
            return Convert.ToDecimal(value);
        if (type == typeof(double))
            return Convert.ToDouble(value);
        if (type == typeof(float))
            return Convert.ToSingle(value);
        if (type == typeof(int))
            return Convert.ToInt32(value);
        if (type == typeof(uint))
            return Convert.ToUInt32(value);
        if (type == typeof(long))
            return Convert.ToInt64(value);
        if (type == typeof(ulong))
            return Convert.ToUInt64(value);
        if (type == typeof(short))
            return Convert.ToInt16(value);
        if (type == typeof(ushort))
            return Convert.ToUInt16(value);
        if (type == typeof(DateTime))
            return Convert.ToDateTime(value);
        if (type == typeof(Guid))
            return Guid.Parse(value.ToString() ?? throw new NullReferenceException());
        if (type == typeof(TimeSpan))
            return TimeSpan.Parse(value.ToString() ?? throw new NullReferenceException());
        if (type == typeof(Uri))
            return new Uri(value.ToString() ?? throw new NullReferenceException());
        if (type == typeof(byte[]))
        {
            var val = value.ToString() ?? throw new NullReferenceException();
            var bytes = new byte[val.Length / 2];
            return Convert.TryFromBase64String(val, bytes, out var len)
                ? bytes[..len].ToArray()
                : Convert.FromHexString(value.ToString() ?? throw new NullReferenceException());
        }
        if (type == typeof(char[]))
            return value.ToString()?.ToCharArray();
        if (type == typeof(DateTimeOffset))
            return DateTimeOffset.Parse(value.ToString() ?? throw new NullReferenceException());
        throw new InvalidCastException($"can not convert {value.GetType()} to {type}");
    }

    //todo JS_ToFloat64?
    public static unsafe object? ToClrObject(this JsValue @this, JsContextWrapper ctx) =>
        ToClrObject(@this, ctx.Context);

    public static unsafe object? ToClrObject(this JsValue @this, JsContext* ctx)
    {
        switch (@this.Tag)
        {
            case JsTag.Bool:
                return @this.ToBoolean();
            case JsTag.Null
            or JsTag.Undefined
            or JsTag.Uninitialized:
                return null;
            case JsTag.String:
                return @this.ToString(ctx);
            case JsTag.Int:
                return @this.ToInt32();
            case JsTag.Float64:
                return @this.ToDouble();
            case JsTag.Exception:
            case JsTag.Object when Native.JS_IsError(ctx, @this):
                return new QuickJsException(@this, ctx);
            case JsTag.FunctionBytecode:
            case JsTag.Object when Native.JS_IsFunction(ctx, @this):
                throw new NotImplementedException("js function to clr not impl");
            //case JsTag.Object when Native.JS_IsArray(ctx, @this):
            //    throw new NotImplementedException("js array to clr not impl");
            case JsTag.Object:
                if (
                    ClrProxyBase.TryGetInstance(@this, out var item)
                    && item is ClrInstanceProxy { Instance: var instance }
                )
                {
                    return instance;
                }
                if (
                    ClrProxyBase.TryGetInstance(@this, out item)
                    && item is ClrTypeProxy { Type: var type }
                )
                {
                    return type;
                }
                var json = @this.ToJson(ctx);
                return JsonNode.Parse(json);
            default:
                throw new NotImplementedException($"js value {@this.Tag} to clr not impl");
        }
    }
    #endregion
}
