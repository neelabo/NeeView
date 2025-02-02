namespace NeeView
{
    public interface IMemoryElement
    {
        // メモリ保持者
        IMemoryOwner Owner { get; }

        // メモリサイズ
        long MemorySize { get; }
        
        // メモリ解放
        void Unload();
    }

}
