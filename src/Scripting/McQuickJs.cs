using System.Diagnostics.CodeAnalysis;
using Hosihikari.Minecraft;
using Hosihikari.NativeInterop;
using Hosihikari.VanillaScript.QuickJS.Types;
using Hosihikari.VanillaScript.Scripting.Entt;
using Hosihikari.VanillaScript.Scripting.ScriptModuleMinecraft;
using Hosihikari.VanillaScript.Scripting.Std;

namespace Hosihikari.VanillaScript.Scripting;

public static unsafe class McQuickJs
{
    internal static Lazy<nint> GetPointer(string symbol) => SymbolHelper.DlsymLazy(symbol);

    public static bool PlayerToJsValue(Player player, JsContext* ctx, out JsValue value)
    {
        unsafe
        {
            //void* ptr = player.Pointer;
            //var ctxd = (JsContextData*)ctx;
            //Scripting::LifetimeRegistry & reg = ctxd->getLifetimeRegistry();
            //Scripting::ObjectHandle handle(ptr);
        }
        value = default;
        return false;
    }

    public static bool JsValueToPlayer(
        JsContext* ctx,
        JsValue value,
        [NotNullWhen(true)] out Player? player
    )
    {
        using var scriptPlayerType = Resolve.ResolveScriptPlayer();
        using var variant = JSValueToNativeAny(ctx, value, scriptPlayerType);
        if (variant.MoveValue(out var metaAny)) //call std::get<entt::meta_any>(any)
        {
            try
            {
                //call meta_any::try_cast<ScriptModuleMinecraft::ScriptPlayer>();
                if (metaAny.TryCast<ScriptPlayer>(out var scriptPlayer))
                {
                    //Player* ScriptModuleMinecraft::ScriptPlayer::tryGetPlayer(void) const;
                    return scriptPlayer.TryGetPlayer(out player);
                }
            }
            finally
            {
                //free
                metaAny.Dispose();
            }
        }
        player = null;
        return false;
    }

    //std::variant<entt::meta_any,JSValue> JSValueToNativeAny(JSContext*, JSValue, entt::meta_type);
    public static StdVariantOfEnttMetaAnyAndJsValue JSValueToNativeAny(
        JsContext* ctx,
        JsValue value,
        MetaType metaTypeType
    )
    {
        StdVariantOfEnttMetaAnyAndJsValue variant = new();
        var ret = (
            (delegate* unmanaged<void*, JsContext*, JsValue, void*, void*>)
                _ptrJSValueToNativeAny.Value
        )(variant, ctx, value, metaTypeType);
        if (ret != variant) //return a1
            throw new Exception("JSValueToNativeAny failed");
        return variant;
    }

    private static Lazy<nint> _ptrJSValueToNativeAny = GetPointer(
        "_ZN9Scripting7QuickJS18JSValueToNativeAnyEP9JSContext7JSValueRKN4entt9meta_typeE"
    );

    /*__int64 __fastcall std::variant<entt::meta_any,JSValue>::variant<entt::meta_any,void,void,entt::meta_any,void>(
        __int64 a1,
        __int64 a2)*/
}
