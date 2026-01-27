using System;

namespace NeeView
{
    /// <summary>
    /// アーカイブ専用の一時ファイルディレクトリ
    /// </summary>
    public class ArchiveTemporary : IDisposable
    {
        private TempDirectory? _tempDirectory;
        private bool _disposedValue;


        public string CreateTempFileName(string prefix, string ext)
        {
            var tempDirectory = EnsureTempDirectory();
            return TemporaryTools.CreateCountedTempFileName(tempDirectory.Path, prefix, ext);
        }

        public string CreateTempFileName(string name)
        {
            var tempDirectory = EnsureTempDirectory();
            return TemporaryTools.CreateTempFileName(tempDirectory.Path, name);
        }


        private TempDirectory EnsureTempDirectory()
        {
            if (_tempDirectory is null)
            {
                var name = Temporary.Current.CreateCountedTempFileName("tmp", "");
                _tempDirectory = new TempDirectory(name);
            }
            return _tempDirectory;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _tempDirectory?.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

