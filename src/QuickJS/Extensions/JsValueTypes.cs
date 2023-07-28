using System.Runtime.CompilerServices;
using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.QuickJS.Extensions;

//ref #L556
public static class JsValueTypes
{
    //static inline JS_BOOL JS_IsNumber(JSValueConst v)
    //{
    //    int tag = JS_VALUE_GET_TAG(v);
    //    return tag == JS_TAG_INT || JS_TAG_IS_FLOAT64(tag);
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNumber(this JsValue @this)
    {
        return @this.Data.tag is JsTag.Int or JsTag.Float64;
    }

    //static inline JS_BOOL JS_IsBigInt(JSContext* ctx, JSValueConst v)
    //{
    //    int tag = JS_VALUE_GET_TAG(v);
    //    return tag == JS_TAG_BIG_INT;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBigInt(this JsValue @this)
    {
        return @this.Data.tag is JsTag.BigInt;
    }

    //static inline JS_BOOL JS_IsBigFloat(JSValueConst v)
    //{
    //    int tag = JS_VALUE_GET_TAG(v);
    //    return tag == JS_TAG_BIG_FLOAT;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBigFloat(this JsValue @this)
    {
        return @this.Data.tag is JsTag.BigFloat;
    }

    //static inline JS_BOOL JS_IsBigDecimal(JSValueConst v)
    //{
    //    int tag = JS_VALUE_GET_TAG(v);
    //    return tag == JS_TAG_BIG_DECIMAL;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBigDecimal(this JsValue @this)
    {
        return @this.Data.tag is JsTag.BigDecimal;
    }

    //static inline JS_BOOL JS_IsBool(JSValueConst v)
    //{
    //    return JS_VALUE_GET_TAG(v) == JS_TAG_BOOL;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBool(this JsValue @this)
    {
        return @this.Data.tag is JsTag.Bool;
    }

    //static inline JS_BOOL JS_IsNull(JSValueConst v)
    //{
    //    return JS_VALUE_GET_TAG(v) == JS_TAG_NULL;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNull(this JsValue @this)
    {
        return @this.Data.tag is JsTag.Null;
    }

    //static inline JS_BOOL JS_IsUndefined(JSValueConst v)
    //{
    //    return JS_VALUE_GET_TAG(v) == JS_TAG_UNDEFINED;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsUndefined(this JsValue @this)
    {
        return @this.Data.tag is JsTag.Undefined;
    }

    //static inline JS_BOOL JS_IsException(JSValueConst v)
    //{
    //    return js_unlikely(JS_VALUE_GET_TAG(v) == JS_TAG_EXCEPTION);
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsException(this JsValue @this)
    {
        return @this.Data.tag is JsTag.Exception;
    }

    //static inline JS_BOOL JS_IsUninitialized(JSValueConst v)
    //{
    //    return js_unlikely(JS_VALUE_GET_TAG(v) == JS_TAG_UNINITIALIZED);
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsUninitialized(this JsValue @this)
    {
        return @this.Data.tag is JsTag.Uninitialized;
    }

    //static inline JS_BOOL JS_IsString(JSValueConst v)
    //{
    //    return JS_VALUE_GET_TAG(v) == JS_TAG_STRING;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsString(this JsValue @this)
    {
        return @this.Data.tag is JsTag.String;
    }

    //static inline JS_BOOL JS_IsSymbol(JSValueConst v)
    //{
    //    return JS_VALUE_GET_TAG(v) == JS_TAG_SYMBOL;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSymbol(this JsValue @this)
    {
        return @this.Data.tag is JsTag.Symbol;
    }

    //static inline JS_BOOL JS_IsObject(JSValueConst v)
    //{
    //    return JS_VALUE_GET_TAG(v) == JS_TAG_OBJECT;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsObject(this JsValue @this)
    {
        return @this.Data.tag is JsTag.Object;
    }
}
