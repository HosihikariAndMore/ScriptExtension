namespace Hosihikari.ScriptExtension.QuickJS.Types;

using size_t = UIntPtr;

public ref struct JsMallocState
{
    public size_t MallocCount;
    public size_t MallocSize;
    public size_t MallocLimit;
    public unsafe void* Opaque; /* user opaque */
}

public unsafe ref struct JsMallocFunctions
{
    //void *(*js_malloc)(JSMallocState *s, size_t size);
    public delegate* unmanaged<JsMallocState*, nuint, void*> JsMalloc;

    //void (*js_free)(JSMallocState *s, void *ptr);
    public delegate* unmanaged<JsMallocState*, void*, void> JsFree;

    //void *(*js_realloc)(JSMallocState *s, void *ptr, size_t size);
    public delegate* unmanaged<JsMallocState*, void*, nuint, void*> JsReAlloc;

    //size_t (*js_malloc_usable_size)(const void *ptr);
    public delegate* unmanaged<void*, size_t> JsMallocUsableSize;
}
