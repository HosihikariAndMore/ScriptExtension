using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Hosihikari.VanillaScript.Scripting.Entt;

namespace Hosihikari.VanillaScript.Scripting.Std;

/// <summary>
/// std::variant&lt;entt::meta_any,JSValue&gt;
/// </summary>
public unsafe class StdVariantOfEnttMetaAnyAndJsValue : IDisposable
{
    public const int Size = MetaAny.Size + 8; //168+8
    private void* _ptr;

    public StdVariantOfEnttMetaAnyAndJsValue()
    {
        _ptr = NativeMemory.AllocZeroed(Size);
    }

    //ref to std::get<entt::meta_any,entt::meta_any,JSValue>
    private bool HasMetaAny =>
        _ptr is not null
        && *(
            (byte*)_ptr + MetaAny.Size /*168*/
        ) == 0; //index is first type

    public bool MoveValue([NotNullWhen(true)] out MetaAny? value)
    {
        if (HasMetaAny)
        {
            value = new(_ptr);
            Free();
            return true;
        }
        value = null;
        return false;
    }

    private void Free()
    {
        if (_ptr is not null)
        {
            NativeMemory.Free(_ptr);
            _ptr = null;
        }
    }

    ~StdVariantOfEnttMetaAnyAndJsValue()
    {
        MoveValue(out _);
        Free();
    }

    public void Dispose()
    {
        MoveValue(out _);
        Free();
        GC.SuppressFinalize(this);
    }

    public static implicit operator void*(StdVariantOfEnttMetaAnyAndJsValue val)
    {
        if (val._ptr is null)
            throw new NullReferenceException(nameof(StdVariantOfEnttMetaAnyAndJsValue));
        return val._ptr;
    }
}
