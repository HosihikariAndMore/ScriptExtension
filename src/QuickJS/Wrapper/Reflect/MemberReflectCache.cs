using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper.Reflect;

public class MemberReflectCache
{
    private Dictionary<
        (PropertyInfo, object[]?),
        (MethodHelper? get, MethodHelper? set)
    >? _propCache = null;
    private Dictionary<MethodInfo, MethodHelper>? _methodCache = null;

    public void AddPropHelperCache(
        PropertyInfo property,
        MethodHelper? get,
        MethodHelper? set,
        object[]? indexer = null
    ) => (_propCache ??= new()).Add((property, indexer), (get, set));

    public bool GetPropHelperCache(
        PropertyInfo property,
        out MethodHelper? get,
        out MethodHelper? set,
        object[]? indexer = null
    )
    {
        if (_propCache is not null)
        {
            if (_propCache.TryGetValue((property, indexer), out var helper))
            {
                get = helper.get;
                set = helper.set;
                return true;
            }
        }
        get = set = null;
        return false;
    }

    public void AddMethodHelperCache(MethodInfo property, MethodHelper method) =>
        (_methodCache ??= new()).Add(property, method);

    public bool GetMethodHelperCache(
        MethodInfo property,
        [NotNullWhen(true)] out MethodHelper? method
    )
    {
        if (_methodCache is not null && _methodCache.TryGetValue(property, out var helper))
        {
            method = helper;
            return true;
        }
        method = null;
        return false;
    }
}
