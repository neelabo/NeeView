using System;

namespace NeeView
{
    /// <summary>
    /// ページ削除用のエントリ種類
    /// </summary>
    [Flags]
    public enum DeleteEntryType
    {
        None = 0,
        File = (1 << 0),
        ArchiveEntry = (1 << 1),
        PlaylistEntry = (1 << 2),
    }


    public static class DeleteEntryTypeExtensions
    {
        /// <summary>
        /// 複数の種類？
        /// </summary>
        public static bool IsVarious(this DeleteEntryType value)
        {
            int popCount = System.Numerics.BitOperations.PopCount((uint)value);
            return (popCount > 1);
        }

        /// <summary>
        /// 削除したら復元不可能？
        /// </summary>
        public static bool IsIrreversible(this DeleteEntryType value)
        {
            return value.HasFlag(DeleteEntryType.ArchiveEntry);
        }
    }
}
