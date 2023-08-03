using System.Diagnostics.CodeAnalysis;
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

public class StaticFunctionFinder
{
    private readonly Dictionary<string, MemberInfo> _membersCache = new();
    private readonly Type _type;

    public StaticFunctionFinder(Type type)
    {
        _type = type;
    }

    public bool TryFindMember(string name, [NotNullWhen(true)] out MemberInfo? result)
    {
        if (_membersCache.TryGetValue(name, out var memberResult))
        {
            result = memberResult;
            return true;
        }
        memberResult = _type
            .GetMember(name, BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault();
        if (memberResult is null)
        {
            result = null;
            return false;
        }
        _membersCache.Add(name, memberResult);
        result = memberResult;
        return true;
    }

    public IEnumerable<MemberInfo> EnumStaticMembers()
    {
        foreach (var member in _type.GetMembers(BindingFlags.Public | BindingFlags.Static))
        {
            yield return member;
        }
    }
}
