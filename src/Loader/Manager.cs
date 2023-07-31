using Hosihikari.VanillaScript.QuickJS.Types;
using Hosihikari.VanillaScript.QuickJS.Wrapper;

namespace Hosihikari.VanillaScript.Loader;

public static partial class Manager
{
    internal static readonly List<JsContextWrapper> LoadedScriptsContext = new();
    internal static readonly List<JsRuntimeWrapper> LoadedScriptsRuntime = new();

    internal static unsafe void AddContext(JsContext* ctx, bool isLoaderContext)
    {
        var ctxInstance = JsContextWrapper.FetchOrCreate(ctx); //add to LoadedScriptsContext
        //if load from other behavior pack, just register modules for context
        //if is loader entry point, load all scripts in this context
        if (isLoaderContext)
        {
            LoadAllScripts(ctxInstance);
        }
    }

    internal static void FreeAllContextJsValue()
    {
        unsafe
        {
            foreach (var toFree in LoadedScriptsContext.ToArray())
            {
                try
                {
                    toFree.FreeValues();
                }
                catch (Exception ex)
                {
                    Log.Logger.Error("Free JsContext " + ((nint)toFree.Context).ToString("X"), ex);
                }
                finally
                {
                    LoadedScriptsContext.Remove(toFree);
                }
            }
        }
    }

    internal static unsafe void FreeContext(JsContext* ctx)
    {
        Log.Logger.Trace("JsContext Free " + ((nint)ctx).ToString("X"));
        foreach (var toFree in LoadedScriptsContext.FindAll(x => x.Context == ctx).ToArray())
        {
            try
            {
                toFree.Free();
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Free JsContext " + ((nint)ctx).ToString("X"), ex);
            }
            finally
            {
                LoadedScriptsContext.Remove(toFree);
            }
        }
    }

    internal static unsafe void AddRuntime(JsRuntime* rt)
    {
        var rtInstance = JsRuntimeWrapper.FetchOrCreate(rt); //add to LoadedScriptsRuntime
        Log.Logger.Trace("JsRuntime Add" + ((nint)rtInstance.Runtime).ToString("X"));
    }

    internal static unsafe void FreeRuntime(JsRuntime* rt)
    {
        foreach (var toFree in LoadedScriptsRuntime.FindAll(x => x.Runtime == rt).ToArray())
        {
            try
            {
                Log.Logger.Trace("JsRuntime Free" + ((nint)rt).ToString("X"));
                toFree.Free();
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Free JsRuntime " + ((nint)rt).ToString("X"), ex);
            }
            finally
            {
                LoadedScriptsRuntime.Remove(toFree);
            }
        }
    }
}
