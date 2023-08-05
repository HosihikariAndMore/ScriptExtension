using System.Reflection;
using Hosihikari.VanillaScript.QuickJS.Extensions;
using Hosihikari.VanillaScript.QuickJS.Helper;
using Hosihikari.VanillaScript.QuickJS.Types;
using Hosihikari.VanillaScript.QuickJS.Wrapper;
using Hosihikari.VanillaScript.QuickJS.Wrapper.Reflect;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper.ClrProxy;

internal class ClrInstanceProxy : ClrInstanceProxyBase, IDisposable, IFormattable
{
    private readonly Lazy<InstanceMemberFinder> _staticFunctionFinder;
    public InstanceMemberFinder MemberFinder => _staticFunctionFinder.Value;
    private MemberReflectCache _memberCache = new();
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
        var ownPropertyNames = (
            from member in MemberFinder.EnumMembers()
            let enumerable = member is FieldInfo or PropertyInfo //function is not enumerable in for .. in loop in js
            where enumerable || member is MethodInfo
            select new JsPropertyEnum
            {
                Atom = ctxInstance.NewAtom(member.Name).Steal(),
                IsEnumerable = enumerable
            }
        ).ToArray();
        return ownPropertyNames;
    }

    protected override bool GetOwnProperty(
        JsContextWrapper ctxInstance,
        out JsPropertyDescriptor data,
        JsAtom propName
    )
    {
        void InvokeAsProperty(
            PropertyInfo prop,
            out JsPropertyDescriptor data,
            object[]? indexer = null
        )
        {
            data = new JsPropertyDescriptor
            {
                Flags = JsPropertyFlags.GetSet | JsPropertyFlags.Enumerable
            };
            if (
                !_memberCache.GetPropHelperCache(
                    prop,
                    out var getHelper,
                    out var setHelper,
                    indexer
                )
            )
            {
                if (indexer is not null)
                {
                    if (prop.GetMethod is { } getMethod)
                        getHelper = new IndexerMethodHelper(getMethod, Instance, indexer);
                    if (prop.SetMethod is { } setMethod)
                        setHelper = new IndexerMethodHelper(setMethod, Instance, indexer);
                }
                else
                {
                    if (prop.GetMethod is { } getMethod)
                        getHelper = new MethodHelper(getMethod, Instance);
                    if (prop.SetMethod is { } setMethod)
                        setHelper = new MethodHelper(setMethod, Instance);
                }
                _memberCache.AddPropHelperCache(prop, getHelper, setHelper, indexer);
            }
            if (getHelper is not null)
            {
                data.Flags |= JsPropertyFlags.HasGet;
                data.Getter = ctxInstance
                    .NewJsFunctionObject(
                        (_, thisObj, argv) => getHelper.Invoke(ctxInstance, argv, thisObj).Steal()
                    )
                    .Steal();
            }
            if (setHelper is not null)
            {
                data.Flags |= JsPropertyFlags.HasSet;
                data.Setter = ctxInstance
                    .NewJsFunctionObject(
                        (_, thisObj, argv) => setHelper.Invoke(ctxInstance, argv, thisObj).Steal()
                    )
                    .Steal();
            }
        }
        //bool withIndex = propName.TryGetIndex(ctxInstance, out uint idx);
        if (propName.TryGetIndex(ctxInstance, out var idx)) //todo support multi dimension indexer
        {
            if (MemberFinder.TryGetIndexer(out var indexer))
            {
                InvokeAsProperty(indexer, out data, new object[] { idx });
                return true;
            }
        }
        var name = propName.ToString(ctxInstance);
        if (!MemberFinder.TryFindMember(name, out var member))
        {
            data = default;
            return false;
        }
        switch (member)
        {
            case PropertyInfo property:
                InvokeAsProperty(property, out data);
                return true;
            case MethodInfo method:
            {
                if (!_memberCache.GetMethodHelperCache(method, out var helper))
                {
                    helper = new MethodHelper(method, Instance);
                    _memberCache.AddMethodHelperCache(method, helper);
                }
                data = new JsPropertyDescriptor
                {
                    Flags = JsPropertyFlags.HasValue,
                    Value = ctxInstance
                        .NewJsFunctionObject(
                            (_, thisObj, argv) => helper.Invoke(ctxInstance, argv, thisObj).Steal()
                        )
                        .Steal(),
                };
                return true;
            }
            case FieldInfo field:
            {
                var value = field.GetValue(Instance);
                data = new JsPropertyDescriptor
                {
                    Flags = JsPropertyFlags.HasValue | JsPropertyFlags.Enumerable,
                    Value = JsValueCreateHelper
                        .New(value, ctxInstance, JsValueCreateHelper.Undefined)
                        .Steal()
                };
                return true;
            }
            default:
                throw new NotImplementedException(
                    $"member type {member.Name}:{member.GetType()} not impl"
                );
        }
    }
}
