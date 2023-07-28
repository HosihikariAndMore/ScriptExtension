using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using Hosihikari.NativeInterop;
using Hosihikari.NativeInterop.Utils;
using Hosihikari.VanillaScript.QuickJS.Extensions;
using Hosihikari.VanillaScript.QuickJS.Types;
using Hosihikari.VanillaScript.QuickJS.Wrapper;

namespace Hosihikari.VanillaScript.QuickJS;

internal static unsafe class Native
{
    private static Lazy<nint> GetPointerLazy(string symbol)
    {
        return SymbolHelper.DlsymLazy(symbol);
    }

    #region __JS_FreeValue
    /// <summary>
    /// do not call directly !!!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void __JS_FreeValue(JsContext* ctx, JsValue jsValue)
    {
        ((delegate* unmanaged<JsContext*, JsValue*, void>)_jsFreeValue.Value)(ctx, &jsValue);
    }

    private static readonly Lazy<nint> _jsFreeValue = GetPointerLazy("__JS_FreeValue");
    #endregion

    #region JS_GetGlobalObject
    public static SafeJsValue JS_GetGlobalObject(JsContext* ctx)
    {
        JsValue ret = new();
        var func = (delegate* unmanaged<JsValue*, JsContext*, JsValue*>)_ptrJsGetGlobalObject.Value;
        //the call will increase refCount
        var result = func(&ret, ctx);
        //return SafeJsValue to auto remove refCount
        return new SafeJsValue(*result, ctx);
    }

    private static readonly Lazy<nint> _ptrJsGetGlobalObject = GetPointerLazy("JS_GetGlobalObject");
    #endregion

    #region JS_NewCModule
    //ref #L27196
    //typedef int JSModuleInitFunc(JSContext *ctx, JSModuleDef *m);
    public static JsModuleDef* JS_NewCModule(
        JsContext* ctx,
        string name, /*JSModuleInitFunc*/
        delegate* unmanaged<JsContext*, JsModuleDef*, int> onImport
    )
    {
        fixed (byte* ptr = StringUtils.StringToManagedUtf8(name))
        {
            return (
                (delegate* unmanaged<
                    JsContext*,
                    byte*,
                    delegate* unmanaged<JsContext*, JsModuleDef*, int>,
                    JsModuleDef*>)
                    _ptrJsNewCModule.Value
            )(ctx, ptr, onImport);
        }
    }

    private static readonly Lazy<nint> _ptrJsNewCModule = GetPointerLazy("JS_NewCModule");

    #endregion

    #region JS_AddModuleExport
    //ref #L27209
    //int JS_AddModuleExport(JSContext *ctx, JSModuleDef *m, const char *export_name)
    public static int JS_AddModuleExport(JsContext* ctx, JsModuleDef* module, string exportName)
    {
        fixed (byte* ptr = StringUtils.StringToManagedUtf8(exportName))
        {
            return (
                (delegate* unmanaged<JsContext*, JsModuleDef*, byte*, int>)
                    _ptrJsAddModuleExport.Value
            )(ctx, module, ptr);
        }
    }

    private static readonly Lazy<nint> _ptrJsAddModuleExport = GetPointerLazy("JS_AddModuleExport");
    #endregion

    #region JS_SetModuleExport
    //ref #L27225
    //int JS_SetModuleExport(JSContext *ctx, JSModuleDef *m, const char *export_name, JSValue val)
    public static int JS_SetModuleExport(
        JsContext* ctx,
        JsModuleDef* module,
        string exportName,
        JsValue value
    )
    {
        fixed (byte* ptr = StringUtils.StringToManagedUtf8(exportName))
        {
            return (
                (delegate* unmanaged<JsContext*, JsModuleDef*, byte*, JsValue, int>)
                    _ptrJsSetModuleExport.Value
            )(ctx, module, ptr, value);
        }
    }

    private static readonly Lazy<nint> _ptrJsSetModuleExport = GetPointerLazy("JS_SetModuleExport");
    #endregion
    #region JS_NewObject
    public static JsValue JS_NewObject(JsContext* ctx)
    {
        var func = (delegate* unmanaged<JsContext*, JsValue>)_ptrJsNewObject.Value;
        //#L4723 JS_NewObjectFromShape
        // `p->header.ref_count = 1;`
        // so initial refCount is 1
        var result = func(ctx);
        //it seems no need to decrease refCount
        //because many call such as JS_SetModuleExport will automatic decrease refCount if failed
        //but if JsValue object is not really used, it should be free manually.

        //process exception
        //    sh = js_new_shape(ctx, proto);
        //if (!sh)
        //    return JS_EXCEPTION;

        if (result.IsException())
        {
            var exception = JS_GetException(ctx);
        }
        return result; //new SafeJsValue(*result, ctx);
    }

    private static readonly Lazy<nint> _ptrJsNewObject = GetPointerLazy("JS_NewObject");

    #endregion

    #region JS_GetException
    //ref #L6335
    public static SafeJsValue JS_GetException(JsContext* ctx)
    {
        var func = (delegate* unmanaged<JsContext*, JsValue>)_ptrJsGetException.Value;
        var result = func(ctx);
        // get the the pending exception (cannot be called twice).
        // the exception is cleared after return.
        // and need to free the exception value if no longer used.
        // so return SafeJsValue to auto remove refCount
        return new SafeJsValue(result, ctx);
    }

    private static readonly Lazy<nint> _ptrJsGetException = GetPointerLazy("JS_GetException");
    #endregion
}
