using Hosihikari.NativeInterop.Hook.ObjectOriented;
using System.Runtime.InteropServices;
using System.Text;
using Hosihikari.VanillaScript.QuickJS;

namespace Hosihikari.VanillaScript.Hook.QuickJS;

//ref https://github.com/bellard/quickjs/blob/master/quickjs.c#L33730
internal class Eval : HookBase<Eval.HookDelegate>
{
    internal unsafe delegate JsValue* HookDelegate(
        JsContext* ctx,
        byte* input,
        long inputLen,
        nint filename,
        JsEvalFlag evalFlags,
        JsValue* a1,
        byte unknown
    );

    //__int64 ctx,
    //    __int64 input,
    //__int64 input_len,
    //    __int64 fliename,
    //unsigned int a5,
    //    __int64 a6,
    //char unknown

    public Eval()
        : base("JS_Eval") { }

    public override unsafe HookDelegate HookedFunc =>
        (ctx, contentBytes, size, filenamePtr, evalFlags, jsValue, unknown) =>
        {
            try
            {
                Console.WriteLine("start");
                Console.WriteLine(unknown);
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
            return Original(ctx, contentBytes, size, filenamePtr, evalFlags, jsValue, unknown);
        };
}
