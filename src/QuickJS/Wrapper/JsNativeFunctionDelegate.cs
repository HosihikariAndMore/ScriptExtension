using Hosihikari.ScriptExtension.QuickJS.Types;

namespace Hosihikari.ScriptExtension.QuickJS.Wrapper;

public delegate JsValue JsNativeFunctionDelegate(
    JsContextWrapper ctx,
    JsValue thisObject,
    ReadOnlySpan<JsValue> argv
);
