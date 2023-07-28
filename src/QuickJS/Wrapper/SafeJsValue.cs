using Hosihikari.VanillaScript.QuickJS.Extensions;
using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper;

/// <summary>
/// auto add and remove refCount
/// </summary>
public class SafeJsValue
{
    private JsValue _value;
    private readonly unsafe JsContext* _context;

    public unsafe SafeJsValue(JsValue value, JsContext* context)
    {
        _value = value;
        this._context = context;
    }

    ~SafeJsValue()
    {
        unsafe
        {
            _value.UnsafeRemoveRefCount(_context);
        }
    }

    public static explicit operator JsValue(SafeJsValue @this) => @this._value;

    public ref JsValue Value => ref _value;
}
