namespace Hosihikari.VanillaScript.Scripting.Entt;

//entt::resolve<ScriptModuleMinecraft::*
public static unsafe class Resolve
{
    #region ScriptPlayer
    //_ZN4entt7resolveIN21ScriptModuleMinecraft12ScriptPlayerEEENS_9meta_typeEv
    //entt::meta_type entt::resolve<ScriptModuleMinecraft::ScriptPlayer>(__int64 a1)
    public static MetaType ResolveScriptPlayer()
    {
        MetaType instance = new();
        ((delegate* unmanaged<void*, void*>)_ptrResolveScriptPlayer.Value)(instance);
        return instance;
    }

    private static Lazy<nint> _ptrResolveScriptPlayer = McQuickJs.GetPointer(
        "_ZN4entt7resolveIN21ScriptModuleMinecraft12ScriptPlayerEEENS_9meta_typeEv"
    );
    #endregion
}
