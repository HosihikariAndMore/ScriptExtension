using System.Runtime.CompilerServices;
using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.QuickJS.Extensions;

public static class JsValueExtension
{
    //ref #L252
    private static bool HasRefCount(this JsValue @this) => @this.Data.tag >= JsTag.First;

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
        var val = Native.JS_GetPropertyStr(ctx, @this, propertyName);
        return Native.JS_ToCString(ctx, val.Value);
    }

    //JS_ToString
    public static unsafe string ToString(this JsValue @this, JsContext* ctx)
    {
        return Native.JS_ToCString(ctx, @this);
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

    //public static unsafe bool DefineProperty(
    //    this JsValue @this,
    //    JsContext* ctx,
    //    string propertyName,
    //    string value,
    //    JsPropertyFlags flags = JsPropertyFlags.CWE
    //)
    //{
    //    return Native.JS_DefinePropertyValueStr(ctx, @this, propertyName, value, flags);
    //}
    public static unsafe bool DefineFunction(
        this JsValue @this,
        JsContext* ctx,
        string funcName,
        delegate* unmanaged<JsContext*, JsValue, int, JsValue*, JsValue> func,
        int argumentLength,
        JscFunctionEnum cproto = JscFunctionEnum.Generic,
        int magic = 0,
        JsPropertyFlags flags = JsPropertyFlags.CWE
    )
    {
        var value = Native.JS_NewCFunction2(ctx, func, funcName, argumentLength, cproto, magic);
        return Native.JS_DefinePropertyValueStr(ctx, @this, funcName, value.Value, flags);
    }

    public static unsafe string ToJson(this JsValue @this, JsContext* ctx)
    {
        return Native.JS_JSONStringify(ctx, @this);
    }
}
