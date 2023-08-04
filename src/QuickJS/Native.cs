using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

    private static void ThrowPendingException(JsContext* ctx)
    {
        using var ex = JS_GetException(ctx);
#if DEBUG
        if (ex.Value.IsNull())
        {
            Log.Logger.Error(Environment.StackTrace);
            Log.Logger.Error(
                "[DEBUG Mode] Unexpected ThrowPendingException, please check error handing logic !!!"
            );
            Log.Logger.Information("Press any key to continue.");
            Console.ReadLine();
        }
#endif
        //throw new QuickJsException("unknown exception");
        throw new QuickJsException(ex);
    }

    #region __JS_FreeValue
    /// <summary>
    /// do not call directly !!!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void __JS_FreeValue(JsContext* ctx, JsValue jsValue)
    {
        //Log.Logger.Trace(
        //    "call start __JS_FreeValue ctx: 0x" + ((nint)ctx).ToString("X") + " tag: " + jsValue.Tag
        //);
        ((delegate* unmanaged<JsContext*, JsValue, void>)_jsFreeValue.Value)(ctx, jsValue);
        //Log.Logger.Trace(
        //    "call end __JS_FreeValue ctx: 0x" + ((nint)ctx).ToString("X") + " tag: " + jsValue.Tag
        //);
    }

    private static readonly Lazy<nint> _jsFreeValue = GetPointerLazy("__JS_FreeValue");
    #endregion
    #region JS_GetGlobalObject
    public static AutoDropJsValue JS_GetGlobalObject(JsContext* ctx)
    {
        var func = (delegate* unmanaged<JsContext*, JsValue>)_ptrJsGetGlobalObject.Value;
        //the call will increase refCount
        var result = func(ctx);
        //return SafeJsValue to auto remove refCount
        return new AutoDropJsValue(result, ctx);
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
    public static void JS_AddModuleExport(JsContext* ctx, JsModuleDef* module, string exportName)
    {
        fixed (byte* ptr = StringUtils.StringToManagedUtf8(exportName))
        {
            var result = (
                (delegate* unmanaged<JsContext*, JsModuleDef*, byte*, int>)
                    _ptrJsAddModuleExport.Value
            )(ctx, module, ptr);

            /*
             me = add_export_entry2(ctx, NULL, m, JS_ATOM_NULL, name,
                       JS_EXPORT_TYPE_LOCAL);
JS_FreeAtom(ctx, name);
if (!me)
    return -1;
else
    return 0;
             */
            if (result != 0)
            {
                ThrowPendingException(ctx);
            }
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
        return ((delegate* unmanaged<JsContext*, JsValue, int>)_ptrJsIsError.Value)(ctx, jsValue)
            != 0;
    }

    private static readonly Lazy<nint> _ptrJsIsError = GetPointerLazy("JS_IsError");
    #endregion

    #region JS_IsFunction

    public static bool JS_IsFunction(JsContext* ctx, JsValue @this)
    {
        return ((delegate* unmanaged<JsContext*, JsValue, int>)_ptrJsIsFunction.Value)(ctx, @this)
            != 0;
    }

    private static readonly Lazy<nint> _ptrJsIsFunction = GetPointerLazy("JS_IsFunction");
    #endregion

    #region JS_IsArray

    public static bool JS_IsArray(JsContext* ctx, JsValue @this)
    {
        var result = ((delegate* unmanaged<JsContext*, JsValue, int>)_ptrJsIsArray.Value)(
            ctx,
            @this
        );
        if (result == -1)
            ThrowPendingException(ctx);
        return result != 0;
    }

    private static readonly Lazy<nint> _ptrJsIsArray = GetPointerLazy("JS_IsArray");

    #endregion

    #region JS_IsCFunction

    public static bool JS_IsCFunction(JsContext* ctx, JsValue @this, void* funcPtr, int magic)
    {
        return ((delegate* unmanaged<JsContext*, JsValue, void*, int, int>)_ptrJsIsCFunction.Value)(
                ctx,
                @this,
                funcPtr,
                magic
            ) != 0;
    }

    private static readonly Lazy<nint> _ptrJsIsCFunction = GetPointerLazy("JS_IsCFunction");
    #endregion

    #region JS_IsConstructor

    public static bool JS_IsConstructor(JsContext* ctx, JsValue @this)
    {
        return ((delegate* unmanaged<JsContext*, JsValue, int>)_ptrJsIsConstructor.Value)(
                ctx,
                @this
            ) != 0;
    }

    private static readonly Lazy<nint> _ptrJsIsConstructor = GetPointerLazy("JS_IsConstructor");

    #endregion

    #region JS_SetConstructorBit

    public static void JS_SetConstructorBit(JsContext* ctx, JsValue @this, bool val)
    {
        ((delegate* unmanaged<JsContext*, JsValue, byte, void>)_ptrJsSetConstructorBit.Value)(
            ctx,
            @this,
            val ? (byte)1 : (byte)0
        );
    }

    private static readonly Lazy<nint> _ptrJsSetConstructorBit = GetPointerLazy(
        "JS_SetConstructorBit"
    );
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
                ThrowPendingException(ctx);
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
            ThrowPendingException(ctx);
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


    #region JS_DefinePropertyValue
    //int JS_DefinePropertyValue(JSContext* ctx, JSValueConst this_obj,
    //                       JSAtom prop, JSValue val, int flags)
    public static bool JS_DefinePropertyValue(
        JsContext* ctx,
        JsValue thisObj,
        JsAtom prop,
        JsValue val,
        JsPropertyFlags flags
    )
    {
        var func = (delegate* unmanaged<JsContext*, JsValue, JsAtom, JsValue, int, int>)
            _ptrJsDefinePropertyValue.Value;
        var result = func(ctx, thisObj, prop, val, (int)flags);
        if (result == -1)
        {
            ThrowPendingException(ctx);
        }
        return result == 1;
    }

    private static readonly Lazy<nint> _ptrJsDefinePropertyValue = GetPointerLazy(
        "JS_DefinePropertyValue"
    );
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
                ThrowPendingException(ctx);
            }
            return result == 1;
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
        fixed (byte* contentPtr = StringUtils.StringToManagedUtf8(content, out var len))
        {
            var func = (delegate* unmanaged<JsContext*, byte*, size_t, byte*, int, JsValue>)
                _ptrJsEval.Value;
            var result = func(ctx, contentPtr, (size_t)len, filePtr, (int)flags);
            if (result.IsException())
            {
                ThrowPendingException(ctx);
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
        JsCFunctionEnum cproto,
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
                JsCFunctionEnum,
                int,
                JsValue>)
                _ptrJsNewCFunction2.Value;
            var result = funcPtr(ctx, func, ptr, argumentLength, cproto, magic);
            if (result.IsException())
            {
                ThrowPendingException(ctx);
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

    #region JS_NewError

    //JSValue JS_NewError(JSContext *ctx)
    public static AutoDropJsValue JS_NewError(JsContext* ctx)
    {
        var func = (delegate* unmanaged<JsContext*, JsValue>)_ptrJsNewError.Value;
        var result = func(ctx);
        if (result.IsException())
        {
            ThrowPendingException(ctx);
        }
        return new AutoDropJsValue(result, ctx);
    }

    private static readonly Lazy<nint> _ptrJsNewError = GetPointerLazy("JS_NewError");
    #endregion
    #region JS_NewObject
    /// <summary>
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="autoDrop"> whether to decrease ref count when released from managed environment </param>
    public static AutoDropJsValue JS_NewObject(JsContext* ctx)
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
            ThrowPendingException(ctx);
        }
        return new AutoDropJsValue(result, ctx);
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
    public static AutoDropJsValue JS_NewString(JsContext* ctx, string str)
    {
        fixed (byte* ptr = StringUtils.StringToManagedUtf8(str, out var len))
        {
            var func = (delegate* unmanaged<JsContext*, byte*, nuint, JsValue>)
                _ptrJsNewStringLen.Value;
            var result = func(ctx, ptr, (nuint)len);
            if (result.IsException())
            {
                ThrowPendingException(ctx);
            }
            return new AutoDropJsValue(result, ctx);
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
                ThrowPendingException(ctx);
            }
            return new AutoDropJsValue(result, ctx);
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
            ThrowPendingException(ctx);
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
            ThrowPendingException(ctx);
        }
        return new AutoDropJsValue(result, ctx);
    }

    private static readonly Lazy<nint> _ptrJsInvoke = GetPointerLazy("JS_Invoke");
    #endregion
    #region __JS_FindAtom
    //JSAtom __JS_FindAtom(JSContext *ctx, const char *name)
    #endregion
    //#region JS_ThrowError
    ////static JSValue JS_ThrowError2(JSContext *ctx, JSErrorEnum error_num,
    ////                          const char *fmt, va_list ap, BOOL add_backtrace)
    ////static JSValue JS_ThrowError(JSContext *ctx, JSErrorEnum error_num,
    ////                         const char *fmt, va_list ap)
    //public static JsValue JS_ThrowError(
    //    JsContext* ctx,
    //    string exMessage,
    //    JsErrorEnum errorType = JsErrorEnum.InternalError
    //)
    //{
    //    return JS_ThrowError(
    //        ctx,
    //        exMessage.Replace("%", "%%"), /* prevent format*/
    //        errorType,
    //        true
    //    );
    //}

    //public static JsValue JS_ThrowError(
    //    JsContext* ctx,
    //    string exMessage,
    //    JsErrorEnum errorType,
    //    bool addBackTrace
    //)
    //{
    //    fixed (byte* ptr = StringUtils.StringToManagedUtf8(exMessage))
    //    {
    //        var func = (delegate* unmanaged<JsContext*, JsErrorEnum, byte*, byte*, int, JsValue>)
    //            _ptrJsThrowError.Value;
    //        //var func = Marshal.GetDelegateForFunctionPointer<ThrowErrorDelegate>(
    //        //    _ptrJsThrowError.Value
    //        //);
    //        var result = func(ctx, errorType, ptr, null, addBackTrace ? 1 : 0);
    //        if (!result.IsException())
    //        {
    //            //it seem always return exception type
    //            //so if not exception, it means throw failed ?
    //            Log.Logger.Error("throw error may failed");
    //        }
    //        return result;
    //    }
    //}

    //[StructLayout(LayoutKind.Sequential, Pack = 4)]
    //struct VaListLinuxX64
    //{
    //    //gp_offset The element holds the offset in bytes from reg_save_area to the place where the next available general purpose argument register is saved. In case all argument registers have been exhausted, it is set to the value 48 (6 * 8).
    //    public uint gp_offset = 6 * 8;

    //    //fp_offset The element holds the offset in bytes from reg_save_area to the place where the next available floating point argument register is saved. In case all argument registers have been exhausted, it is set to the value 304 (6 * 8 + 16 * 16).
    //    public uint fp_offset = 6 * 8 + 16 * 16;

    //    //overﬂow_arg_area This pointer is used to fetch arguments passed on the stack. It is initialized with the address of the first argument passed on the stack, if any, and then always updated to point to the start of the next argument on the stack.
    //    public IntPtr overflow_arg_area;

    //    //reg_save_area The element points to the start of the register save area.
    //    public IntPtr reg_save_area;

    //    public VaListLinuxX64() { }
    //}

    ////private delegate JsValue ThrowErrorDelegate(
    ////    JsContext* ctx,
    ////    JsErrorEnum errorNum,
    ////    byte* fmt,
    ////    ArgIterator ap,
    ////    int addBacktrace
    ////);
    //private static readonly Lazy<nint> _ptrJsThrowError = GetPointerLazy("JS_ThrowError2");
    //#endregion

    #region JS_ThrowInternalError
    //JSValue __attribute__((format(printf, 2, 3))) JS_ThrowInternalError(JSContext *ctx, const char *fmt, ...);

    public static JsValue JS_ThrowInternalError(JsContext* ctx, Exception exception)
    {
        return JS_ThrowInternalError(ctx, exception.ToString());
    }

    public static JsValue JS_ThrowInternalError(JsContext* ctx, string message)
    {
        var func = (delegate* unmanaged<JsContext*, byte*, JsValue>)_ptrJsThrowInternalError.Value;
        fixed (
            byte* ptr = StringUtils.StringToManagedUtf8(
                message.Replace("%%", "%") /* prevent format*/
            )
        )
        {
            var result = func(ctx, ptr);
            if (!result.IsException())
            {
                //it seem always return exception type
                //so if not exception, it means throw failed ?
                Log.Logger.Error("throw error may failed");
            }
            return result;
        }
    }

    private static readonly Lazy<nint> _ptrJsThrowInternalError = GetPointerLazy(
        "JS_ThrowInternalError"
    );
    #endregion
    #region JS_GetScriptOrModuleName
    //JSAtom JS_GetScriptOrModuleName(JSContext *ctx, int n_stack_levels)
    public static string JS_GetScriptOrModuleName(JsContext* ctx, int nStackLevels)
    {
        var func = (delegate* unmanaged<JsContext*, int, JsAtom>)_ptrJsGetScriptOrModuleName.Value;
        var result = func(ctx, nStackLevels);
        if (result == JsAtom.BuildIn.Null)
        {
            return string.Empty;
        }
        try
        {
            return JS_AtomToCString(ctx, result);
        }
        finally
        {
            JS_FreeAtom(ctx, result);
        }
    }

    private static readonly Lazy<nint> _ptrJsGetScriptOrModuleName = GetPointerLazy(
        "JS_GetScriptOrModuleName"
    );
    #endregion


    #region __JS_AtomIsConst

    public static bool JS_AtomIsConst(JsContext* ctx, JsAtom atom)
    {
        //ref to void JS_FreeAtom(__int64 a1, int a2)
        // if ( a2 >= 207 )
        return atom.Id < 207;
    }

    #endregion

    #region JS_AtomIsNumericIndex
    public static bool JS_AtomIsNumericIndex(JsContext* ctx, JsAtom atom)
    {
        var func = (delegate* unmanaged<JsContext*, JsAtom, int>)_ptrJsAtomIsNumericIndex.Value;
        var result = func(ctx, atom);
        if (result == -1)
            ThrowPendingException(ctx);
        return result != 0;
    }

    private static readonly Lazy<nint> _ptrJsAtomIsNumericIndex = GetPointerLazy(
        "JS_AtomIsNumericIndex"
    );
    #endregion
    #region JS_AtomIsArrayIndex
    //static BOOL JS_AtomIsArrayIndex(JSContext *ctx, uint32_t *pval, JSAtom atom)
    public static bool JS_AtomIsArrayIndex(JsContext* ctx, out uint val, JsAtom atom)
    {
        var func = (delegate* unmanaged<JsContext*, out uint, JsAtom, int>)
            _ptrJsAtomIsArrayIndex.Value;
        var result = func(ctx, out val, atom);
        return result != 0;
    }

    private static readonly Lazy<nint> _ptrJsAtomIsArrayIndex = GetPointerLazy(
        "JS_AtomIsArrayIndex"
    );
    #endregion
    #region JS_NewAtom
    //JSAtom JS_NewAtomLen(JSContext *ctx, const char *str, size_t len)
    public static AutoDropJsAtom JS_NewAtom(JsContext* ctx, string str)
    {
        var func = (delegate* unmanaged<JsContext*, byte*, nuint, JsAtom>)_ptrJsNewAtomLen.Value;
        fixed (byte* ptr = StringUtils.StringToManagedUtf8(str, out var len))
        {
            var result = func(ctx, ptr, (nuint)len);
            if (result == JsAtom.BuildIn.Null)
            {
                ThrowPendingException(ctx);
            }
            return new AutoDropJsAtom(result, ctx);
        }
    }

    private static readonly Lazy<nint> _ptrJsNewAtomLen = GetPointerLazy("JS_NewAtomLen");
    #endregion
    #region JS_AtomToCString
    //const char *JS_AtomToCString(JSContext *ctx, JSAtom atom)
    public static string JS_AtomToCString(JsContext* ctx, JsAtom atom)
    {
        var func = (delegate* unmanaged<JsContext*, JsAtom, byte*>)_ptrJsAtomToCString.Value;
        var result = func(ctx, atom);
        if (result is null)
            ThrowPendingException(ctx);
        JS_FreeCString(ctx, result);
        return Marshal.PtrToStringUTF8((nint)result) ?? string.Empty;
    }

    private static readonly Lazy<nint> _ptrJsAtomToCString = GetPointerLazy("JS_AtomToCString");
    #endregion
    #region JS_FreeAtom
    //void JS_FreeAtom(JSContext *ctx, JSAtom v)
    public static void JS_FreeAtom(JsContext* ctx, JsAtom v)
    {
        ((delegate* unmanaged<JsContext*, JsAtom, void>)_ptrJsFreeAtom.Value)(ctx, v);
    }

    private static readonly Lazy<nint> _ptrJsFreeAtom = GetPointerLazy("JS_FreeAtom");

    #endregion
    #region JS_NewPromiseCapability
    //JSValue JS_NewPromiseCapability(JSContext *ctx, JSValue *resolving_funcs)
    //ret = JS_Call(ctx, resolve, JS_UNDEFINED, 1, (JSValueConst *)&values);
    public static AutoDropJsValue JS_NewPromiseCapability(
        JsContext* ctx,
        out SafeJsValue resolve,
        out SafeJsValue reject
    )
    {
        //#L46698
        /*
             for(i = 0; i < 2; i++)
                resolving_funcs[i] = JS_DupValue(ctx, s->data[i]);
         */
        var resolvingFunc = stackalloc JsValue[2];
        var func = (delegate* unmanaged<JsContext*, JsValue*, JsValue>)
            _ptrJsNewPromiseCapability.Value;
        var result = func(ctx, resolvingFunc);
        if (result.IsException())
            ThrowPendingException(ctx);
        resolve = new SafeJsValue(new AutoDropJsValue(resolvingFunc[0], ctx));
        reject = new SafeJsValue(new AutoDropJsValue(resolvingFunc[1], ctx));
        return new AutoDropJsValue(result, ctx);
    }

    private static readonly Lazy<nint> _ptrJsNewPromiseCapability = GetPointerLazy(
        "JS_NewPromiseCapability"
    );
    #endregion

    #region JS_Call
    /*
JSValue JS_Call(JSContext *ctx, JSValueConst func_obj, JSValueConst this_obj,
                int argc, JSValueConst *argv)
{
    return JS_CallInternal(ctx, func_obj, this_obj, JS_UNDEFINED,
                           argc, (JSValue *)argv, JS_CALL_FLAG_COPY_ARGV);
}

static JSValue JS_CallFree(JSContext *ctx, JSValue func_obj, JSValueConst this_obj,
                           int argc, JSValueConst *argv)
{
    JSValue res = JS_CallInternal(ctx, func_obj, this_obj, JS_UNDEFINED,
                                  argc, (JSValue *)argv, JS_CALL_FLAG_COPY_ARGV);
    JS_FreeValue(ctx, func_obj);
    return res;
}
     */
    public static AutoDropJsValue JS_Call(
        JsContext* ctx,
        JsValue func,
        JsValue thisObj,
        int argc,
        JsValue* argv
    )
    {
        var funcPtr = (delegate* unmanaged<JsContext*, JsValue, JsValue, int, JsValue*, JsValue>)
            _ptrJsCall.Value;
        var result = funcPtr(ctx, func, thisObj, argc, argv);
        if (result.IsException())
            ThrowPendingException(ctx);
        return new AutoDropJsValue(result, ctx);
    }

    private static readonly Lazy<nint> _ptrJsCall = GetPointerLazy("JS_Call");
    #endregion
    #region JS_NewArray
    public static AutoDropJsValue JS_NewArray(JsContext* ctx)
    {
        var func = (delegate* unmanaged<JsContext*, JsValue>)_ptrJsNewArray.Value;
        var result = func(ctx);
        if (result.IsException())
            ThrowPendingException(ctx);
        return new AutoDropJsValue(result, ctx);
    }

    private static readonly Lazy<nint> _ptrJsNewArray = GetPointerLazy("JS_NewArray");
    #endregion

    #region JS_ArraySpeciesCreate
    //   static JSValue JS_ArraySpeciesCreate(JSContext* ctx, JSValueConst obj,JSValueConst len_val)
    public static AutoDropJsValue JS_ArraySpeciesCreate(JsContext* ctx, JsValue obj, int len)
    {
        var func = (delegate* unmanaged<JsContext*, JsValue, JsValue, JsValue>)
            _ptrJsArraySpeciesCreate.Value;
        var lenVal = JsValueCreateHelper.NewInt32(len);
        var result = func(ctx, obj, lenVal);
        if (result.IsException())
            ThrowPendingException(ctx);
        return new AutoDropJsValue(result, ctx);
    }

    private static readonly Lazy<nint> _ptrJsArraySpeciesCreate = GetPointerLazy(
        "JS_ArraySpeciesCreate"
    );

    #endregion

    #region JS_GetPropertyUint32

    public static AutoDropJsValue JS_GetPropertyUint32(JsContext* ctx, JsValue thisObj, uint idx)
    {
        var func = (delegate* unmanaged<JsContext*, JsValue, uint, JsValue>)
            _ptrJsGetPropertyUint32.Value;
        var result = func(ctx, thisObj, idx);
        if (result.IsException())
            ThrowPendingException(ctx);
        return new AutoDropJsValue(result, ctx);
    }

    private static readonly Lazy<nint> _ptrJsGetPropertyUint32 = GetPointerLazy(
        "JS_GetPropertyUint32"
    );

    #endregion

    #region JS_SetPropertyValue
    // int JS_SetPropertyValue(JSContext* ctx, JSValueConst this_obj,JSValue prop, JSValue val, int flags)
    public static bool JS_SetPropertyValue(
        JsContext* ctx,
        JsValue thisObj,
        JsValue prop,
        JsValue val,
        JsPropertyFlags flags
    )
    {
        var func = (delegate* unmanaged<
            JsContext*,
            JsValue,
            JsValue,
            JsValue,
            JsPropertyFlags,
            int>)
            _ptrJsSetPropertyValue.Value;
        var result = func(ctx, thisObj, prop, val, flags);
        if (result == -1)
            ThrowPendingException(ctx);
        return result == 1;
    }

    private static readonly Lazy<nint> _ptrJsSetPropertyValue = GetPointerLazy(
        "JS_SetPropertyValue"
    );
    #endregion
    #region JS_SetPropertyUint32
    /*int JS_SetPropertyUint32(JSContext *ctx, JSValueConst this_obj,
                         uint32_t idx, JSValue val)*/
    public static bool JS_SetPropertyUint32(JsContext* ctx, JsValue thisObj, uint idx, JsValue val)
    {
        var func = (delegate* unmanaged<JsContext*, JsValue, uint, JsValue, int>)
            _ptrJsSetPropertyUint32.Value;
        var result = func(ctx, thisObj, idx, val);
        if (result == -1)
            ThrowPendingException(ctx);
        return result == 1;
    }

    private static readonly Lazy<nint> _ptrJsSetPropertyUint32 = GetPointerLazy(
        "JS_SetPropertyUint32"
    );
    #endregion
    #region JS_GetOwnPropertyNames
    /*int JS_GetOwnPropertyNames(JSContext *ctx, JSPropertyEnum **ptab,
                           uint32_t *plen, JSValueConst obj, int flags)*/
    public static string[] JS_GetOwnPropertyNames(JsContext* ctx, JsValue obj)
    {
        var func = (delegate* unmanaged<
            JsContext*,
            out JsPropertyEnum*,
            out uint,
            JsValue,
            int,
            int>)
            _ptrJsGetOwnPropertyNames.Value;
        var result = func(ctx, out var ptab, out var plen, obj, 0);
        if (result != 0)
            ThrowPendingException(ctx);
        try
        {
            var names = new string[plen];
            for (var i = 0; i < plen; i++)
            {
                var name = JS_AtomToCString(ctx, ptab[i].Atom);
                names[i] = name;
            }
            return names;
        }
        finally
        {
            //free ptab
            js_free_prop_enum(ctx, ptab, plen);
        }
    }

    private static readonly Lazy<nint> _ptrJsGetOwnPropertyNames = GetPointerLazy(
        "JS_GetOwnPropertyNames"
    );
    #endregion

    #region js_free_prop_enum

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void js_free_prop_enum(JsContext* ctx, JsPropertyEnum* ptab, uint plen)
    {
        //uint32_t i;
        //if (tab)
        //{
        //    for (i = 0; i < len; i++)
        //        JS_FreeAtom(ctx, tab[i].atom);
        //    js_free(ctx, tab);
        //}
        (
            (delegate* unmanaged<JsContext*, JsPropertyEnum*, uint, void>)_ptrJsFreePropEnum.Value
        )(ctx, ptab, plen);
    }

    private static Lazy<nint> _ptrJsFreePropEnum = GetPointerLazy("js_free_prop_enum");

    #endregion
    #region js_free
    //void js_free(JSContext *ctx, void *ptr)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void js_free(JsContext* ctx, void* ptr)
    {
        ((delegate* unmanaged<JsContext*, void*, void>)_ptrJsFree.Value)(ctx, ptr);
    }

    private static readonly Lazy<nint> _ptrJsFree = GetPointerLazy("js_free");
    #endregion

    #region js_ma
    /*
       tab_atom = js_malloc(ctx, sizeof(tab_atom[0]) * max_int(atom_count, 1));
    if (!tab_atom)
    {
        js_free_prop_enum(ctx, tab_exotic, exotic_count);
        return -1;
    }
     */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* js_malloc(JsContext* ctx, size_t size)
    {
        return ((delegate* unmanaged<JsContext*, size_t, void*>)_ptrJsMalloc.Value)(ctx, size);
    }

    private static readonly Lazy<nint> _ptrJsMalloc = GetPointerLazy("js_malloc");
    #endregion
    #region JS_NewClassID
    //JSClassID JS_NewClassID(JSClassID *pclass_id);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsClassId JS_NewClassID(JsRuntime* rt)
    {
        JsClassId classId = new() { Id = 0 };

#if DEBUG
        var result =
#endif

        (
            (delegate* unmanaged<JsRuntime*, JsClassId*, JsClassId>)_ptrJsNewClassId.Value
        )(rt, &classId);
#if DEBUG
        if (result != classId)
        {
            throw new InvalidOperationException("JS_NewClassID failed");
        }
#endif
        return classId;
    }

    private static readonly Lazy<nint> _ptrJsNewClassId = GetPointerLazy("JS_NewClassID");
    #endregion
    #region JS_IsRegisteredClass

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool JS_IsRegisteredClass(JsRuntime* ctx, JsClassId classId)
    {
        return ((delegate* unmanaged<JsRuntime*, JsClassId, int>)_ptrJsIsRegisteredClass.Value)(
                ctx,
                classId
            ) != 0;
    }

    private static readonly Lazy<nint> _ptrJsIsRegisteredClass = GetPointerLazy(
        "JS_IsRegisteredClass"
    );
    #endregion
    #region JS_GetRuntime

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsRuntime* JS_GetRuntime(JsContext* ctx)
    {
        //or ctx->rt;
        return ((delegate* unmanaged<JsContext*, JsRuntime*>)_ptrJsGetRuntime.Value)(ctx);
    }

    private static readonly Lazy<nint> _ptrJsGetRuntime = GetPointerLazy("JS_GetRuntime");
    #endregion
    #region JS_NewClass

    //int JS_NewClass(JSRuntime *rt, JSClassID class_id, const JSClassDef *class_def)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void JS_NewClass(JsRuntime* rt, JsClassId classId, JsClassDef* classDef)
    {
        var result = (
            (delegate* unmanaged<JsRuntime*, JsClassId, JsClassDef*, int>)_ptrJsNewClass.Value
        )(rt, classId, classDef);
        if (result != 0)
            throw new Exception("JS_NewClass failed");
    }

    private static readonly Lazy<nint> _ptrJsNewClass = GetPointerLazy("JS_NewClass");

    #endregion
    #region JS_NewObjectProtoClass
    //JSValue JS_NewObjectProtoClass(JSContext *ctx, JSValueConst proto, JSClassID class_id)


    #endregion

    #region JS_NewObjectClass

    //  JSValue JS_NewObjectClass(JSContext* ctx, int class_id)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AutoDropJsValue JS_NewObjectClass(JsContext* ctx, JsClassId classId)
    {
        var result = (
            (delegate* unmanaged<JsContext*, JsClassId, JsValue>)_ptrJsNewObjectClass.Value
        )(ctx, classId);
        if (result.IsException())
            ThrowPendingException(ctx);
        return new AutoDropJsValue(result, ctx);
    }

    private static readonly Lazy<nint> _ptrJsNewObjectClass = GetPointerLazy("JS_NewObjectClass");

    #endregion
    #region JS_SetClassProto
    //void JS_SetClassProto(JSContext *ctx, JSClassID class_id, JSValue obj)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void JS_SetClassProto(JsContext* ctx, JsClassId classId, JsValue obj)
    {
        ((delegate* unmanaged<JsContext*, JsClassId, JsValue, void>)_ptrJsSetClassProto.Value)(
            ctx,
            classId,
            obj
        );
    }

    private static readonly Lazy<nint> _ptrJsSetClassProto = GetPointerLazy("JS_SetClassProto");
    #endregion
    #region JS_GetClassProto
    //JSValue JS_GetClassProto(JSContext* ctx, JSClassID class_id)
    //{
    //    JSRuntime* rt = ctx->rt;
    //    assert(class_id < rt->class_count);
    //    return JS_DupValue(ctx, ctx->class_proto[class_id]);
    //}
    public static AutoDropJsValue JS_GetClassProto(JsContext* ctx, JsClassId classId)
    {
        var func = (delegate* unmanaged<JsContext*, JsClassId, JsValue>)_ptrJsGetClassProto.Value;
        var result = func(ctx, classId);
        if (result.IsException())
        {
            ThrowPendingException(ctx);
        }
        //it use JS_DupValue so need to free
        return new AutoDropJsValue(result, ctx);
    }

    private static readonly Lazy<nint> _ptrJsGetClassProto = GetPointerLazy("JS_GetClassProto");
    #endregion

    #region js_object___getClass
    /*
     static JSValue js_object___getClass(JSContext *ctx, JSValueConst this_val,
                                    int argc, JSValueConst *argv)
     */
    /* return an empty string if not an object */
    public static string JS_GetClassName(JsContext* ctx, JsValue value)
    {
        //var func = (delegate* unmanaged<JsContext*, JsValue, int, JsValue*, JsValue>)
        //    _ptrJsGetClassName.Value;
        //var thisObj = JsValueCreateHelper.Undefined;
        //var result = func(ctx, thisObj, 1, &value);
        //if (result.IsException())
        //    ThrowPendingException(ctx);
        //using var autoDrop = new AutoDropJsValue(result, ctx);
        //return autoDrop.ToString();
        if (value.IsObject())
        {
            using var constructor = value.GetProperty(ctx, "constructor");
            if (constructor.Value.IsObject())
                return constructor.GetStringProperty("name");
        }
        return string.Empty;
    }

    //private static readonly Lazy<nint> _ptrJsGetClassName = GetPointerLazy("js_object___getClass");

    #endregion

    #region JS_GetOpaqueWithoutClass

    public static nint JS_GetOpaqueWithoutClass(JsValue val)
    {
        return ((delegate* unmanaged<JsValue, nint>)_ptrJsGetOpaqueWithoutClass.Value)(val);
    }

    private static readonly Lazy<nint> _ptrJsGetOpaqueWithoutClass = GetPointerLazy(
        "JS_GetOpaqueWithoutClass"
    );
    #endregion
    #region JS_GetOpaque
    public static nint JS_GetOpaque(JsValue val, JsClassId id)
    {
        return ((delegate* unmanaged<JsValue, JsClassId, nint>)_ptrJsGetOpaque.Value)(val, id);
    }

    private static readonly Lazy<nint> _ptrJsGetOpaque = GetPointerLazy("JS_GetOpaque");
    #endregion
    #region JS_GetOpaque2

    public static nint JS_GetOpaque2(JsContext* ctx, JsValue val, JsClassId id)
    {
        return ((delegate* unmanaged<JsContext*, JsValue, JsClassId, nint>)_ptrJsGetOpaque2.Value)(
            ctx,
            val,
            id
        );
    }

    private static readonly Lazy<nint> _ptrJsGetOpaque2 = GetPointerLazy("JS_GetOpaque2");
    #endregion
    #region JS_SetOpaque

    public static void JS_SetOpaque(JsValue val, nint opaque)
    {
        ((delegate* unmanaged<JsValue, nint, void>)_ptrJsSetOpaque.Value)(val, opaque);
    }

    private static readonly Lazy<nint> _ptrJsSetOpaque = GetPointerLazy("JS_SetOpaque");
    #endregion
}
