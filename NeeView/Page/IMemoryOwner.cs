namespace NeeView
{
    public interface IMemoryOwner
    {
        // メモリ使用中で開放できない
        public bool IsMemoryLocked { get; }

        // 管理者番号
        // メモリ解放の優先度計算に使用される
        public int Index { get; }
    }

}
