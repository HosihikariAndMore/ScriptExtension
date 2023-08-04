//using Hosihikari.VanillaScript.QuickJS.Extensions;
//using Hosihikari.VanillaScript.QuickJS.Types;

//namespace Hosihikari.VanillaScript.QuickJS.Wrapper;

using Hosihikari.VanillaScript.QuickJS.Extensions;
using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper;

public class SafeJsValue
{
    private JsValue _value;
    private readonly JsContextWrapper _ctx;
    public JsContextWrapper Context
    {
        get
        {
            _ctx.ThrowIfFree();
            return _ctx;
        }
    }

    public SafeJsValue(JsValue value, nint ctx)
    {
        if (JsContextWrapper.TryGet(ctx, out var tCtx))
        {
            _ctx = tCtx;
        }
        else
            throw new ArgumentException("context not found");
        _value = value;
        if (value.HasRefCount())
        {
            value.UnsafeAddRefCount();
            _ctx.FreeContextCallback += FreeThis;
        }
    }

    public SafeJsValue(AutoDropJsValue value)
    {
        //steal the value to prevent free, then the value will be memory safe in managed environment
        _value = value.Steal();
        _ctx = value.Context;
        if (value.Value.HasRefCount())
            _ctx.FreeContextCallback += FreeThis;
    }

    public ref JsValue Instance => ref _value;

    /// <summary>
    /// Please call this if you think the value is no longer used. It is safe most case, just call it for free memory.
    /// Even if call twice, it won't take effect because the value is reset to zero in the first call, so never cause memory corruption.
    /// </summary>
    internal void FreeThis()
    {
        //remove ref count to free the value
        //this was originally done by AutoDropJsValue.Dispose()
        //but we steal the value from AutoDropJsValue, so we need to free it manually here
        unsafe
        {
            _value.UnsafeRemoveRefCount(_ctx.Context);
            _value = default;
            _ctx.FreeContextCallback -= FreeThis;
        }
    }

    ~SafeJsValue() => FreeThis();
}
