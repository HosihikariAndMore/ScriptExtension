using Hosihikari.VanillaScript.QuickJS.Types;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper;

public ref struct JsModuleDefWrapper
{
    public unsafe required JsModuleDef* ModuleDef;
    public required JsContextWrapper Context;
    public required string ModuleName;
    public required Dictionary<string, JsModuleImportDelegate> Members;

    public void AddExportValue(string name, JsModuleImportDelegate onImport)
    {
        AddExport(name, onImport);
    }

    public void AddExportValue(string name, JsModuleImportAutoDelegate onImport)
    {
        AddExport(name, c => onImport(c).Steal());
    }

    public unsafe void AddExportFunction(
        string name,
        int argumentLength,
        delegate* unmanaged<JsContext*, JsValue, int, JsValue*, JsValue> func,
        JsCFunctionEnum protoType = JsCFunctionEnum.Generic
    )
    {
        AddExport(
            name, //same name as function below
            ctx => ctx.NewStaticJsFunction(name, argumentLength, func, protoType).Steal()
        );
    }

    public void AddExportFunction(
        string name,
        int argumentLength,
        JsNativeFunctionDelegate func,
        JsCFunctionEnum protoType = JsCFunctionEnum.Generic
    )
    {
        AddExport(
            name, //same name as function below
            ctx => ctx.NewJsFunction(name, argumentLength, func, protoType).Steal()
        );
    }

    public void AddExport(string name, JsModuleImportDelegate onImport)
    {
        unsafe
        {
            if (Members.ContainsKey(name))
            {
                throw new InvalidOperationException(
                    $"Module {ModuleName} already has export {name}"
                );
            }
            Native.JS_AddModuleExport(Context.Context, ModuleDef, name);
            Members.Add(name, onImport);
        }
    }
}
