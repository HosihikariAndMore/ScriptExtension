using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper.Reflect;

public class TypeConstructorFinder
{
    private readonly Dictionary<Type[], ConstructorInfo> _constructorsCache = new();
    private readonly Type _type;

    public TypeConstructorFinder(Type type)
    {
        _type = type;
    }

    public bool TryFindConstructor(
        Type[] argumentTypes,
        [NotNullWhen(true)] out ConstructorInfo? result
    )
    {
        if (_constructorsCache.TryGetValue(argumentTypes, out var constructorResult))
        {
            result = constructorResult;
            return true;
        }
        constructorResult = _type.GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            argumentTypes,
            null
        );
        if (constructorResult is null)
        {
            result = null;
            return false;
        }
        _constructorsCache.Add(argumentTypes, constructorResult);
        result = constructorResult;
        return true;
    }
}
