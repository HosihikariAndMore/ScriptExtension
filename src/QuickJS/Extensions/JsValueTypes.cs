using System.Runtime.CompilerServices;
using Hosihikari.VanillaScript.QuickJS.Types;
using Hosihikari.VanillaScript.QuickJS.Wrapper;

namespace Hosihikari.VanillaScript.QuickJS.Extensions;

//ref #L556
public static class JsValueTypes
{
    //#define JS_TAG_IS_FLOAT64(tag) ((unsigned)(tag) == JS_TAG_FLOAT64)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFloat64(this JsTag @this)
    {
        return @this is JsTag.Float64;
    }

    //static inline JS_BOOL JS_IsNumber(JSValueConst v)
    //{
    //    int tag = JS_VALUE_GET_TAG(v);
    //    return tag == JS_TAG_INT || JS_TAG_IS_FLOAT64(tag);
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNumber(this JsValue @this)
    {
        return @this.Tag is JsTag.Int or JsTag.Float64;
    }

    //static inline JS_BOOL JS_IsBigInt(JSContext* ctx, JSValueConst v)
    //{
    //    int tag = JS_VALUE_GET_TAG(v);
    //    return tag == JS_TAG_BIG_INT;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBigInt(this JsValue @this)
    {
        return @this.Tag is JsTag.BigInt;
    }

    //static inline JS_BOOL JS_IsBigFloat(JSValueConst v)
    //{
    //    int tag = JS_VALUE_GET_TAG(v);
    //    return tag == JS_TAG_BIG_FLOAT;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBigFloat(this JsValue @this)
    {
        return @this.Tag is JsTag.BigFloat;
    }

    //static inline JS_BOOL JS_IsBigDecimal(JSValueConst v)
    //{
    //    int tag = JS_VALUE_GET_TAG(v);
    //    return tag == JS_TAG_BIG_DECIMAL;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBigDecimal(this JsValue @this)
    {
        return @this.Tag is JsTag.BigDecimal;
    }

    //static inline JS_BOOL JS_IsBool(JSValueConst v)
    //{
    //    return JS_VALUE_GET_TAG(v) == JS_TAG_BOOL;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBool(this JsValue @this)
    {
        return @this.Tag is JsTag.Bool;
    }

    //static inline JS_BOOL JS_IsNull(JSValueConst v)
    //{
    //    return JS_VALUE_GET_TAG(v) == JS_TAG_NULL;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNull(this JsValue @this)
    {
        return @this.Tag is JsTag.Null;
    }

    //static inline JS_BOOL JS_IsUndefined(JSValueConst v)
    //{
    //    return JS_VALUE_GET_TAG(v) == JS_TAG_UNDEFINED;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsUndefined(this JsValue @this)
    {
        return @this.Tag is JsTag.Undefined;
    }

    //static inline JS_BOOL JS_IsException(JSValueConst v)
    //{
    //    return js_unlikely(JS_VALUE_GET_TAG(v) == JS_TAG_EXCEPTION);
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsException(this JsValue @this)
    {
        return @this.Tag is JsTag.Exception;
    }

    //static inline JS_BOOL JS_IsUninitialized(JSValueConst v)
    //{
    //    return js_unlikely(JS_VALUE_GET_TAG(v) == JS_TAG_UNINITIALIZED);
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsUninitialized(this JsValue @this)
    {
        return @this.Tag is JsTag.Uninitialized;
    }

    //static inline JS_BOOL JS_IsString(JSValueConst v)
    //{
    //    return JS_VALUE_GET_TAG(v) == JS_TAG_STRING;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsString(this JsValue @this)
    {
        return @this.Tag is JsTag.String;
    }

    //static inline JS_BOOL JS_IsSymbol(JSValueConst v)
    //{
    //    return JS_VALUE_GET_TAG(v) == JS_TAG_SYMBOL;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSymbol(this JsValue @this)
    {
        return @this.Tag is JsTag.Symbol;
    }

    //static inline JS_BOOL JS_IsObject(JSValueConst v)
    //{
    //    return JS_VALUE_GET_TAG(v) == JS_TAG_OBJECT;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsObject(this JsValue @this)
    {
        return @this.Tag is JsTag.Object;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool IsArray(this JsValue @this, JsContextWrapper ctx) =>
        IsArray(@this, ctx.Context);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool IsArray(this JsValue @this, JsContext* ctx)
    {
        if (@this.Tag is not JsTag.Object)
            return false;
        return Native.JS_IsArray(ctx, @this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int GetArrayLength(this JsValue @this, JsContext* ctx)
    {
        if (!IsArray(@this, ctx))
            throw new InvalidOperationException("@this is not array");
        using var value = Native.JS_GetPropertyStr(ctx, @this, "length");
        return value.Value.ToInt32();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int GetArrayLength(this JsValue @this, JsContextWrapper ctx) =>
        GetArrayLength(@this, ctx.Context);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe AutoDropJsValue[] GetArrayValues(
        this JsValue @this,
        JsContextWrapper ctx
    ) => GetArrayValues(@this, ctx.Context);

    public static unsafe AutoDropJsValue[] GetArrayValues(this JsValue @this, JsContext* ctx)
    {
        var len = GetArrayLength(@this, ctx);
        var items = new AutoDropJsValue[len];
        for (var i = 0; i < len; i++)
        {
            items[i] = Native.JS_GetPropertyUint32(ctx, @this, (uint)i);
        }
        return items;
    }
    //ref #L9753
    //BOOL JS_IsError(JSContext* ctx, JSValueConst val)
    //{
    //    JSObject* p;
    //    if (JS_VALUE_GET_TAG(val) != JS_TAG_OBJECT)
    //        return FALSE;
    //    p = JS_VALUE_GET_OBJ(val);
    //    return (p->class_id == JS_CLASS_ERROR);
    //}
}
