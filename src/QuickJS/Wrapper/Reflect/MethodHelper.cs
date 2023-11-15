using System.Reflection;
using Hosihikari.ScriptExtension.QuickJS.Extensions;
using Hosihikari.ScriptExtension.QuickJS.Extensions.Check;
using Hosihikari.ScriptExtension.QuickJS.Helper;
using Hosihikari.ScriptExtension.QuickJS.Types;

namespace Hosihikari.ScriptExtension.QuickJS.Wrapper.Reflect;

public class MethodHelper
{
    private protected readonly MethodInfo _method;
    private protected readonly Lazy<ParameterInfo[]> _parametersCache = new();
    private protected readonly object? _instance;

    public MethodHelper(MethodInfo method, object? instance = null)
    {
        _method = method;
        _instance = instance;
        _parametersCache = new Lazy<ParameterInfo[]>(_method.GetParameters);
    }

    public virtual AutoDropJsValue Invoke(
        JsContextWrapper ctx,
        ReadOnlySpan<JsValue> argv,
        JsValue thisObj
    )
    {
        try
        {
            var parameters = _parametersCache.Value;
            argv.InsureArgumentCount(parameters.Length);
            var argvClr = new object?[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (parameter.ParameterType == typeof(JsValue))
                {
                    argvClr[i] = argv[i];
                }
                else
                {
                    argvClr[i] = argv[i].ToClrObject(ctx, parameter.ParameterType);
                }
            }
            var result = _method.Invoke(_instance, argvClr);
            return JsValueCreateHelper.New(result, ctx, thisObj);
        }
        catch (Exception ex)
        {
            return new AutoDropJsValue(ctx.ThrowJsError(ex), ctx);
        }
    }
}
