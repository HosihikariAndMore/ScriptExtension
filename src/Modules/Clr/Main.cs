using System.Reflection;
using Hosihikari.VanillaScript.QuickJS;
using Hosihikari.VanillaScript.QuickJS.Extensions;
using Hosihikari.VanillaScript.QuickJS.Extensions.Check;
using Hosihikari.VanillaScript.QuickJS.Helper;
using Hosihikari.VanillaScript.QuickJS.Types;
using Hosihikari.VanillaScript.QuickJS.Wrapper;
using Hosihikari.VanillaScript.QuickJS.Wrapper.Reflect;

namespace Hosihikari.VanillaScript.Modules.Clr;

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
    public static void Setup(JsContextWrapper ctx, JsModuleDefWrapper module)
    {
        module.AddExportFunction(
            "clrUsingStatic",
            2,
            (_, _, argv) =>
            {
                argv.InsureArgumentCount(2);
                argv[0].InsureTypeString(ctx, out var assemblyName);
                argv[1].InsureTypeString(ctx, out var type);
                return ctx.NewObject(new StaticTypeProxy(assemblyName, type)).Steal();
            }
        );
        module.AddExportFunction(
            "getAllLoadedAssemblyName",
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
            "getAllTypesFromAssembly",
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

internal class StaticTypeProxy : ClrProxyBase, IDisposable
{
    public Type? Type { get; }
    private readonly Lazy<StaticFunctionFinder> _staticFunctionFinder;
    public StaticFunctionFinder FunctionFinder => _staticFunctionFinder.Value;

    public StaticTypeProxy(string assemblyName, string type)
    {
        Type = new TypeFinder(assemblyName).FindType(type);
        _staticFunctionFinder = new Lazy<StaticFunctionFinder>(
            () => new StaticFunctionFinder(Type)
        );
    }

    public void Dispose()
    {
        OnDispose?.Invoke();
    }

    public event Action? OnDispose;

    protected override JsPropertyEnum[] GetOwnPropertyNames(JsContextWrapper ctxInstance) =>
        FunctionFinder
            .EnumStaticMembers()
            .Select(
                member =>
                    new JsPropertyEnum
                    {
                        Atom = ctxInstance.NewAtom(member.Name).Steal(),
                        IsEnumerable = false //todo what is this?
                    }
            )
            .ToArray();

    protected override bool GetOwnProperty(
        JsContextWrapper ctxInstance,
        out JsPropertyDescriptor data,
        JsAtom propName
    )
    {
        var name = propName.ToString(ctxInstance);
        if (!FunctionFinder.TryFindMember(name, out var member))
        {
            data = default;
            return false;
        }
        //MethodInfo
        if (member is MethodInfo method)
        { //todo fix this
            data = new JsPropertyDescriptor
            {
                Flags = JsPropertyFlags.HasGet,
                Getter = ctxInstance
                    .NewJsFunctionObject(
                        (_, thisObj, argv) =>
                        {
                            Log.Logger.Trace("get");
                            //todo impl
                            return JsValueCreateHelper.NewInt32(233);
                        }
                    )
                    .Steal(),
            };
            Log.Logger.Trace("method");
            return true;
        }
        if (member is PropertyInfo property)
        {
            data = new JsPropertyDescriptor { Flags = JsPropertyFlags.Normal };
            if (property.GetMethod is { } getMethod)
            {
                data.Flags |= JsPropertyFlags.HasGet;
                data.Getter = ctxInstance
                    .NewJsFunctionObject(
                        (_, thisObj, argv) =>
                        {
                            Log.Logger.Trace("get");
                            //todo impl
                            return JsValueCreateHelper.Undefined;
                        }
                    )
                    .Steal();
            }
            if (property.SetMethod is { } setMethod)
            {
                data.Flags |= JsPropertyFlags.HasSet;
                data.Setter = ctxInstance
                    .NewJsFunctionObject(
                        (_, thisObj, argv) =>
                        {
                            Log.Logger.Trace("set");
                            //todo impl
                            return JsValueCreateHelper.Undefined;
                        }
                    )
                    .Steal();
            }
            return true;
        }
        if (member is FieldInfo field)
        {
            var value = field.GetValue(null);
            data = new JsPropertyDescriptor
            {
                Flags = JsPropertyFlags.HasValue,
                Value = JsValueCreateHelper.New(value, ctxInstance).Steal()
            };
            return true;
        }
        throw new NotImplementedException($"member type {member.Name}:{member.GetType()} not impl");
    }

    protected override JsValue Invoke(
        JsContextWrapper ctxInstance,
        JsValue thisVal,
        ReadOnlySpan<JsValue> argv,
        JsCallFlag flags
    )
    {
        return ctxInstance.ThrowJsError(
            new InvalidOperationException("call static type not allowed")
        );
    }
}
