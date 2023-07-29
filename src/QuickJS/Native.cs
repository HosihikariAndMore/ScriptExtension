using System.Runtime.CompilerServices;
using Hosihikari.NativeInterop;
using Hosihikari.NativeInterop.Utils;
using Hosihikari.VanillaScript.QuickJS.Exceptions;
using Hosihikari.VanillaScript.QuickJS.Extensions;
using Hosihikari.VanillaScript.QuickJS.Helper;
using Hosihikari.VanillaScript.QuickJS.Types;
using Hosihikari.VanillaScript.QuickJS.Wrapper;
using size_t = nuint;

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
    public static AutoDropJsValue JS_GetGlobalObject(JsContext* ctx)
    {
        JsValue ret = new();
        var func = (delegate* unmanaged<JsValue*, JsContext*, JsValue*>)_ptrJsGetGlobalObject.Value;
        //the call will increase refCount
        var result = func(&ret, ctx);
        //return SafeJsValue to auto remove refCount
        return new AutoDropJsValue(*result, ctx);
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
    #region JS_GetException
    //ref #L6335
    public static AutoDropJsValue JS_GetException(JsContext* ctx)
    {
        var func = (delegate* unmanaged<JsContext*, JsValue>)_ptrJsGetException.Value;
        var result = func(ctx);
        // get the the pending exception (cannot be called twice).
        // the exception is cleared after return.
        // and need to free the exception value if no longer used.
        // so return SafeJsValue to auto remove refCount
        return new AutoDropJsValue(result, ctx);
    }

    private static readonly Lazy<nint> _ptrJsGetException = GetPointerLazy("JS_GetException");
    #endregion
    #region JS_IsError
    //JS_BOOL JS_IsError(JSContext *ctx, JSValueConst val);
    public static bool JS_IsError(JsContext* ctx, JsValue jsValue)
    {
        var func = (delegate* unmanaged<JsContext*, JsValue, int>)_ptrJsIsError.Value;
        return func(ctx, jsValue) != 0;
    }

    private static readonly Lazy<nint> _ptrJsIsError = GetPointerLazy("JS_IsError");
    #endregion
    #region JS_GetPropertyStr
    public static AutoDropJsValue JS_GetPropertyStr(
        JsContext* ctx,
        JsValue @this,
        string propertyName
    )
    {
        fixed (byte* ptr = StringUtils.StringToManagedUtf8(propertyName))
        {
            var func = (delegate* unmanaged<JsContext*, JsValue, byte*, JsValue>)
                _ptrJsGetPropertyStr.Value;
            var result = func(ctx, @this, ptr);
            if (result.IsException())
            {
                throw new QuickJsException(JS_GetException(ctx));
            }
            return new AutoDropJsValue(result, ctx);
        }
    }

    private static readonly Lazy<nint> _ptrJsGetPropertyStr = GetPointerLazy("JS_GetPropertyStr");
    #endregion
    #region JS_ToCStringLen2

    /// <summary>
    /// Returns a string representation of the value of the current instance.
    /// </summary>
    /// <param name="val"><see cref="JsValue"/> Instance</param>
    /// <param name="cesu8">Determines if non-BMP1 codepoints are encoded as 1 or 2 utf-8 sequences.</param>
    /// <param name="ctx">The context that <see cref="JSValue"/> belongs to.</param>
    public static string JS_ToCString(JsContext* ctx, JsValue val, bool cesu8 = true)
    { //ref #L3971
        //const char *JS_ToCStringLen2(JSContext *ctx, size_t *plen, JSValueConst val1, BOOL cesu8)
        var func = (delegate* unmanaged<JsContext*, out size_t, JsValue, int, byte*>)
            _ptrJsToCStringLen2.Value;
        var ptr = func(ctx, out var len, val, cesu8 ? 1 : 0);
        if (ptr is null)
        {
            /* return (NULL, 0) if exception. */
            throw new QuickJsException(JS_GetException(ctx));
        }
        try
        {
            return StringUtils.Utf8ToString(new ReadOnlySpan<byte>(ptr, (int)len));
        }
        finally
        { //free the string (pointer stands for JSString)
            JS_FreeCString(ctx, ptr);
        }
    }

    private static readonly Lazy<nint> _ptrJsToCStringLen2 = GetPointerLazy("JS_ToCStringLen2");

    #endregion
    #region JS_FreeCString
    private static void JS_FreeCString(JsContext* ctx, byte* ptr)
    {
        var func = (delegate* unmanaged<JsContext*, byte*, void>)_ptrJsFreeCString.Value;
        func(ctx, ptr);
    }

    private static readonly Lazy<nint> _ptrJsFreeCString = GetPointerLazy("JS_FreeCString");
    #endregion
    #region JS_SetPropertyFunctionList
    //ref #L36128
    //todo

    #endregion
    #region JS_GetClassProto
    //JSValue JS_GetClassProto(JSContext* ctx, JSClassID class_id)
    //{
    //    JSRuntime* rt = ctx->rt;
    //    assert(class_id < rt->class_count);
    //    return JS_DupValue(ctx, ctx->class_proto[class_id]);
    //}
    public static AutoDropJsValue JS_GetClassProto(JsContext* ctx, JsClassIdEnum classId)
    {
        var func = (delegate* unmanaged<JsContext*, JsClassIdEnum, JsValue>)
            _ptrJsGetClassProto.Value;
        var result = func(ctx, classId);
        if (result.IsException())
        {
            throw new QuickJsException(JS_GetException(ctx));
        }
        //it use JS_DupValue so need to free
        return new AutoDropJsValue(result, ctx);
    }

    private static readonly Lazy<nint> _ptrJsGetClassProto = GetPointerLazy("JS_GetClassProto");
    #endregion
    #region JS_DefinePropertyValueStr
    //int JS_DefinePropertyValueStr(JSContext *ctx, JSValueConst this_obj,
    //                              const char *prop, JSValue val, int flags)

    public static bool JS_DefinePropertyValueStr(
        JsContext* ctx,
        JsValue thisObj,
        string prop,
        JsValue val,
        JsPropertyFlags flags
    )
    {
        fixed (byte* ptr = StringUtils.StringToManagedUtf8(prop))
        {
            var func = (delegate* unmanaged<JsContext*, JsValue, byte*, JsValue, int, int>)
                _ptrJsDefinePropertyValueStr.Value;
            var result = func(ctx, thisObj, ptr, val, (int)flags);
            if (result == -1)
            {
                throw new QuickJsException(JS_GetException(ctx));
            }
            return result != 0;
        }
    }

    private static readonly Lazy<nint> _ptrJsDefinePropertyValueStr = GetPointerLazy(
        "JS_DefinePropertyValueStr"
    );

    #endregion
    #region JS_Eval
    //JSValue JS_Eval(JSContext *ctx, const char *input, size_t input_len,
    //const char* filename, int eval_flags)
    internal static AutoDropJsValue JS_Eval(
        JsContext* ctx,
        string file,
        string content,
        JsEvalFlag flags = JsEvalFlag.TypeModule
    )
    {
        fixed (byte* filePtr = StringUtils.StringToManagedUtf8(file))
        fixed (byte* contentPtr = StringUtils.StringToManagedUtf8(content))
        {
            var func = (delegate* unmanaged<JsContext*, byte*, size_t, byte*, int, JsValue>)
                _ptrJsEval.Value;
            var result = func(ctx, contentPtr, (size_t)content.Length, filePtr, (int)flags);
            if (result.IsException())
            {
                throw new QuickJsException(JS_GetException(ctx));
            }
            return new AutoDropJsValue(result, ctx);
        }
    }

    private static readonly Lazy<nint> _ptrJsEval = GetPointerLazy("JS_Eval");
    #endregion
    #region JS_NewCFunction2
    //JSValue JS_NewCFunction2(JSContext* ctx, JSCFunction* func,
    //                     const char* name,
    //                     int length, JSCFunctionEnum cproto, int magic)
    //typedef JSValue JSCFunction(JSContext *ctx, JSValueConst this_val, int argc, JSValueConst *argv);
    public static AutoDropJsValue JS_NewCFunction2(
        JsContext* ctx,
        delegate* unmanaged<JsContext*, JsValue, int, JsValue*, JsValue> func,
        string name,
        int argumentLength, //Note: at least 'length' arguments will be readable in 'argv'
        JscFunctionEnum cproto,
        int magic
    )
    {
        fixed (byte* ptr = StringUtils.StringToManagedUtf8(name))
        {
            var funcPtr = (delegate* unmanaged<
                JsContext*,
                delegate* unmanaged<JsContext*, JsValue, int, JsValue*, JsValue>,
                byte*,
                int,
                JscFunctionEnum,
                int,
                JsValue>)
                _ptrJsNewCFunction2.Value;
            var result = funcPtr(ctx, func, ptr, argumentLength, cproto, magic);
            if (result.IsException())
            {
                throw new QuickJsException(JS_GetException(ctx));
            }
            return new AutoDropJsValue(result, ctx);
        }
    }

    private static readonly Lazy<nint> _ptrJsNewCFunction2 = GetPointerLazy("JS_NewCFunction2");
    #endregion
    #region JS_NewCFunction3
    //
    //todo  JS_NewCFunction3 has protoVal which filled with ctx->function_proto in JS_NewCFunction2
    ///*
    // static JSValue JS_NewCFunction3(JSContext *ctx, JSCFunction *func,
    //                            const char *name,
    //                            int length, JSCFunctionEnum cproto, int magic,
    //                            JSValueConst proto_val)
    // */
    ////typedef JSValue JSCFunction(JSContext *ctx, JSValueConst this_val, int argc, JSValueConst *argv);
    //public static JsValue JS_NewCFunction3(
    //    JsContext* ctx,
    //    delegate* unmanaged<JsContext*, JsValue, int, JsValue*, JsValue> func,
    //    string name,
    //    int length, //Note: at least 'length' arguments will be readable in 'argv'
    //    JscFunctionEnum cproto,
    //    int magic,
    //    JsValue protoVal
    //)
    //{

    //}

    #endregion
    #region JS_NewObject
    /// <summary>
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="autoDrop"> whether to decrease ref count when released from managed environment </param>
    public static AutoDropJsValue JS_NewObject(JsContext* ctx, bool autoDrop)
    {
        var func = (delegate* unmanaged<JsContext*, JsValue>)_ptrJsNewObject.Value;
        //#L4723 JS_NewObjectFromShape
        // `p->header.ref_count = 1;`
        // so initial refCount is 1
        var result = func(ctx);

        //process exception
        //    sh = js_new_shape(ctx, proto);
        //if (!sh)
        //    return JS_EXCEPTION;
        if (result.IsException()) //if the return value is exception, indicate that call JS_GetException will get the real exception data
        {
            throw new QuickJsException(JS_GetException(ctx));
        }
        return autoDrop ? new AutoDropJsValue(result, ctx) : new SafeJsValue(result, ctx);
    }

    private static readonly Lazy<nint> _ptrJsNewObject = GetPointerLazy("JS_NewObject");
    #endregion
    #region JS_NewString
    //JSValue JS_NewStringLen(JSContext *ctx, const char *buf, size_t buf_len)
    /// <summary>
    ///
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="str"></param>
    /// <param name="autoDrop"> whether to decrease ref count when released from managed environment </param>
    /// <returns></returns>
    /// <exception cref="QuickJsException"></exception>
    public static AutoDropJsValue JS_NewString(JsContext* ctx, string str, bool autoDrop)
    {
        fixed (byte* ptr = StringUtils.StringToManagedUtf8(str, out var len))
        {
            var func = (delegate* unmanaged<JsContext*, byte*, nuint, JsValue>)
                _ptrJsNewStringLen.Value;
            var result = func(ctx, ptr, (nuint)len);
            if (result.IsException())
            {
                throw new QuickJsException(JS_GetException(ctx));
            }
            return autoDrop ? new AutoDropJsValue(result, ctx) : new SafeJsValue(result, ctx);
        }
    }

    private static readonly Lazy<nint> _ptrJsNewStringLen = GetPointerLazy("JS_NewStringLen");
    #endregion
    #region JS_ParseJSON
    //JSValue JS_ParseJSON(JSContext *ctx, const char *buf, size_t buf_len,const char* filename, int flags)
    /// <summary>
    ///
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="jsonStr"></param>
    /// <param name="autoDrop"> whether to decrease ref count when released from managed environment </param>
    /// <param name="filename"></param>
    /// <returns></returns>
    /// <exception cref="QuickJsException"></exception>
    public static AutoDropJsValue JS_ParseJSON(
        JsContext* ctx,
        string jsonStr,
        bool autoDrop,
        string filename = "<native>"
    )
    {
        fixed (byte* ptr = StringUtils.StringToManagedUtf8(jsonStr, out var len))
        fixed (byte* ptrFile = StringUtils.StringToManagedUtf8(filename))
        {
            var func = (delegate* unmanaged<JsContext*, byte*, nuint, byte*, int, JsValue>)
                _ptrJsParseJson.Value;
            var result = func(ctx, ptr, (nuint)len, ptrFile, 0);
            if (result.IsException())
            {
                throw new QuickJsException(JS_GetException(ctx));
            }
            return autoDrop ? new AutoDropJsValue(result, ctx) : new SafeJsValue(result, ctx);
        }
    }

    private static readonly Lazy<nint> _ptrJsParseJson = GetPointerLazy("JS_ParseJSON");
    #endregion
    #region JS_JSONStringify
    //JSValue JS_JSONStringify(JSContext *ctx, JSValueConst obj,
    //JSValueConst replacer, JSValueConst space0)
    public static string JS_JSONStringify(
        JsContext* ctx,
        JsValue obj,
        JsValue? replacer = null,
        JsValue? space0 = null
    )
    {
        var func = (delegate* unmanaged<JsContext*, JsValue, JsValue, JsValue, JsValue>)
            _ptrJsJsonStringify.Value;
        var result = func(
            ctx,
            obj,
            replacer ?? JsValueCreateHelper.Undefined,
            space0 ?? JsValueCreateHelper.Undefined
        );
        if (result.IsException())
        {
            throw new QuickJsException(JS_GetException(ctx));
        }
        return JS_ToCString(ctx, result);
    }

    private static readonly Lazy<nint> _ptrJsJsonStringify = GetPointerLazy("JS_JSONStringify");
    #endregion
    #region JS_Invoke
    //JSValue JS_Invoke(JSContext *ctx, JSValueConst this_val,
    //                  JSAtom atom, int argc, JSValueConst *argv)
    public static AutoDropJsValue JS_Invoke(
        JsContext* ctx,
        JsValue thisVal,
        JsAtom atom,
        int argc,
        JsValue* argv
    )
    {
        var func = (delegate* unmanaged<JsContext*, JsValue, JsAtom, int, JsValue*, JsValue>)
            _ptrJsInvoke.Value;
        var result = func(ctx, thisVal, atom, argc, argv);
        if (result.IsException())
        {
            throw new QuickJsException(JS_GetException(ctx));
        }
        return new AutoDropJsValue(result, ctx);
    }

    private static readonly Lazy<nint> _ptrJsInvoke = GetPointerLazy("JS_Invoke");
    #endregion
    #region __JS_FindAtom
    //JSAtom __JS_FindAtom(JSContext *ctx, const char *name)
    #endregion
    #region JS_ThrowError
    //static JSValue JS_ThrowError2(JSContext *ctx, JSErrorEnum error_num,
    //                          const char *fmt, va_list ap, BOOL add_backtrace)
    //static JSValue JS_ThrowError(JSContext *ctx, JSErrorEnum error_num,
    //                         const char *fmt, va_list ap)
    public static JsValue JS_ThrowError(
        JsContext* ctx,
        string exMessage,
        JsErrorEnum errorType = JsErrorEnum.InternalError
    )
    {
        return JS_ThrowError(
            ctx,
            exMessage.Replace(
                "%",
                "%%"
            ) /* prevent format*/
            ,
            errorType,
            __arglist()
        );
    }

    public static JsValue JS_ThrowError(
        JsContext* ctx,
        string exMessage,
        JsErrorEnum errorType = JsErrorEnum.InternalError,
        __arglist
    )
    {
        fixed (byte* ptr = StringUtils.StringToManagedUtf8(exMessage))
        {
            var func = (delegate* unmanaged<
                JsContext*,
                JsErrorEnum,
                byte*,
                RuntimeArgumentHandle,
                JsValue>)
                _ptrJsThrowError.Value;
            var result = func(ctx, errorType, ptr, __arglist);
            if (!result.IsException())
            {
                //it seem always return exception type
                //so if not exception, it means throw failed ?
                Log.Logger.Error("throw error may failed");
            }
            return result;
        }
    }

    private static readonly Lazy<nint> _ptrJsThrowError = GetPointerLazy("JS_ThrowError");
    #endregion
}
