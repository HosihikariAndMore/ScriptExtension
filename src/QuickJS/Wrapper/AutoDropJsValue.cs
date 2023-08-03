using Hosihikari.VanillaScript.QuickJS.Extensions;
using Hosihikari.VanillaScript.QuickJS.Types;
using System.Runtime.CompilerServices;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper;

/// <summary>
/// auto add and remove refCount
/// </summary>
public class AutoDropJsValue : IDisposable
{
    private JsValue _value;
    private readonly unsafe JsContext* _context;
    public JsContextWrapper Context
    {
        get
        {
            unsafe
            {
                return JsContextWrapper.FetchOrCreate(_context);
            }
        }
    }

    public unsafe AutoDropJsValue(JsValue value, JsContextWrapper context)
        : this(value, context.Context) { }

    public unsafe AutoDropJsValue(JsValue value, JsContext* context)
    {
        _value = value;
        _context = context;
        if (JsContextWrapper.TryGet((nint)context, out var tCtx))
        {
            tCtx.FreeContextCallback += FreeThis;
        }
    }

    /// <summary>
    /// only use to pass the value to JS callback such as JS_NewCFunction
    /// this method can't be called twice
    /// the purpose of this method is to prevent the value from being freed, and pass to JS callback then will auto free by JS engine
    /// </summary>
    /// <returns></returns>
    public JsValue Steal()
    {
        var ret = _value;
        _value = default;
        return ret;
    }

    public void Dispose()
    {
        FreeThis();
        GC.SuppressFinalize(this); //prevent call ~SafeJsValue()
    }

    ~AutoDropJsValue()
    {
        FreeThis();
    }

    //bool _disposed = false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FreeThis()
    {
        unsafe
        {
            if (JsContextWrapper.TryGet((nint)_context, out var tCtx))
                tCtx.FreeContextCallback -= FreeThis;
#if DEBUG
            //var stack = Environment.StackTrace;
            //LevelTick.PostTick(() =>
            //{
            //Log.Logger.Trace(stack);
            _value.UnsafeRemoveRefCount(_context);
            _value = default;
            //});
#else
            //todo it seem necessary to post tick to main thread when freeing value
            //ref to JS_FreeAtomStruct, it finally change an array in JSRuntime,
            //so if called in GC thread and call by other in the same time, it might make the array broken ?
            _value.UnsafeRemoveRefCount(_context);
            _value = default; //default int value, no longer use and prevent call __JS_FreeValue
#endif
        }
    }

    public static explicit operator JsValue(AutoDropJsValue @this) => @this._value;

    public ref JsValue Value => ref _value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe bool IsError() => Native.JS_IsError(_context, _value);

    public string GetStringProperty(string propertyName)
    {
        unsafe
        {
            return _value.GetStringProperty(_context, propertyName);
        }
    }

    public override string ToString()
    {
        unsafe
        {
            return _value.ToString(_context);
        }
    }

    public string ToJson()
    {
        unsafe
        {
            return _value.ToString(_context);
        }
    }

    public void DefineProperty(
        JsContextWrapper ctx,
        string exists,
        AutoDropJsValue value,
        JsPropertyFlags flags = JsPropertyFlags.Normal
    )
    {
        unsafe
        {
            _value.DefineProperty(ctx.Context, exists, value.Steal(), flags);
        }
    }

    public unsafe void DefineFunction(
        JsContextWrapper ctx,
        string funcName,
        int argumentLength,
        delegate* unmanaged<JsContext*, JsValue, int, JsValue*, JsValue> func,
        JsCFunctionEnum protoType = JsCFunctionEnum.Generic,
        JsPropertyFlags propFlags = JsPropertyFlags.Normal
    )
    {
        _value.DefineFunction(ctx.Context, funcName, func, argumentLength, protoType, 0, propFlags);
    }

    public unsafe void DefineFunction(
        JsContextWrapper ctx,
        string funcName,
        int argumentLength,
        JsNativeFunctionDelegate func,
        JsCFunctionEnum protoType = JsCFunctionEnum.Generic,
        JsPropertyFlags propFlags = JsPropertyFlags.Normal
    )
    {
        using var value = ctx.NewJsFunction(funcName, argumentLength, func, protoType);
        _value.DefineProperty(ctx.Context, funcName, value.Steal(), propFlags);
    }
}
