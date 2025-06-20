using System;
using System.IO;


namespace NeeView
{
    public class FileResource
    {
        private readonly string _path;
        private byte[]? _bytes;
        private FileResourceState _state;
        private FileStamp _fileStamp;
        private Exception? _exception;

        public FileResource(string path)
        {
            _path = path;
            _fileStamp = new FileStamp(path, default);
        }

        public string Path => _path;
        public byte[]? Bytes => _state == FileResourceState.Stable ? _bytes : GetBytes();
        public FileResourceState State => _state;
        public FileStamp FileStamp => _fileStamp;
        public Exception? Exception => _state == FileResourceState.Exception ? _exception : null;
        public bool IsBackup { get; init; }

        public bool IsValid()
        {
            GetBytes();
            return _state == FileResourceState.Stable;
        }

        private byte[]? GetBytes()
        {
            if (_state == FileResourceState.None)
            {
                try
                {
                    using (ProcessLock.Lock())
                    {
                        if (File.Exists(_path))
                        {
                            _bytes = File.ReadAllBytes(_path);
                            _fileStamp = FileStamp.Create(_path);
                            _state = FileResourceState.Stable;
                        }
                        else
                        {
                            _bytes = null;
                            _state = FileResourceState.FileNotFound;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _bytes = null;
                    _exception = ex;
                    _state = FileResourceState.Exception;
                }
            }
            return _bytes;
        }

        public void SetException(Exception ex)
        {
            _bytes = null;
            _exception = ex;
            _state = FileResourceState.Exception;
        }
    }


    public enum FileResourceState
    {
        /// <summary>
        /// 未処理
        /// </summary>
        None,

        /// <summary>
        /// データ読み込み済の安定状態
        /// </summary>
        Stable,

        /// <summary>
        /// ファイルが存在しません
        /// </summary>
        FileNotFound,

        /// <summary>
        /// 例外発生
        /// </summary>
        Exception,
    }
}
