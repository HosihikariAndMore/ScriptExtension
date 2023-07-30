using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Hosihikari.VanillaScript.QuickJS.Types;
using Hosihikari.VanillaScript.QuickJS.Wrapper;

namespace Hosihikari.VanillaScript.QuickJS.Helper;

//ref #L505
internal static class JsValueCreateHelper
{
    //#define JS_MKVAL(tag, val) (JSValue){ (JSValueUnion){ .int32 = val }, tag }
    private static JsValue MkVal(JsTag tag, int val)
    {
        var item = new JsValue { Tag = tag, int32 = val };
        return item;
    }

    //    /* special values */
    //#define JS_NULL      JS_MKVAL(JS_TAG_NULL, 0)
    public static JsValue Null => MkVal(JsTag.Null, 0);

    //#define JS_UNDEFINED JS_MKVAL(JS_TAG_UNDEFINED, 0)
    public static JsValue Undefined => MkVal(JsTag.Undefined, 0);

    //#define JS_FALSE     JS_MKVAL(JS_TAG_BOOL, 0)
    public static JsValue False => MkVal(JsTag.Bool, 0);

    //#define JS_TRUE      JS_MKVAL(JS_TAG_BOOL, 1)
    public static JsValue True => MkVal(JsTag.Bool, 1);

    //#define JS_EXCEPTION JS_MKVAL(JS_TAG_EXCEPTION, 0)
    public static JsValue Exception => MkVal(JsTag.Exception, 0);

    //#define JS_UNINITIALIZED JS_MKVAL(JS_TAG_UNINITIALIZED, 0)
    public static JsValue Uninitialized => MkVal(JsTag.Uninitialized, 0);

    //#define JS_MKPTR(tag, p) (JSValue){ (JSValueUnion){ .ptr = p }, tag }

    //#define JS_NAN (JSValue){ .u.float64 = JS_FLOAT64_NAN, JS_TAG_FLOAT64 }
    public static JsValue Nan => new() { Tag = JsTag.Float64, float64 = double.NaN };

    //static inline JSValue __JS_NewFloat64(JSContext *ctx, double d)
    //{
    //    JSValue v;
    //    v.tag = JS_TAG_FLOAT64;
    //    v.u.float64 = d;
    //    return v;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static JsValue __NewFloat64(double d) => new() { Tag = JsTag.Float64, float64 = d };

    //static inline JS_BOOL JS_VALUE_IS_NAN(JSValue v)
    //{
    //    union {
    //        double d;
    //        uint64_t u64;
    //    } u;
    //    if (v.tag != JS_TAG_FLOAT64)
    //        return 0;
    //    u.d = v.u.float64;
    //    return (u.u64 & 0x7fffffffffffffff) > 0x7ff0000000000000;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNan(JsValue v)
    {
        return v.Tag == JsTag.Float64 && (v.uint64 & 0x7fffffffffffffff) > 0x7ff0000000000000;
    }

    //static js_force_inline JSValue JS_NewBool(JSContext* ctx, JS_BOOL val)
    //{
    //    return JS_MKVAL(JS_TAG_BOOL, (val != 0));
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsValue NewBool(bool val) => MkVal(JsTag.Bool, val ? 1 : 0);

    //static js_force_inline JSValue JS_NewInt32(JSContext* ctx, int32_t val)
    //{
    //    return JS_MKVAL(JS_TAG_INT, val);
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsValue NewInt32(int val) => MkVal(JsTag.Int, val);

    //static js_force_inline JSValue JS_NewCatchOffset(JSContext* ctx, int32_t val)
    //{
    //    return JS_MKVAL(JS_TAG_CATCH_OFFSET, val);
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsValue NewCatchOffset(int val) => MkVal(JsTag.CatchOffset, val);

    //static js_force_inline JSValue JS_NewInt64(JSContext* ctx, int64_t val)
    //{
    //    JSValue v;
    //    if (val == (int32_t)val)
    //    {
    //        v = JS_NewInt32(ctx, val);
    //    }
    //    else
    //    {
    //        v = __JS_NewFloat64(ctx, val);
    //    }
    //    return v;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsValue NewInt64(long val)
    {
        if (val == unchecked((int)val))
        {
            return MkVal(JsTag.Int, (int)val);
        }
        else
        {
            return __NewFloat64(val);
        }
    }

    //static js_force_inline JSValue JS_NewUint32(JSContext* ctx, uint32_t val)
    //{
    //    JSValue v;
    //    if (val <= 0x7fffffff)
    //    {
    //        v = JS_NewInt32(ctx, val);
    //    }
    //    else
    //    {
    //        v = __JS_NewFloat64(ctx, val);
    //    }
    //    return v;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsValue NewUint32(uint val)
    {
        if (val <= 0x7fffffff)
        {
            return MkVal(JsTag.Int, (int)val);
        }
        return __NewFloat64(val);
    }

    //JSValue JS_NewBigInt64(JSContext* ctx, int64_t v);

    //JSValue JS_NewBigUint64(JSContext* ctx, uint64_t v);

    //static js_force_inline JSValue JS_NewFloat64(JSContext* ctx, double d)
    //{
    //    JSValue v;
    //    int32_t val;
    //    union {
    //        double d;
    //        uint64_t u;
    //    }
    //    u, t;
    //    u.d = d;
    //    val = (int32_t)d;
    //    t.d = val;
    //    /* -0 cannot be represented as integer, so we compare the bit
    //        representation */
    //    if (u.u == t.u)
    //    {
    //        v = JS_MKVAL(JS_TAG_INT, val);
    //    }
    //    else
    //    {
    //        v = __JS_NewFloat64(ctx, d);
    //    }
    //    return v;
    //}
    [StructLayout(LayoutKind.Explicit)]
    private struct NewFloat64Union
    {
        [FieldOffset(0)]
        public double d;

        [FieldOffset(0)]
        public ulong u;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsValue NewFloat64(double d)
    {
        NewFloat64Union u = default;
        NewFloat64Union t = default;
        u.d = d;
        int val = unchecked((int)d);
        t.d = val;
        /* -0 cannot be represented as integer, so we compare the bit
            representation */
        return u.u == t.u ? MkVal(JsTag.Int, val) : __NewFloat64(d);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="str"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe AutoDropJsValue NewString(JsContext* ctx, string str)
    {
        return Native.JS_NewString(ctx, str);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="str"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe AutoDropJsValue FromJson(JsContext* ctx, string str)
    {
        return Native.JS_ParseJSON(ctx, str);
    }

    /// <summary>
    /// new empty object
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe AutoDropJsValue NewObject(JsContext* ctx)
    {
        return Native.JS_NewObject(ctx);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe AutoDropJsValue NewPromise(
        JsContext* ctx,
        out SafeJsValue resolve,
        out SafeJsValue reject
    )
    {
        return Native.JS_NewPromiseCapability(ctx, out resolve, out reject);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe (
        AutoDropJsValue promise,
        SafeJsValue resolve,
        SafeJsValue reject
    ) NewPromise(JsContext* ctx)
    {
        var promise = Native.JS_NewPromiseCapability(ctx, out var resolve, out var reject);
        return (promise, resolve, reject);
    }
}
