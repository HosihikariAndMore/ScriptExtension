using Hosihikari.NativeInterop.Hook.ObjectOriented;
using System.Runtime.InteropServices;
using System.Text;
using Hosihikari.VanillaScript.QuickJS;

namespace Hosihikari.VanillaScript.Hook.QuickJS;

//ref https://github.com/bellard/quickjs/blob/master/quickjs.c#L33730
internal class Eval : HookBase<Eval.HookDelegate>
{
    internal unsafe delegate JsValue HookDelegate(
        JsValue a1,
        JsContext* a2,
        byte* input,
        long inputLen,
        nint filename,
        JsEvalFlag evalFlags
    );

    public Eval()
        : base("JS_Eval") { }

    public override unsafe HookDelegate HookedFunc =>
        (a1, ctx, contentBytes, size, filenamePtr, evalFlags) =>
        {
            try
            {
                if (Marshal.PtrToStringUTF8(filenamePtr) is { } filename)
                {
                    var content = Encoding.UTF8.GetString(contentBytes, (int)size);
                    Log.Logger.Trace(filename);
                    Log.Logger.Trace(content);
                    Log.Logger.Trace(evalFlags.ToString());
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error(nameof(Eval), e);
            }
            return Original(a1, ctx, contentBytes, size, filenamePtr, evalFlags);
        };
}
