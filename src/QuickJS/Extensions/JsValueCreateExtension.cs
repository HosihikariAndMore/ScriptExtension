using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.QuickJS.Extensions;

//todo for create JsValue from int or float ...
//ref #L505
internal static class JsValueCreateExtension
{
    //private static JsValue NewInt
    //
    //static js_force_inline JSValue JS_NewBool(JSContext* ctx, JS_BOOL val)
    //{
    //    return JS_MKVAL(JS_TAG_BOOL, (val != 0));
    //}

    //static js_force_inline JSValue JS_NewInt32(JSContext* ctx, int32_t val)
    //{
    //    return JS_MKVAL(JS_TAG_INT, val);
    //}

    //static js_force_inline JSValue JS_NewCatchOffset(JSContext* ctx, int32_t val)
    //{
    //    return JS_MKVAL(JS_TAG_CATCH_OFFSET, val);
    //}

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
}
