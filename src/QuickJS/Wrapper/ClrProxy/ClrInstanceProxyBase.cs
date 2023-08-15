using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper.ClrProxy;

public abstract class ClrInstanceProxyBase : ClrProxyBase
{
    protected override JsValue Invoke(
        JsContextWrapper ctxInstance,
        JsValue contextThis,
        ReadOnlySpan<JsValue> argv,
        JsCallFlag flags
    )
    {
        return ctxInstance.ThrowJsError(new NotImplementedException(nameof(Invoke)));
    }

    protected override bool GetOwnProperty(
        JsContextWrapper ctxInstance,
        out JsPropertyDescriptor data,
        JsAtom propName
    )
    {
        data = default;
        return false;
    }

    protected override JsPropertyEnum[] GetOwnPropertyNames(JsContextWrapper ctxInstance)
    {
        return Array.Empty<JsPropertyEnum>();
    }

    protected override bool DeleteProperty(JsContextWrapper ctxInstance, JsValue @this, JsAtom prop)
    {
        return false;
    }

    protected override bool DefineOwnProperty(
        JsContextWrapper ctxInstance,
        JsValue @this,
        JsAtom prop,
        JsValue val,
        JsValue getter,
        JsValue setter,
        JsPropertyFlags flags
    )
    {
        ctxInstance.ThrowJsError(new NotImplementedException(nameof(DefineOwnProperty)));
        return false;
    }
}
