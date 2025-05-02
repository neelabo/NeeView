using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// フォルダーの並び
    /// </summary>
    public enum FolderOrder
    {
        [AliasName("@SortOrder.FileName")]
        FileName,

        [AliasName("@SortOrder.FileNameDescending")]
        FileNameDescending,

        [AliasName("@SortOrder.Path")]
        Path,

        [AliasName("@SortOrder.PathDescending")]
        PathDescending,

        [AliasName("@SortOrder.FileType")]
        FileType,

        [AliasName("@SortOrder.FileTypeDescending")]
        FileTypeDescending,

        [AliasName("@SortOrder.TimeStamp")]
        TimeStamp,

        [AliasName("@SortOrder.TimeStampDescending")]
        TimeStampDescending,

        [AliasName("@SortOrder.Size")]
        Size,

        [AliasName("@SortOrder.SizeDescending")]
        SizeDescending,

        [AliasName("@SortOrder.EntryTime")]
        EntryTime,

        [AliasName("@SortOrder.EntryTimeDescending")]
        EntryTimeDescending,

        [AliasName("@SortOrder.Random")]
        Random,
    }

    public static class FolderOrderExtension
    {
        public static bool IsEntryCategory(this FolderOrder mode)
        {
            return mode switch
            {
                FolderOrder.EntryTime or FolderOrder.EntryTimeDescending => true,
                _ => false,
            };
        }

        public static bool IsPathCategory(this FolderOrder mode)
        {
            return mode switch
            {
                FolderOrder.Path or FolderOrder.PathDescending => true,
                _ => false,
            };
        }

        public static bool IsDescending(this FolderOrder mode)
        {
            return mode switch
            {
                FolderOrder.FileNameDescending => true,
                FolderOrder.PathDescending => true,
                FolderOrder.FileTypeDescending => true,
                FolderOrder.TimeStampDescending => true,
                FolderOrder.SizeDescending => true,
                FolderOrder.EntryTimeDescending => true,
                _ => false,
            };
        }
    }

}
