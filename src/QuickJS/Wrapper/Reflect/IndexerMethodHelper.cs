using System.Reflection;
using Hosihikari.ScriptExtension.QuickJS.Extensions;
using Hosihikari.ScriptExtension.QuickJS.Extensions.Check;
using Hosihikari.ScriptExtension.QuickJS.Helper;
using Hosihikari.ScriptExtension.QuickJS.Types;

namespace Hosihikari.ScriptExtension.QuickJS.Wrapper.Reflect;

public class IndexerMethodHelper : MethodHelper
{
    private readonly object[] _indexer;

    public override AutoDropJsValue Invoke(
        JsContextWrapper ctx,
        ReadOnlySpan<JsValue> argv,
        JsValue thisObj
    )
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
            return JsValueCreateHelper.New(result, ctx, thisObj);
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