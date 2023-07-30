using Hosihikari.VanillaScript.QuickJS.Types;
using Hosihikari.VanillaScript.QuickJS.Wrapper;

namespace Hosihikari.VanillaScript.Loader;

public static partial class Manager
{
    internal static readonly List<JsContextWrapper> LoadedScriptsContext = new();

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

    internal static unsafe void FreeContext(JsContext* ctx)
    {
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
}
