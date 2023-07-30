using System.Runtime.InteropServices;
using Hosihikari.VanillaScript.QuickJS;
using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.Loader;

/// <summary>
/// add js module to JsContext
/// </summary>
public static partial class Manager
{
    //usage in js:
    // import { api } from '@hosihikari';
    private const string ApiModuleName = "api";

    internal static unsafe void SetupContext(JsContext* ctx)
    {
        var module = Native.JS_NewCModule(
            ctx,
            "@" + nameof(Hosihikari).ToLower(), //module Name
            &JsModuleInitFunc // callback when import this module in js
        );
        Native.JS_AddModuleExport(ctx, module, ApiModuleName);
    }

    [UnmanagedCallersOnly]
    public static unsafe int JsModuleInitFunc(JsContext* ctx, JsModuleDef* module)
    {
        //when import in js called, this func will be called
        //then setup real JSValue for this module
        try
        {
            using var instanceSafe = Native.JS_NewObject(ctx);
            var instance = instanceSafe.Steal();
            Modules.TestModule.Bind(ctx, instance);
            Native.JS_SetModuleExport(ctx, module, ApiModuleName, instance);
        }
        catch (Exception e)
        {
            Log.Logger.Error(nameof(JsModuleInitFunc), e);
        }
        return 0;
    }
}
