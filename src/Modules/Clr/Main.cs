using System.ComponentModel;
using Hosihikari.ScriptExtension.QuickJS.Extensions;
using Hosihikari.ScriptExtension.QuickJS.Extensions.Check;
using Hosihikari.ScriptExtension.QuickJS.Helper;
using Hosihikari.ScriptExtension.QuickJS.Types;
using Hosihikari.ScriptExtension.QuickJS.Wrapper;
using Hosihikari.ScriptExtension.QuickJS.Wrapper.ClrProxy;
using Hosihikari.ScriptExtension.QuickJS.Wrapper.ClrProxy.Generic;
using Hosihikari.ScriptExtension.QuickJS.Wrapper.Reflect;

namespace Hosihikari.ScriptExtension.Modules.Clr;

internal class Main
{
    public static void Setup(JsContextWrapper ctx)
    {
        var module = ctx.NewModule(Config.ConfigModules.ClrModuleName);
        ClrModule.Setup(ctx, module);
    }
}

internal class ClrModule
{
    /// <summary>
    /// get type from JsValue, the JsValue might be ClrTypeProxy with specific typ or string indicating type name
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="InvalidEnumArgumentException"></exception>
    private static Type GetBoxedClrTypeFromJsValue(JsContextWrapper ctx, JsValue value)
    {
        if (
            ClrProxyBase.TryGetInstance(value, out var item)
            && item is ClrTypeProxy { Type: var targetType }
        )
        { //direct provided  type
            return targetType;
        } //provide with string name of type
        return value.InsureTypeString(ctx) switch
        {
            "int" => typeof(int),
            "string" => typeof(string),
            "bool" => typeof(bool),
            "double" => typeof(double),
            "float" => typeof(float),
            "long" => typeof(long),
            "short" => typeof(short),
            "byte" => typeof(byte),
            "char" => typeof(char),
            "uint" => typeof(uint),
            "ulong" => typeof(ulong),
            "ushort" => typeof(ushort),
            "sbyte" => typeof(sbyte),
            "decimal" => typeof(decimal),
            "object" => typeof(object),
            _ => throw new InvalidEnumArgumentException("toClrInstance argv[1]")
        };
    }

    public static void Setup(JsContextWrapper ctx, JsModuleDefWrapper module)
    {
        module.AddExportFunction(
            "makeGenericType",
            2,
            (_, _, argv) =>
            {
                argv.InsureArgumentCountAtLeast(2);
                var baseType = GetBoxedClrTypeFromJsValue(ctx, argv[0]);
                Type[] types;
                if (argv[1].IsArray(ctx))
                {
                    var values = argv[1].GetArrayValues(ctx);
                    try
                    {
                        types = values
                            .Select(x => GetBoxedClrTypeFromJsValue(ctx, x.Value))
                            .ToArray();
                    }
                    finally
                    { //free immediately
                        foreach (var value in values)
                            value.Dispose();
                    }
                }
                else
                {
                    var typesJsValue = argv[1..];
                    types = new Type[typesJsValue.Length];
                    for (var i = 0; i < typesJsValue.Length; i++)
                        types[i] = GetBoxedClrTypeFromJsValue(ctx, typesJsValue[i]);
                }
                return ctx.NewClrTypeObject(new ClrTypeProxy(baseType.MakeGenericType(types)))
                    .Steal();
            }
        );
        module.AddExportFunction(
            "toClrInstance",
            2,
            (_, _, argv) =>
            {
                switch (argv.InsureArgumentCount(1, 2))
                {
                    case 1:
                    {
                        var obj =
                            argv[0].ToClrObject(ctx)
                            ?? throw new NullReferenceException("toClrInstance argv[0]");
                        return ctx.NewClrInstanceObject(new ClrInstanceProxy(obj, obj.GetType()))
                            .Steal();
                    }
                    case 2:
                    {
                        var obj =
                            argv[0].ToClrObject(ctx, GetBoxedClrTypeFromJsValue(ctx, argv[1]))
                            ?? throw new NullReferenceException("toClrInstance argv[0]");
                        return ctx.NewClrInstanceObject(new ClrInstanceProxy(obj, obj.GetType()))
                            .Steal();
                    }
                }

                return JsValueCreateHelper.Undefined;
            }
        );
        module.AddExportFunction(
            "clrImport",
            2,
            (_, _, argv) =>
            {
                if (argv.InsureArgumentCount(1, 2) == 2)
                {
                    argv[0].InsureTypeString(ctx, out var type);
                    argv[1].InsureTypeString(ctx, out var assemblyName);
                    return ctx.NewClrTypeObject(new ClrTypeProxy(assemblyName, type)).Steal();
                }
                {
                    argv[0].InsureTypeString(ctx, out var type);
                    return ctx.NewClrTypeObject(new ClrTypeProxy(type)).Steal();
                }
            }
        );
        module.AddExportFunction(
            "getAllLoadedAssembly",
            0,
            (_, _, argv) =>
            {
                argv.InsureArgumentCount(0);
                //filter null
                return ctx.NewArray(
                        from x in TypeFinder.EnumAllAssemblies()
                        select x.GetName().Name into x
                        where !string.IsNullOrWhiteSpace(x)
                        select x
                    )
                    .Steal();
            }
        );
        module.AddExportFunction(
            "getAllTypeFromAssembly",
            1,
            (_, _, argv) =>
            {
                argv.InsureArgumentCount(1);
                argv[0].InsureTypeString(ctx, out var assemblyName);
                //filter null
                return ctx.NewArray(
                        from x in TypeFinder.EnumAllType(assemblyName)
                        select x.FullName into x
                        where !string.IsNullOrWhiteSpace(x)
                        select x
                    )
                    .Steal();
            }
        );
    }
}
