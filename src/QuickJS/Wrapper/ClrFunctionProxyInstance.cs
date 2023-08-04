using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper;

public class ClrFunctionProxyInstance : ClrFunctionProxyBase
{
    public ClrFunctionProxyInstance(JsNativeFunctionDelegate callback)
    {
        Callback = callback;
    }

    public JsNativeFunctionDelegate Callback;

    protected override JsValue Invoke(
        JsContextWrapper ctxInstance,
        JsValue contextThis,
        ReadOnlySpan<JsValue> argv,
        JsCallFlag flags
    )
    {
        //check has JsCallFlag.Constructor
        if (!Enum.IsDefined(typeof(JsCallFlag), flags))
        {
            return ctxInstance.ThrowJsError(
                new NotImplementedException($"operation JsCallFlag {flags} unknown")
            );
        }

        if ((flags & JsCallFlag.Constructor) != 0)
        {
            return ctxInstance.ThrowJsError(
                new InvalidOperationException("invalid constructor call")
            );
        }
        return Callback(ctxInstance, contextThis, argv);
    }

    public override string ToString()
    {
        return $"[ClrFunctionProxy {Callback.Method.Name}]";
    }
}
