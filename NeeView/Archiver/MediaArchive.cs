using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class MediaArchive : Archive
    {
        public MediaArchive(string path, ArchiveEntry? source, ArchiveHint archiveHint) : base(path, source, archiveHint)
        {
        }


        public override string ToString()
        {
            return TextResources.GetString("Archiver.Media");
        }

        protected override async Task<List<ArchiveEntry>> GetEntriesInnerAsync(bool decrypt, CancellationToken token)
        {
            var fileInfo = new FileInfo(this.Path);

            var entry = new MediaArchiveEntry(this)
            {
                IsValid = true,
                Id = 0,
                Instance = null,
                RawEntryName = LoosePath.GetFileName(this.EntryName),
                Length = fileInfo.Length,
                CreationTime = fileInfo.GetSafeCreationTime(),
                LastWriteTime = fileInfo.GetSafeLastWriteTime(),
            };

            return new List<ArchiveEntry>() { entry };
        }

        public override bool IsSupported()
        {
            return Config.Current.Archive.Media.IsEnabled;
        }

        protected override async Task<Stream> OpenStreamInnerAsync(ArchiveEntry entry, bool decrypt, CancellationToken token)
        {
            Debug.Assert(entry.Archive == this);
            var path = entry.EntityPath ?? throw new InvalidOperationException("Must exist.");
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        protected override async Task ExtractToFileInnerAsync(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            Debug.Assert(entry.Archive == this);
            var path = entry.EntityPath ?? throw new InvalidOperationException("Must exist.");
            await FileIO.CopyFileAsync(path, exportFileName, isOverwrite, true, token);
        }
    }
}
