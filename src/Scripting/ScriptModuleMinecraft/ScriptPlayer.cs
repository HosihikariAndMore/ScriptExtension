using System.Diagnostics.CodeAnalysis;
using Hosihikari.Minecraft;

namespace Hosihikari.VanillaScript.Scripting.ScriptModuleMinecraft;

public class ScriptPlayer : ScriptObjectBase
{
    private static readonly Lazy<nint> _ptrTryGetPlayer = McQuickJs.GetPointer(
        "_ZNK21ScriptModuleMinecraft12ScriptPlayer12tryGetPlayerEv"
    );

    public bool TryGetPlayer([NotNullWhen(true)] out Player? player)
    {
        unsafe
        {
            var result = ((delegate* unmanaged<void*, void*>)_ptrTryGetPlayer.Value)(Ptr);
            if (result is not null)
            {
                player = new Player(result);
                return true;
            }
            player = null;
            return false;
        }
    }
}
