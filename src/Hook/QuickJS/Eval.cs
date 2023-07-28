using Hosihikari.NativeInterop.Hook.ObjectOriented;
using System.Text;
using Hosihikari.NativeInterop.Utils;
using Hosihikari.VanillaScript.Assets;
using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.Hook.QuickJS;

//ref https://github.com/bellard/quickjs/blob/master/quickjs.c#L33730
internal class Eval : HookBase<Eval.HookDelegate>
{
    internal unsafe delegate JsValue HookDelegate(
        JsContext* ctx,
        byte* input,
        long inputLen,
        nint filename,
        JsEvalFlag evalFlags,
        JsValue thisObj
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
        (ctx, contentBytes, size, file, evalFlags, thisObj) =>
        {
            try
            {
                //if (Marshal.PtrToStringUTF8(filenamePtr) is { } filename)
                {
                    var content = Encoding.UTF8.GetString(contentBytes, (int)size);
                    if (content == Prepare.FailedScriptContent)
                    {
                        //main script entry point
                        Loader.Manager.AddContext(ctx);
                        fixed (
                            byte* p = StringUtils.StringToManagedUtf8(
                                Prepare.SuccessScriptContent,
                                out var len
                            )
                        )
                        {
                            var ret = Original(ctx, p, len, file, evalFlags, thisObj);
                            return ret;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error(nameof(Eval), e);
            }
            return Original(ctx, contentBytes, size, file, evalFlags, thisObj);
        };
    internal HookDelegate OriginalFunc => Original;
}
