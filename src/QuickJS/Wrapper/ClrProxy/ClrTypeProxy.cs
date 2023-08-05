using System.Reflection;
using Hosihikari.VanillaScript.QuickJS.Extensions;
using Hosihikari.VanillaScript.QuickJS.Helper;
using Hosihikari.VanillaScript.QuickJS.Types;
using Hosihikari.VanillaScript.QuickJS.Wrapper;
using Hosihikari.VanillaScript.QuickJS.Wrapper.Reflect;

namespace Hosihikari.VanillaScript.QuickJS.Wrapper.ClrProxy;

internal class ClrTypeProxy : ClrTypeProxyBase, IDisposable
{
    public override bool HasConstructor => true;

    //Type
    //    is not (
    //        { IsAbstract: true }
    //        or { IsGenericType: true, IsConstructedGenericType: false }
    //        or { IsInterface: true }
    //    );

    public Type Type { get; }
    private readonly Lazy<StaticMemberFinder> _staticFunctionFinder;
    public StaticMemberFinder MemberFinder => _staticFunctionFinder.Value;
    private readonly Lazy<TypeConstructorFinder> _constructorFinder;
    public TypeConstructorFinder ConstructorFinder => _constructorFinder.Value;
    private MemberReflectCache _memberCache = new();

    public ClrTypeProxy(Type type)
    {
        Type = type;
        _staticFunctionFinder = new Lazy<StaticMemberFinder>(() => new StaticMemberFinder(Type));
        _constructorFinder = new Lazy<TypeConstructorFinder>(() => new TypeConstructorFinder(Type));
    }

    private static Type FindTypeInAllAssemblies(string type)
    {
        foreach (var assembly in TypeFinder.EnumAllAssemblies())
        {
            var typeFinder = new TypeFinder(assembly);
            if (typeFinder.TryFindType(type, out var item))
            {
                return item;
            }
        }
        throw new TypeLoadException($"type {type} not found");
    }

    public ClrTypeProxy(string type)
        : this(FindTypeInAllAssemblies(type)) { }

    public ClrTypeProxy(string assemblyName, string type)
        : this(new TypeFinder(assemblyName).FindType(type)) { }

    public void Dispose()
    {
        OnDispose?.Invoke();
    }

    public event Action? OnDispose;

    protected override JsPropertyEnum[] GetOwnPropertyNames(JsContextWrapper ctxInstance)
    {
        var ownPropertyNames = MemberFinder
            .EnumStaticMembers()
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
        switch (member)
        {
            case MethodInfo method:
            {
                if (!_memberCache.GetMethodHelperCache(method, out var helper))
                {
                    helper = new MethodHelper(method);
                    _memberCache.AddMethodHelperCache(method, helper);
                }
                data = new JsPropertyDescriptor
                {
                    Flags = JsPropertyFlags.HasValue,
                    Value = ctxInstance
                        .NewJsFunctionObject(
                            (_, thisObj, argv) =>
                            {
                                Log.Logger.Trace("call");
                                return helper.Call(ctxInstance, argv, thisObj).Steal();
                            }
                        )
                        .Steal(),
                };
                Log.Logger.Trace("method");
                return true;
            }
            case PropertyInfo property:
            {
                data = new JsPropertyDescriptor
                {
                    Flags = JsPropertyFlags.GetSet | JsPropertyFlags.Enumerable
                };
                if (
                    !_memberCache.GetPropHelperCache(property, out var getHelper, out var setHelper)
                )
                {
                    if (property.GetMethod is { } getMethod)
                        getHelper = new MethodHelper(getMethod);
                    if (property.SetMethod is { } setMethod)
                        setHelper = new MethodHelper(setMethod);
                    _memberCache.AddPropHelperCache(property, getHelper, setHelper);
                }
                if (getHelper is not null)
                {
                    data.Flags |= JsPropertyFlags.HasGet;
                    data.Getter = ctxInstance
                        .NewJsFunctionObject(
                            (_, thisObj, argv) =>
                            {
                                Log.Logger.Trace("get");
                                return getHelper.Call(ctxInstance, argv, thisObj).Steal();
                            }
                        )
                        .Steal();
                }
                if (setHelper is not null)
                {
                    data.Flags |= JsPropertyFlags.HasSet;
                    data.Setter = ctxInstance
                        .NewJsFunctionObject(
                            (_, thisObj, argv) =>
                            {
                                Log.Logger.Trace("set");
                                return setHelper.Call(ctxInstance, argv, thisObj).Steal();
                            }
                        )
                        .Steal();
                }
                return true;
            }
            case FieldInfo field:
            {
                var value = field.GetValue(null);
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

    private static void CheckTypeConstructableOrThrow(Type type, int i = -1)
    {
        if (type.IsAbstract)
        {
            throw new ArgumentException(
                (i == -1 ? "" : $"argv[{i}]") + "abstract type not allowed"
            );
        }

        if (type is { IsGenericType: true, IsConstructedGenericType: false })
        {
            throw new ArgumentException(
                (i == -1 ? "" : $"argv[{i}]") + "generic type without MakeGenericType not allowed"
            );
        }

        if (type.IsInterface)
        {
            throw new ArgumentException(
                (i == -1 ? "" : $"argv[{i}]") + "interface type not allowed"
            );
        }
    }

    protected override JsValue Invoke(
        JsContextWrapper ctxInstance,
        JsValue thisVal,
        ReadOnlySpan<JsValue> argv,
        JsCallFlag flags
    )
    {
        if ((flags & JsCallFlag.Constructor) == 0)
        {
            return ctxInstance.ThrowJsError(
                new InvalidOperationException("call static type not allowed" + flags)
            );
        }
        //construct new instance
        CheckTypeConstructableOrThrow(Type);
        var data = new object?[argv.Length];
        var types = new Type[argv.Length];
        for (var i = 0; i < argv.Length; i++)
        {
            var arg = argv[i];
            if (arg.IsString())
            {
                data[i] = arg.ToString(ctxInstance);
                types[i] = typeof(string);
            }
            else if (arg.IsBool())
            {
                data[i] = arg.ToBoolean();
                types[i] = typeof(bool);
            }
            else if (arg.IsNumber())
            {
                throw new ArgumentException(
                    $"arg[{i}] is number. Please use toClrInstance to indicate which number type it is."
                );
                //data[i] = arg.ToDouble();
                //types[i]=typeof(double);
            }
            else if (arg.IsNull() || arg.IsUndefined() || arg.IsUninitialized())
            {
                throw new ArgumentException(
                    $"arg[{i}] is null or undefined. Please use toClrInstance to indicate the real type."
                );
            }
            else if (
                arg.IsObject()
                && TryGetInstance(arg, out var typeObj)
                && typeObj is ClrInstanceProxy { Instance: var instance, Type: var type }
            )
            {
                data[i] = instance;
                types[i] = type;
            }
            else
            {
                throw new ArgumentException(
                    $"arg[{i}] constructor must be ClrInstanceProxy. use toClrInstance to convert pure js object."
                );
            }
        }
        if (ConstructorFinder.TryFindConstructor(types, out var info))
        {
            var instance = info.Invoke(data);
            return ctxInstance
                .NewClrInstanceObject(new ClrInstanceProxy(instance, instance.GetType()))
                .Steal();
        }
        throw new ArgumentException(
            $"constructor with provided Type [{string.Join(", ", from x in types select x.Name)}] not found"
        );
    }

    public override string ToString()
    {
        return $"[ClrTypeProxy {Type}]";
    }
}
