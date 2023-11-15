using Hosihikari.ScriptExtension.QuickJS.Types;

namespace Hosihikari.ScriptExtension.QuickJS.Wrapper;

public delegate JsValue JsModuleImportDelegate(JsContextWrapper ctx);
public delegate AutoDropJsValue JsModuleImportAutoDelegate(JsContextWrapper ctx);
