namespace Hosihikari.VanillaScript.QuickJS.Types;

//ref https://github.com/bellard/quickjs/blob/master/quickjs.c#L302
[Flags]
public enum JsEvalFlag : int
{
    /// <summary>
    /// global code (default)
    /// </summary>
    TypeGlobal = 0,

    /// <summary>
    /// module code
    /// </summary>
    TypeModule = 1,

    /// <summary>
    /// direct call (internal use)
    /// </summary>
    TypeDirect = 2,

    /// <summary>
    /// indirect call (internal use)
    /// </summary>
    TypeIndirect = 3,

    /// <summary>
    /// force 'strict' mode
    /// </summary>
    Strict = 1 << 3,

    /// <summary>
    /// force 'strip' mode
    /// </summary>
    Strip = 1 << 4,

    /// <summary>
    /// compile but do not run. The result is an object with a
    ///      JS_TAG_FUNCTION_BYTECODE or JS_TAG_MODULE tag. It can be executed
    ///      with JS_EvalFunction().
    /// </summary>
    CompileOnly = 1 << 5,

    /// <summary>
    /// don't include the stack frames before this eval in the Error() backtraces
    /// </summary>
    BacktraceBarrier = 1 << 6,
}
