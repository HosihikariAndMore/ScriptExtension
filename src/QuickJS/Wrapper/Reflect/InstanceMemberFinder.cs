using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper.Reflect;

public class InstanceMemberFinder
{
    private readonly Type _type;
    private Dictionary<string, MemberInfo>? _membersCache = null;
    private Dictionary<int, PropertyInfo?>? _indexerCache = null;

    public InstanceMemberFinder(Type type)
    {
        _type = type;
    }

    public IEnumerable<MemberInfo> EnumMembers()
    {
        foreach (var member in _type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
        {
            yield return member;
        }
    }

    public bool TryGetIndexer(
        [NotNullWhen(true)] out PropertyInfo? result,
        int indexParameterCount = 1 //default one dimension index, such as array[1]
    ) //todo support multi dimension index, such as array[1,2]
    {
        if ((_indexerCache ??= new()).TryGetValue(indexParameterCount, out var indexerResult))
        {
            result = indexerResult;
            return result is not null;
        }
        result = _type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => p.GetIndexParameters().Length == indexParameterCount);
        _indexerCache.Add(indexParameterCount, result);
        return result is not null;
    }

    public bool TryFindMember(string name, [NotNullWhen(true)] out MemberInfo? result)
    {
        if ((_membersCache ??= new()).TryGetValue(name, out var memberResult))
        {
            result = memberResult;
            return true;
        }
        memberResult = _type
            .GetMember(name, BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(); //todo how to overload method with same name?
        if (memberResult is null)
        {
            result = null;
            return false;
        }
        _membersCache.Add(name, memberResult);
        result = memberResult;
        return true;
    }
}
