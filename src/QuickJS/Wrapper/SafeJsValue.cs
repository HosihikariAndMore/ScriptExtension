//using Hosihikari.VanillaScript.QuickJS.Extensions;
//using Hosihikari.VanillaScript.QuickJS.Types;

//namespace Hosihikari.VanillaScript.QuickJS.Wrapper;

using Hosihikari.VanillaScript.QuickJS.Extensions;
using Hosihikari.VanillaScript.QuickJS.Types;
using Hosihikari.VanillaScript.QuickJS.Wrapper;

public class SafeJsValue
{
    private JsValue _value;
    private unsafe JsContext* _ctx;

    public SafeJsValue(AutoDropJsValue value)
    {
        //steal the value to prevent free, then the value will be memory safe in managed environment
        _value = value.Steal();
    }

    public ref JsValue Instance => ref _value;

    ~SafeJsValue()
    {
        //remove ref count to free the value
        unsafe
        {
            _value.UnsafeRemoveRefCount(_ctx);
        }
    }
}
