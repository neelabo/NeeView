#define LOCAL_DEBUG

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

        public FolderExportImageWriter(string path, bool isOverwrite)
        {
            Debug.Assert(System.IO.Path.IsPathFullyQualified(path));

            _path = path;

            // 親フォルダーは存在していなければいけない
            if (!Directory.Exists(Path.GetDirectoryName(_path)))
            {
                throw new DirectoryNotFoundException($"Directory not found: {Path.GetDirectoryName(_path)}");
            }

            // 上書きチェック
            if (!isOverwrite && FileIO.ExistsPath(_path))
            {
                throw new IOException($"File already exists: {_path}");
            }

            // フォルダーの存在を確定
            Directory.CreateDirectory(_path);
        }

        // service がなあ。
        public async Task<Stream> OpenEntryAsync(ExportPageSource pageSource, ExportImageService? service, ExportImageParameter parameter, IExportOverwritePolicy overwritePolicy, CancellationToken token)
        {
            var namePolicy = new DefaultExportImageFileNamePolicy();

            var filename = namePolicy.CreateFileName(pageSource, parameter.Mode, parameter.FileNameMode, parameter.FileFormat);

            // 使用できないファイル名を置換
            filename = LoosePath.ValidPath(filename);

            //TODO: service の ExportFolder, parameter の ExportFolder, _root などの衝突がありうるので、ファイルパスの生成ルールを明確にする必要がある
            var resolver = new FileExportOverwriteResolver(parameter);
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

        public ZipExportImageWriter(string path, bool isOverwrite)
        {
            Debug.Assert(System.IO.Path.IsPathFullyQualified(path));

            _path = path;

            // 親フォルダーは存在していなければいけない
            if (!Directory.Exists(Path.GetDirectoryName(_path)))
            {
                throw new DirectoryNotFoundException($"Directory not found: {Path.GetDirectoryName(_path)}");
            }

            // 上書きチェック
            if (!isOverwrite && FileIO.ExistsPath(_path))
            {
                throw new IOException($"File already exists: {_path}");
            }

            _tempPath = Temporary.CreateWorkFileName(_path);

            _output = new FileStream(_tempPath, FileMode.CreateNew, FileAccess.ReadWrite);
            _archive = new System.IO.Compression.ZipArchive(_output, ZipArchiveMode.Create);
        }

        public async Task<Stream> OpenEntryAsync(ExportPageSource pageSource, ExportImageService? service, ExportImageParameter parameter, IExportOverwritePolicy overwritePolicy, CancellationToken token)
        {
            var fileNamePolicy = new DefaultExportImageFileNamePolicy();

            var name = fileNamePolicy.CreateFileName(pageSource, parameter.Mode, parameter.FileNameMode, parameter.FileFormat);

            // 階層構造はそのまま。区切り記号をスラッシュに統一
            name = name.Replace('\\', '/');

            // ZIPでもエントリ名の重複は回避する
            name = overwritePolicy.Resolve(name, _resolver, service);
            _resolver.Add(name);

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
