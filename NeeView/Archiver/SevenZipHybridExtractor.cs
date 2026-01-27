//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using SevenZip;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    [LocalDebug]
    public partial class SevenZipHybridExtractor
    {
        private readonly SevenZipExtractor _extractor;
        private readonly string _directory;
        private readonly Dictionary<int, SevenZipStreamInfo> _map = new();
        private CancellationToken _cancellationToken;
        private Stopwatch _stopwatch = new();
        private ISevenZipFileExtraction _fileExtraction;

        public SevenZipHybridExtractor(SevenZipExtractor extractor, string directory, ISevenZipFileExtraction fileExtraction)
        {
            _extractor = extractor;
            _directory = directory;
            _fileExtraction = fileExtraction;
        }


        public async ValueTask ExtractAsync(CancellationToken token)
        {
            _map.Clear();
            _cancellationToken = token;

            LocalDebug.WriteLine($"PreExtract: ...");
            _stopwatch.Restart();
            try
            {
                _extractor.FileExtractionStarted += Extractor_FileExtractionStarted;
                _extractor.FileExtractionFinished += Extractor_FileExtractionFinished;
                await Task.Run(() => _extractor.ExtractArchive(GetStreamFunc));
                _cancellationToken.ThrowIfCancellationRequested();
            }
            finally
            {
                _extractor.FileExtractionStarted -= Extractor_FileExtractionStarted;
                _extractor.FileExtractionFinished -= Extractor_FileExtractionFinished;
                _stopwatch.Stop();
                LocalDebug.WriteLine($"PreExtract: done. {_stopwatch.ElapsedMilliseconds}ms");
            }
        }

        private void Extractor_FileExtractionStarted(object? sender, FileInfoEventArgs e)
        {
            //LocalDebug.WriteLine($"Extract.Started: {e.FileInfo}, {e.FileInfo.Size:N0}byte, {_stopwatch.ElapsedMilliseconds}ms");
            e.Cancel = _cancellationToken.IsCancellationRequested;
        }

        private void Extractor_FileExtractionFinished(object? sender, FileInfoEventArgs e)
        {
            e.Cancel = _cancellationToken.IsCancellationRequested;
            if (e.Cancel) return;

            if (_map.TryGetValue(e.FileInfo.Index, out SevenZipStreamInfo? item))
            {
                _map.Remove(e.FileInfo.Index);
                item.Stream.Dispose();
                //LocalDebug.WriteLine($"Extract.Finished: {e.FileInfo}, {e.FileInfo.Size:N0}byte, {_stopwatch.ElapsedMilliseconds}ms");
                if (item.Data is not null)
                {
                    Debug.Assert(item.Data is not byte[] rawData || (int)e.FileInfo.Size == rawData.Length);
                    _fileExtraction.SetData(e.FileInfo, item.Data);
                    _fileExtraction.WriteZoneIdentifier(e.FileInfo);
                }
            }
        }

        private Stream? GetStreamFunc(ArchiveFileInfo info)
        {
            // 既にデータが存在しているときはスキップ
            if (_fileExtraction.DataExists(info))
            {
                return null;
            }

            // 展開先をメモリがファイルかを判断する
            // TODO: ArchiveをStream対応させ、オンメモリ展開も選択できるようにする
            SevenZipStreamInfo streamInfo;
            if (ArchiveManager.Current.IsSupported(info.FileName, false, true) || PreExtractMemory.Current.IsFull((long)info.Size))
            {
                var path = Path.Combine(_directory, GetTempFileName(info));
                streamInfo = new TempFileSevenZipStreamInfo(info, path, File.Create(path));
            }
            else
            {
                streamInfo = new MemorySevenZipStreamInfo(info, new MemoryStream((int)info.Size));
            }
            _map.Add(info.Index, streamInfo);
            return streamInfo.Stream;
        }

        private static string GetTempFileName(ArchiveFileInfo info)
        {
            var extension = info.IsDirectory ? "" : LoosePath.GetExtension(info.FileName);
            return $"{info.Index:000000}{extension}";
        }
    }



    public interface ISevenZipFileExtraction
    {
        bool DataExists(ArchiveFileInfo info);
        void SetData(ArchiveFileInfo info, object data);
        void WriteZoneIdentifier(ArchiveFileInfo info);
    }


    public record struct SevenZipEntry(ArchiveFileInfo FileInfo, object? Data);


    public class SevenZipStreamInfo
    {
        public SevenZipStreamInfo(ArchiveFileInfo fileInfo, Stream stream)
        {
            FileInfo = fileInfo;
            Stream = stream;
        }

        public ArchiveFileInfo FileInfo { get; }
        public Stream Stream { get; }
        public virtual object? Data => null;
    }

    public class MemorySevenZipStreamInfo : SevenZipStreamInfo
    {
        public MemorySevenZipStreamInfo(ArchiveFileInfo fileInfo, MemoryStream stream) : base(fileInfo, stream)
        {
        }

        public override object? Data => ((MemoryStream)Stream).GetBuffer();
    }

    public class TempFileSevenZipStreamInfo : SevenZipStreamInfo
    {
        public TempFileSevenZipStreamInfo(ArchiveFileInfo fileInfo, string fileName, FileStream stream) : base(fileInfo, stream)
        {
            FileName = fileName;
        }

        public string FileName { get; }
        public override object? Data => FileName;
    }
}



