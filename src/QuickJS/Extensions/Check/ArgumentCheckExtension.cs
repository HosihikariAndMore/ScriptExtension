using Hosihikari.VanillaScript.QuickJS.Types;
using Hosihikari.VanillaScript.QuickJS.Wrapper;

namespace Hosihikari.VanillaScript.QuickJS.Extensions.Check;

public static class ArgumentCheckExtension
{
    /// <summary>
    /// insure argument count, throw exception if not match
    /// </summary>
    /// <param name="this"></param>
    /// <param name="count"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void InsureArgumentCount(this ReadOnlySpan<JsValue> @this, int count)
    {
        if (@this.Length == count)
        {
            return;
        }
        throw new ArgumentException($"too many arguments, expected {count}, got {@this.Length}");
    }

    /// <summary>
    /// insure argument count, throw exception if not match
    /// </summary>
    /// <param name="this"></param>
    /// <param name="allAllowCount"></param>
    /// <exception cref="ArgumentException"></exception>
    public static int InsureArgumentCount(
        this ReadOnlySpan<JsValue> @this,
        params int[] allAllowCount
    )
    {
        if (allAllowCount.Contains(@this.Length))
        {
            return @this.Length;
        }
        throw new ArgumentException(
            $"too many arguments, expected {string.Join(" or ", allAllowCount)}, got {@this.Length}"
        );
    }

    /// <summary>
    /// check the JSValue is string, throw exception if not
    /// </summary>
    /// <param name="this"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void InsureTypeString(this JsValue @this)
    {
        if (@this.IsString())
            return;
        throw new ArgumentException($"expected string, got {@this.Tag}");
    }

    /// <summary>
    /// check the JSValue is string, throw exception if not
    /// and return the string value
    /// </summary>
    /// <param name="this"></param>
    /// <param name="ctx"></param>
    public static string InsureTypeString(this JsValue @this, JsContextWrapper ctx)
    {
        InsureTypeString(@this);
        unsafe
        {
            return @this.ToString(ctx.Context);
        }
    }

    /// <summary>
    /// check the JSValue is string, throw exception if not
    /// and pass the string value to out parameter
    /// </summary>
    /// <param name="this"></param>
    /// <param name="ctx"></param>
    /// <param name="value"></param>
    public static void InsureTypeString(this JsValue @this, JsContextWrapper ctx, out string value)
    {
        InsureTypeString(@this);
        unsafe
        {
            value = @this.ToString(ctx.Context);
        }
    }
}
