using Hosihikari.VanillaScript.QuickJS.Types;
using System.Runtime.CompilerServices;
using Hosihikari.VanillaScript.QuickJS.Extensions;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper;

public class AutoDropJsAtom : IDisposable
{
    private JsAtom _value;
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

    public unsafe AutoDropJsAtom(JsAtom value, JsContext* context)
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
    public JsAtom Steal()
    {
        var ret = _value;
        _value = default;
        return ret;
    }

    public void Dispose()
    {
        FreeThis();
        GC.SuppressFinalize(this); //prevent call ~SafeJsAtom()
    }

    ~AutoDropJsAtom()
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
            _value.UnsafeRemoveRefCount(_context);
            _value = default;
        }
    }

    public static explicit operator JsAtom(AutoDropJsAtom @this) => @this._value;

    public ref JsAtom Value => ref _value;

    public override string ToString()
    {
        unsafe
        {
            return _value.ToString(_context);
        }
    }
}
