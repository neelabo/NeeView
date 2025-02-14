using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NeeView
{
    public class MediaArchive : Archive
    {
        public MediaArchive(string path, ArchiveEntry? source) : base(path, source)
        {
        }


        public override string ToString()
        {
            return Properties.TextResources.GetString("Archiver.Media");
        }

        protected override async Task<List<ArchiveEntry>> GetEntriesInnerAsync(CancellationToken token)
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

            return await Task.FromResult(new List<ArchiveEntry>() { entry });
        }

        public override bool IsSupported()
        {
            return Config.Current.Archive.Media.IsEnabled;
        }

        protected override async Task<Stream> OpenStreamInnerAsync(ArchiveEntry entry, CancellationToken token)
        {
            Debug.Assert(entry.Archive == this);
            var path = entry.EntityPath ?? throw new InvalidOperationException("Must exist.");
            return await Task.FromResult(new FileStream(path, FileMode.Open, FileAccess.Read));
        }

        protected override async Task ExtractToFileInnerAsync(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            Debug.Assert(entry.Archive == this);
            var path = entry.EntityPath ?? throw new InvalidOperationException("Must exist.");
            await FileIO.CopyFileAsync(path, exportFileName, isOverwrite, token);
        }
    }
}
