using System.Reflection;
using Hosihikari.VanillaScript.QuickJS.Extensions;
using Hosihikari.VanillaScript.QuickJS.Extensions.Check;
using Hosihikari.VanillaScript.QuickJS.Helper;
using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper.Reflect;

public class MethodHelper
{
    private readonly MethodInfo _method;
    private readonly Lazy<ParameterInfo[]> _parametersCache = new();
    private object? _instance;

    public MethodHelper(MethodInfo method, object? instance = null)
    {
        _method = method;
        _instance = instance;
        _parametersCache = new Lazy<ParameterInfo[]>(_method.GetParameters);
    }

    public AutoDropJsValue Call(JsContextWrapper ctx, ReadOnlySpan<JsValue> argv)
    {
        try
        {
            var parameters = _parametersCache.Value;
            argv.InsureArgumentCount(parameters.Length);
            var args = new object?[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (parameter.ParameterType == typeof(JsValue))
                {
                    args[i] = argv[i];
                }
                else
                {
                    args[i] = argv[i].ToClrObject(ctx, parameter.ParameterType);
                }
            }
            var result = _method.Invoke(_instance, args);
            return JsValueCreateHelper.New(result, ctx);
        }
        catch (Exception ex)
        {
            return new AutoDropJsValue(ctx.ThrowJsError(ex), ctx);
        }
    }
}