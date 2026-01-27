using SevenZip;
using System;
using System.Collections.Generic;
using System.IO;

namespace NeeView
{
    /// <summary>
    /// SevenZipでテンポラリファイルに出力する
    /// </summary>
    public class SevenZipTempFileExtractor
    {
        private struct StreamInfo
        {
            public StreamInfo(ArchiveFileInfo fileInfo, string fileName, FileStream stream)
            {
                FileInfo = fileInfo;
                FileName = fileName;
                Stream = stream;
            }

            public ArchiveFileInfo FileInfo { get; }
            public string FileName { get; }
            public FileStream Stream { get; }
        }


        private string? _directory;
        private Dictionary<int, StreamInfo>? _map;


        public event EventHandler<SevenZipTempFileExtractionArgs>? TempFileExtractionFinished;


        public void ExtractArchive(SevenZipExtractor extractor, string directory)
        {
            _map = new Dictionary<int, StreamInfo>();

            _directory = directory;

            if (!Directory.Exists(_directory))
            {
                Directory.CreateDirectory(_directory);
            }

            try
            {
                extractor.FileExtractionFinished += Extractor_FileExtractionFinished;
                extractor.ExtractArchive(GetStreamFunc);
            }
            finally
            {
                extractor.FileExtractionFinished -= Extractor_FileExtractionFinished;
            }
        }

        private void Extractor_FileExtractionFinished(object? sender, FileInfoEventArgs e)
        {
            if (_map is null) throw new InvalidOperationException();

            if (_map.TryGetValue(e.FileInfo.Index, out StreamInfo item))
            {
                item.Stream.Dispose();
                _map.Remove(e.FileInfo.Index);

                TempFileExtractionFinished?.Invoke(sender, new SevenZipTempFileExtractionArgs(item.FileInfo, item.FileName));
            }
        }

        private Stream GetStreamFunc(ArchiveFileInfo info)
        {
            if (_map is null) throw new InvalidOperationException();
            if (_directory is null) throw new InvalidOperationException();

            ////Debug.WriteLine($"{info.Index}: {info.FileName}");

            var path = Path.Combine(_directory, GetTempFileName(info));
            var stream = File.Create(path);

            _map.Add(info.Index, new StreamInfo(info, path, stream));

            return stream;
        }

        private static string GetTempFileName(ArchiveFileInfo info)
        {
            var extension = info.IsDirectory ? "" : LoosePath.GetExtension(info.FileName);
            return $"{info.Index:000000}{extension}";
        }
    }
}


