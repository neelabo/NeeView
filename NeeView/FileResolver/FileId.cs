using System;

namespace NeeView
{
    /// <summary>
    /// FileID
    /// </summary>
    /// <remarks>
    /// ファイルやディレクトリが移動や名前変更されても同じドライブ内であればこのIDで追跡できる。
    /// ローカルドライブ (NTFS) のみ有効。
    /// </remarks>
    public record class FileId
    {
        public FileId(string volumePath, byte[] fileId)
        {
            FileId128 = fileId;
            VolumePath = volumePath;
        }

        public string VolumePath { get; }

        public byte[] FileId128 { get; }

        public override string ToString()
        {
            return $"VolumePath = {VolumePath}, FileId128 = {BitConverter.ToUInt128(FileId128):X32}";
        }
    }

}