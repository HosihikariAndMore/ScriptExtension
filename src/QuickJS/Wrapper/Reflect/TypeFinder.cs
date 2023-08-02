using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper.Reflect;

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

public class TypeFinder
{
    private readonly Dictionary<string, Type> _typesCache = new();
    private readonly Assembly _assembly;

    public static IEnumerable<Assembly> EnumAllAssemblies()
    {
        return AppDomain.CurrentDomain.GetAssemblies();
    }

    public static IEnumerable<Type> EnumAllType(string assemblyName)
    {
        return EnumAllAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name == assemblyName)
                ?.GetExportedTypes() ?? Enumerable.Empty<Type>();
    }

    public TypeFinder(string assemblyName)
    {
        _assembly =
            EnumAllAssemblies().FirstOrDefault(assembly => assembly.GetName().Name == assemblyName)
            ?? Assembly.Load(assemblyName);
    }

    public bool TryFindType(string type, [NotNullWhen(true)] out Type? result)
    {
        if (_typesCache.TryGetValue(type, out var typeResult))
        {
            result = typeResult;
            return true;
        }

        typeResult = _assembly.GetType(type);
        if (typeResult is null)
        {
            result = null;
            return false;
        }
        _typesCache.Add(type, typeResult);
        result = typeResult;
        return true;
    }

    public Type FindType(string type)
    {
        if (TryFindType(type, out var result))
        {
            return result;
        }
        throw new ArgumentException($"Type {type} not found in assembly {_assembly.FullName}");
    }
}
