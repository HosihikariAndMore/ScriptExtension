using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper.Reflect;

public class StaticMemberFinder
{
    private readonly Dictionary<string, MemberInfo> _membersCache = new();
    private readonly Type _type;

    public StaticMemberFinder(Type type)
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

    public IEnumerable<MemberInfo> EnumStaticMembers()
    {
        foreach (var member in _type.GetMembers(BindingFlags.Public | BindingFlags.Static))
        {
            yield return member;
        }
    }
}
