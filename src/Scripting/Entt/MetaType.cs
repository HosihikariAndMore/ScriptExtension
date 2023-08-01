using System.Runtime.InteropServices;

namespace Hosihikari.VanillaScript.Scripting.Entt;

public unsafe class MetaType : IDisposable
{
    private void* _ptr;

    public MetaType()
    {
        _ptr = NativeMemory.AllocZeroed(128);
    }

    //_ZN4entt9meta_typeD2Ev
    //entt::meta_type::~meta_type(_QWORD *a1)
    private static readonly Lazy<nint> _ptrDestructor = McQuickJs.GetPointer(
        "_ZN4entt9meta_typeD2Ev"
    );

    private void Free()
    {
        if (_ptr is not null)
        {
            ((delegate* unmanaged<void*, void>)_ptrDestructor.Value)(_ptr);
            NativeMemory.Free(_ptr);
            _ptr = null;
        }
    }

    ~MetaType()
    {
        Free();
    }

    public void Dispose()
    {
        Free();
        GC.SuppressFinalize(this);
    }

    public static implicit operator void*(MetaType metaType)
    {
        if (metaType._ptr is null)
            throw new NullReferenceException(nameof(MetaType));
        return metaType._ptr;
    }
}
