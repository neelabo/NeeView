using System;

namespace NeeView
{
    /// <summary>
    /// FileId EX
    /// </summary>
    /// <remarks>
    /// VolePath を別テーブルのインデックス値にしたもの。
    /// データベース登録用に省メモリ化している。
    /// </remarks>
    public record class FileIdEx
    {
        public FileIdEx(int volumePathId, byte[] fileId128)
        {
            VolumePathId = volumePathId;
            FileId128 = fileId128;
        }

        public int VolumePathId { get; }
        public byte[] FileId128 { get; }

        public override string ToString()
        {
            return $"VolumePathId = {VolumePathId}, FileId128 = {BitConverter.ToUInt128(FileId128):X32}";
        }
    }

}

