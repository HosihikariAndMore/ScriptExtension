using Hosihikari.VanillaScript.QuickJS.Wrapper;

namespace Hosihikari.VanillaScript.QuickJS.Exceptions;

public class QuickJsException : Exception
{
    public override string Message { get; }
    public string? Name { get; }
    public string? JsStack { get; }

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
