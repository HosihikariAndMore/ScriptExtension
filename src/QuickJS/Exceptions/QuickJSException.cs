using Hosihikari.ScriptExtension.QuickJS.Extensions;
using Hosihikari.ScriptExtension.QuickJS.Types;
using Hosihikari.ScriptExtension.QuickJS.Wrapper;

namespace Hosihikari.ScriptExtension.QuickJS.Exceptions;

public class QuickJsException : Exception
{
    public override string Message { get; }
    public string? Name { get; }
    public string? JsStack { get; }

    internal unsafe QuickJsException(JsValue exceptionValue, JsContext* ctx)
    {
        if (Native.JS_IsError(ctx, exceptionValue))
        {
            //error delivered from js Error class
            Message = exceptionValue.GetStringProperty(ctx, "message");
            Name = exceptionValue.GetStringProperty(ctx, "name");
            JsStack = exceptionValue.GetStringProperty(ctx, "stack").TrimEnd();
        }
        else
        { //not standard error
            //just convert to string
            Message = exceptionValue.ToString(ctx);
        }
    }

    internal QuickJsException(AutoDropJsValue exceptionValue)
    {
        if (exceptionValue.IsError())
        {
            //error delivered from js Error class
            Message = exceptionValue.GetStringProperty("message");
            Name = exceptionValue.GetStringProperty("name");
            JsStack = exceptionValue.GetStringProperty("stack").TrimEnd();
        }
        else
        { //not standard error
            //just convert to string
            Message = exceptionValue.ToString();
        }
    }

    public override string? StackTrace
    {
        get
        {
            var originalStackTrace = base.StackTrace;
            var jsStack = JsStack;
            if (jsStack is not null)
            {
                jsStack = string.Join(
                    Environment.NewLine,
                    from x in jsStack.Split(
                        new[] { '\r', '\n' },
                        StringSplitOptions.RemoveEmptyEntries
                    )
                    select "   " + x.TrimStart()
                );
                if (originalStackTrace is null)
                {
                    return jsStack;
                }
                return jsStack + Environment.NewLine + originalStackTrace;
            }
            return originalStackTrace;
        }
    }
}
