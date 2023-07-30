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

    public unsafe AutoDropJsValue(JsValue value, JsContext* context)
    {
        _value = value;
        _context = context;
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

    //bool _disposed = false;
    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this); //prevent call ~SafeJsValue()
    }

    ~AutoDropJsValue()
    {
        ReleaseUnmanagedResources();
    }

    private void ReleaseUnmanagedResources()
    {
        unsafe
        {
            //if (_disposed)
            //    return;
            //_disposed = true;
            //todo it seem necessary to post tick to main thread when freeing value
            //ref to JS_FreeAtomStruct, it finally change an array in JSRuntime,
            //so if called in GC thread and call by other in the same time, it might make the array broken ?
            _value.UnsafeRemoveRefCount(_context);
            _value = default; //default int value, no longer use and prevent call __JS_FreeValue
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
}
