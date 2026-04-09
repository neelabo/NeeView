using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;


namespace NeeView
{
    public interface IExportImageWriter : IDisposable
    {
        public string CurrentName { get; }

        Task<Stream> OpenEntryAsync(ExportPageSource source, ExportImageService? service, ExportImageParameter parameter, IExportOverwritePolicy overwritePolicy, CancellationToken token);

        void Close();
    }


    public static class ExportImageWriterFactory
    {
        public static IExportImageWriter Create(ExportBookType type, string path, bool isOverwrite)
        {
            return type switch
            {
                ExportBookType.Folder
                    => new FolderExportImageWriter(path, isOverwrite),
                ExportBookType.Zip
                    => new ZipExportImageWriter(path, isOverwrite),
                _
                    => throw new ArgumentException($"Unsupported export book type: {type}"),
            };
        }
    }


    public class FolderExportImageWriter : IExportImageWriter
    {
        private string _path;
        private int _index;

        public FolderExportImageWriter(string path, bool isOverwrite)
        {
            Debug.Assert(System.IO.Path.IsPathFullyQualified(path));

            _path = path;

            // 親フォルダーは存在していなければいけない
            if (!FileIO.DirectoryExists(Path.GetDirectoryName(_path)))
            {
                throw new DirectoryNotFoundException($"Directory not found: {Path.GetDirectoryName(_path)}");
            }

            // 上書きチェック
            if (!isOverwrite && FileIO.EntryExists(_path))
            {
                throw new IOException($"File already exists: {_path}");
            }

            // フォルダーの存在を確定
            Directory.CreateDirectory(_path);
        }

        public string CurrentName { get; private set; } = "";

        // service がなあ。
        public async Task<Stream> OpenEntryAsync(ExportPageSource pageSource, ExportImageService? service, ExportImageParameter parameter, IExportOverwritePolicy overwritePolicy, CancellationToken token)
        {
            _index++;

            var namePolicy = new DefaultExportImageFileNamePolicy(parameter);

            var filename = namePolicy.CreateFileName(pageSource, _index);

            // 使用できないファイル名を置換
            filename = LoosePath.ValidPath(filename);

            var resolver = new FileExportOverwriteResolver(_path);
            var name = overwritePolicy.Resolve(filename, resolver, service);
            var path = resolver.GetFullPath(name);

            // サブフォルダーの保証
            if (name.Contains('\\') || name.Contains('/'))
            {
                var dir = Path.GetDirectoryName(path);
                if (dir is not null)
                {
                    Directory.CreateDirectory(dir);
                }
            }

            CurrentName = name;

            return new FileStream(path, FileMode.CreateNew, FileAccess.Write);
        }

        public void Close()
        {
        }

        public void Dispose()
        {
        }
    }


    public class ZipExportImageWriter : IExportImageWriter
    {
        private FileStream _output;
        private System.IO.Compression.ZipArchive _archive;
        private ZipExportOverwriteResolver _resolver = new();
        private string _path;
        private string _tempPath;
        private int _index;

        public ZipExportImageWriter(string path, bool isOverwrite)
        {
            Debug.Assert(System.IO.Path.IsPathFullyQualified(path));

            _path = path;

            // 親フォルダーは存在していなければいけない
            if (!FileIO.DirectoryExists(Path.GetDirectoryName(_path)))
            {
                throw new DirectoryNotFoundException($"Directory not found: {Path.GetDirectoryName(_path)}");
            }

            // 上書きチェック
            if (!isOverwrite && FileIO.EntryExists(_path))
            {
                throw new IOException($"File already exists: {_path}");
            }

            _tempPath = Temporary.CreateWorkFileName(_path);

            _output = new FileStream(_tempPath, FileMode.CreateNew, FileAccess.ReadWrite);
            _archive = new System.IO.Compression.ZipArchive(_output, ZipArchiveMode.Create);
        }

        public string CurrentName { get; private set; } = "";

        public async Task<Stream> OpenEntryAsync(ExportPageSource pageSource, ExportImageService? service, ExportImageParameter parameter, IExportOverwritePolicy overwritePolicy, CancellationToken token)
        {
            _index++;

            var fileNamePolicy = new DefaultExportImageFileNamePolicy(parameter);

            var name = fileNamePolicy.CreateFileName(pageSource, _index);

            // 使用できないファイル名を置換
            name = LoosePath.ValidPath(name);

            // 階層構造はそのまま。区切り記号をスラッシュに統一
            name = name.Replace('\\', '/');

            // ZIPでもエントリ名の重複は回避する
            name = overwritePolicy.Resolve(name, _resolver, service);
            _resolver.Add(name);

            CurrentName = name;

            var entry = _archive.CreateEntry(name);
            return await entry.OpenAsync(token);
        }

        public void Close()
        {
            _archive.Dispose();
            _output.Dispose();

            FileIO.Replace(_tempPath, _path, null);
        }

        public void Dispose()
        {
            _archive.Dispose();
            _output.Dispose();

            FileIO.DeleteFile(_tempPath);
        }
    }
}
