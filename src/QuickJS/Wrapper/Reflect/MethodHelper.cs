using System.Reflection;
using Hosihikari.VanillaScript.QuickJS.Extensions;
using Hosihikari.VanillaScript.QuickJS.Extensions.Check;
using Hosihikari.VanillaScript.QuickJS.Helper;
using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper.Reflect;

public class IndexerMethodHelper : MethodHelper
{
    private readonly object[] _indexer;

    public override AutoDropJsValue Call(JsContextWrapper ctx, ReadOnlySpan<JsValue> argv)
    {
        try
        {
            var parameters = _parametersCache.Value;
            var reserve = _indexer.Length;
            argv.InsureArgumentCount(parameters.Length - reserve);
            var argvClr = new object?[parameters.Length];
            Array.Copy(_indexer, argvClr, reserve);
            for (var i = reserve; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (parameter.ParameterType == typeof(JsValue))
                {
                    argvClr[i] = argv[i - reserve];
                }
                else
                {
                    argvClr[i] = argv[i - reserve].ToClrObject(ctx, parameter.ParameterType);
                }
            }
            var result = _method.Invoke(_instance, argvClr);
            return JsValueCreateHelper.New(result, ctx);
        }
        catch (Exception ex)
        {
            return new AutoDropJsValue(ctx.ThrowJsError(ex), ctx);
        }
    }

    public IndexerMethodHelper(MethodInfo method, object? instance, object[] indexer)
        : base(method, instance)
    {
        _indexer = new object[indexer.Length];
        var parameters = _parametersCache.Value;
        for (var i = 0; i < _indexer.Length; i++)
        {
            var parameter = parameters[i];
            _indexer[i] =
                parameter.ParameterType != indexer[i].GetType()
                    ? Convert.ChangeType(indexer[i], parameter.ParameterType)
                    : indexer[i];
        }
    }
}

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

    public virtual AutoDropJsValue Call(JsContextWrapper ctx, ReadOnlySpan<JsValue> argv)
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
            return JsValueCreateHelper.New(result, ctx);
        }
        catch (Exception ex)
        {
            return new AutoDropJsValue(ctx.ThrowJsError(ex), ctx);
        }
    }
}
