using Hosihikari.NativeInterop.Hook.ObjectOriented;

namespace Hosihikari.VanillaScript.Hook
{
    internal class RequestReload : HookBase<RequestReload.HookDelegate>
    {
        internal unsafe delegate void HookDelegate(void* minecraft);

        public RequestReload()
            : base("_ZN9Minecraft21requestResourceReloadEv") { }

        public override unsafe HookDelegate HookedFunc =>
            minecraft =>
            {
                Original.Invoke(minecraft);
                Config.Reload();
            };
    }
}
