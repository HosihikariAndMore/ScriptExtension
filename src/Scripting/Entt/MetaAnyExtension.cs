using System.Diagnostics.CodeAnalysis;
using Hosihikari.NativeInterop;
using Hosihikari.VanillaScript.Scripting.ScriptModuleMinecraft;

namespace Hosihikari.VanillaScript.Scripting.Entt;

internal static class MetaAnyExtension
{
    //no const
    //_ZNK4entt8meta_any8try_castIN21ScriptModuleMinecraft12ScriptPlayerEEEPKT_v
    //cosnt
    //_ZN4entt8meta_any8try_castIN21ScriptModuleMinecraft12ScriptPlayerEEEPT_v
    private static Dictionary<(Type type, bool isConst), nint> _symbolCache = new();

    private static unsafe delegate* unmanaged<void*, void*> GetPointer<T>(bool isConst)
    {
        var type = typeof(T);
        var name = type.Name;
        var key = (type, isConst);

        if (!_symbolCache.TryGetValue(key, out var cache))
        {
            cache = SymbolHelper.Dlsym(
                isConst
                    ? $"_ZN4entt8meta_any8try_castIN21ScriptModuleMinecraft12{name}EEEPT_v"
                    : $"_ZNK4entt8meta_any8try_castIN21ScriptModuleMinecraft12{name}EEEPKT_v"
            );
            _symbolCache.Add(key, cache);
        }
        return (delegate* unmanaged<void*, void*>)cache;
    }

    public static bool TryCast<T>(
        this MetaAny @this,
        [NotNullWhen(true)] out T? player,
        bool isConst = false
    )
        where T : ScriptObjectBase, new()
    {
        unsafe
        {
            var func = GetPointer<T>(isConst);
            var result = func(@this);
            if (result is not null)
            {
                player = new T { Ptr = result };
                return true;
            }

            player = null;
            return false;
        }
    }

    //public bool TryGetConst<T>(out T player)
    //    where T : ScriptObjectBase { }
}
