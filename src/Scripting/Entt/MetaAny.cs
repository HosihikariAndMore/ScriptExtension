using System.Runtime.InteropServices;

namespace Hosihikari.ScriptExtension.Scripting.Entt;

public unsafe class MetaAny : IDisposable
{
    public const int Size = 168;
    private void* _ptr;

    public MetaAny(void* ptr)
    {
        _ptr = NativeMemory.Alloc(Size);
        //copy all data from ptr to this._ptr
        NativeMemory.Copy(ptr, _ptr, Size);
    }

    public MetaAny()
    {
        _ptr = NativeMemory.AllocZeroed(Size);
    }

    //_ZN4entt8meta_anyD2Ev
    //entt::meta_any::~meta_any(_QWORD *a1)
    private static readonly Lazy<nint> _ptrDestructor = McQuickJs.GetPointer(
        "_ZN4entt8meta_anyD2Ev"
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

    ~MetaAny()
    {
        Free();
    }

    public void Dispose()
    {
        Free();
        GC.SuppressFinalize(this);
    }

    public static implicit operator void*(MetaAny metaAny)
    {
        if (metaAny._ptr is null)
            throw new NullReferenceException(nameof(MetaAny));
        return metaAny._ptr;
    }
}
