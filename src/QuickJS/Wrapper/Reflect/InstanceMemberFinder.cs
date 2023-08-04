using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper.Reflect;

public class InstanceMemberFinder
{
    private readonly Type _type;
    private readonly Dictionary<string, MemberInfo> _membersCache = new();

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

    public bool TryFindMember(string name, [NotNullWhen(true)] out MemberInfo? result)
    {
        if (_membersCache.TryGetValue(name, out var memberResult))
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