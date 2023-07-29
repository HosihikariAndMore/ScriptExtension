using System.Runtime.InteropServices;
using Hosihikari.Logging;
using Hosihikari.VanillaScript.QuickJS;
using Hosihikari.VanillaScript.QuickJS.Extensions;
using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.Loader;

/// <summary>
/// add js module to JsContext
/// </summary>
public static partial class Manager
{
    //usage in js:
    // import { api } from '@hosihikari';
    private const string apiModuleName = "api";

    internal static unsafe void SetupContext(JsContext* ctx)
    {
        var module = Native.JS_NewCModule(
            ctx,
            "@" + nameof(Hosihikari).ToLower(), //module Name
            &JsModuleInitFunc // callback when import this module in js
        );
        Native.JS_AddModuleExport(ctx, module, apiModuleName);
    }

    [UnmanagedCallersOnly]
    public static unsafe int JsModuleInitFunc(JsContext* ctx, JsModuleDef* module)
    {
        //when import in js called, this func will be called
        //then setup real JSValue for this module
        try
        {
            using var instance = Native.JS_NewObject(ctx, false);
            Modules.TestModule.Bind(ctx, instance.Value);
            Native.JS_SetModuleExport(ctx, module, apiModuleName, instance.Value);
        }
        catch (Exception e)
        {
            Log.Logger.Error(nameof(JsModuleInitFunc), e);
        }
        return 0;
    }
}
