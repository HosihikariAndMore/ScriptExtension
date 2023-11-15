namespace Hosihikari.ScriptExtension.QuickJS.Types;

/// <summary>
/// Represents a type using an internal metadata token.
/// </summary>
public enum JsTag
{
    /* all tags with a reference count are negative */
    First = -11, /* first negative tag */
    BigDecimal = -11,
    BigInt = -10,
    BigFloat = -9,
    Symbol = -8,
    String = -7,
    Module = -3, /* used internally */
    FunctionBytecode = -2, /* used internally */
    Object = -1,

    Int = 0,
    Bool = 1,
    Null = 2,
    Undefined = 3,
    Uninitialized = 4,
    CatchOffset = 5,

    /// <summary>
    /// note that JS_TAG_EXCEPTION is not used for exception objects
    /// it only indicate that the context has an exception pending
    /// please call <see cref="Native.JS_GetException"/> to get real exception object
    /// </summary>
    Exception = 6,
    Float64 = 7,
    /* any larger tag is FLOAT64 if JS_NAN_BOXING */
};
