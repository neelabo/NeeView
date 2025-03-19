using System;
using System.Diagnostics;
using System.IO;

namespace NeeView
{
    /// <summary>
    /// テンポラリディレクトリ
    /// </summary>
    public class TempDirectory : ITrash
    {
        private bool _disposedValue = false;

        public TempDirectory(string path)
        {
            // テンポラリフォルダー以外は非対応
            Debug.Assert(path.StartsWith(Temporary.Current.TempDirectory, StringComparison.Ordinal));
            Path = path;
        }


        public string Path { get; private set; }

        public bool IsDisposed => _disposedValue;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                try
                {
                    if (Path != null && Path.StartsWith(Temporary.Current.TempDirectory, StringComparison.Ordinal)) // 念入りチェック
                    {
                        if (Directory.Exists(Path)) Directory.Delete(Path, true);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

                _disposedValue = true;
            }
        }

        ~TempDirectory()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
