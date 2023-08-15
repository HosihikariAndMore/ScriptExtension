using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper.ClrProxy;

public abstract class ClrFunctionProxyBase : ClrProxyBase
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
        throw new InvalidOperationException(nameof(DefineOwnProperty));
    }

    protected override JsPropertyEnum[] GetOwnPropertyNames(JsContextWrapper ctxInstance)
    {
        throw new InvalidOperationException(nameof(GetOwnPropertyNames));
    }

    protected override bool DeleteProperty(JsContextWrapper ctxInstance, JsValue @this, JsAtom prop)
    {
        throw new InvalidOperationException(nameof(DeleteProperty));
    }

    protected override bool GetOwnProperty(
        JsContextWrapper ctxInstance,
        out JsPropertyDescriptor data,
        JsAtom propName
    )
    {
        throw new InvalidOperationException(nameof(GetOwnProperty));
    }
}
