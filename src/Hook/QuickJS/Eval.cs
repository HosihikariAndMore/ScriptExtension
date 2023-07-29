using Hosihikari.NativeInterop.Hook.ObjectOriented;
using System.Text;
using Hosihikari.NativeInterop.Utils;
using Hosihikari.VanillaScript.Assets;
using Hosihikari.VanillaScript.QuickJS.Types;
using System.Runtime.InteropServices;

namespace Hosihikari.VanillaScript.Hook.QuickJS;

//ref https://github.com/bellard/quickjs/blob/master/quickjs.c#L33730
internal class Eval : HookBase<Eval.HookDelegate>
{
    internal unsafe delegate JsValue* HookDelegate(
        JsContext* ctx,
        byte* input,
        nuint inputLen,
        void* filename,
        JsEvalFlag evalFlags
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
        (ctx, contentBytes, size, file, evalFlags) =>
        {
            Log.Logger.Debug(
                "JS_Eval",
                "ctx: 0x"
                    + ((nint)ctx).ToString("X")
                    + " file: "
                    + Marshal.PtrToStringUTF8((nint)file)
            );
            try
            {
                //if (Marshal.PtrToStringUTF8(filenamePtr) is { } filename)
                {
                    var content = Encoding.UTF8.GetString(contentBytes, (int)size);
                    if (content == Prepare.FailedScriptContent) //is script context from build-in loader,not other behavior pack
                    {
                        //main script entry point
                        Loader.Manager.AddContext(ctx, true); //add context and load all scripts
                        fixed (
                            byte* p = StringUtils.StringToManagedUtf8(
                                Prepare.SuccessScriptContent, //replace to fake content
                                out var len
                            )
                        )
                        fixed (
                            byte* fakeFile = StringUtils.StringToManagedUtf8(
                                Guid.NewGuid().ToString("N") + ".js" //replace to fake filename
                            )
                        )
                        {
                            var ret = Original(ctx, p, (nuint)len, fakeFile, evalFlags);
                            return ret;
                        }
                    }
                    else //load from other behavior pack, so only add import
                    {
                        Loader.Manager.AddContext(ctx, false); //add context and but not load customize scripts in this context
                    }
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error(nameof(Eval), e);
            }
            return Original(ctx, contentBytes, size, file, evalFlags);
        };
}
