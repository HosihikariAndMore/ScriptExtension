using System.Reflection;
using Hosihikari.VanillaScript.QuickJS.Extensions;
using Hosihikari.VanillaScript.QuickJS.Helper;
using Hosihikari.VanillaScript.QuickJS.Types;
using Hosihikari.VanillaScript.QuickJS.Wrapper;
using Hosihikari.VanillaScript.QuickJS.Wrapper.Reflect;

namespace Hosihikari.VanillaScript.Modules.Clr;

internal class ClrInstanceProxy : ClrInstanceProxyBase, IDisposable, IFormattable
{
    private readonly Lazy<InstanceMemberFinder> _staticFunctionFinder;
    public InstanceMemberFinder MemberFinder => _staticFunctionFinder.Value;
    public Type Type { get; }
    public object? Instance { get; }

    public ClrInstanceProxy(object? instance, Type type)
    {
        Instance = instance;
        Type = type;
        _staticFunctionFinder = new Lazy<InstanceMemberFinder>(
            () => new InstanceMemberFinder(Type)
        );
    }

    public void Dispose() { }

    public override string ToString()
    {
        return Instance?.ToString() ?? "[null]";
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (Instance is IFormattable f)
        {
            return f.ToString(format, formatProvider);
        }
        return ToString();
    }

    protected override JsPropertyEnum[] GetOwnPropertyNames(JsContextWrapper ctxInstance)
    {
        var ownPropertyNames = MemberFinder
            .EnumMembers()
            .Select(
                member =>
                    new JsPropertyEnum
                    {
                        Atom = ctxInstance.NewAtom(member.Name).Steal(),
                        IsEnumerable = member is FieldInfo or PropertyInfo //function is not enumerable in for .. in loop in js
                    }
            )
            .ToArray();
        Log.Logger.Trace("property" + ownPropertyNames.Length);
        return ownPropertyNames;
    }

    protected override bool GetOwnProperty(
        JsContextWrapper ctxInstance,
        out JsPropertyDescriptor data,
        JsAtom propName
    )
    {
        var name = propName.ToString(ctxInstance);
        Log.Logger.Trace("GetOwnProperty:" + name);
        if (!MemberFinder.TryFindMember(name, out var member))
        {
            data = default;
            return false;
        }
        if (member is MethodInfo method)
        {
            var helper = new MethodHelper(method, Instance);
            data = new JsPropertyDescriptor
            {
                Flags = JsPropertyFlags.HasValue,
                Value = ctxInstance
                    .NewJsFunctionObject(
                        (_, _, argv) =>
                        {
                            Log.Logger.Trace("call");
                            return helper.Call(ctxInstance, argv).Steal();
                        }
                    )
                    .Steal(),
            };
            Log.Logger.Trace("method");
            return true;
        }
        if (member is PropertyInfo property)
        {
            data = new JsPropertyDescriptor
            {
                Flags = JsPropertyFlags.GetSet | JsPropertyFlags.Enumerable
            };
            if (property.GetMethod is { } getMethod)
            {
                var helper = new MethodHelper(getMethod, Instance);
                data.Flags |= JsPropertyFlags.HasGet;
                data.Getter = ctxInstance
                    .NewJsFunctionObject(
                        (_, _, argv) =>
                        {
                            Log.Logger.Trace("get");
                            return helper.Call(ctxInstance, argv).Steal();
                        }
                    )
                    .Steal();
            }
            if (property.SetMethod is { } setMethod)
            {
                var helper = new MethodHelper(setMethod, Instance);
                data.Flags |= JsPropertyFlags.HasSet;
                data.Setter = ctxInstance
                    .NewJsFunctionObject(
                        (_, _, argv) =>
                        {
                            Log.Logger.Trace("set");
                            return helper.Call(ctxInstance, argv).Steal();
                        }
                    )
                    .Steal();
            }
            return true;
        }
        if (member is FieldInfo field)
        {
            var value = field.GetValue(Instance);
            data = new JsPropertyDescriptor
            {
                Flags = JsPropertyFlags.HasValue | JsPropertyFlags.Enumerable,
                Value = JsValueCreateHelper.New(value, ctxInstance).Steal()
            };
            return true;
        }
        throw new NotImplementedException($"member type {member.Name}:{member.GetType()} not impl");
    }
}
