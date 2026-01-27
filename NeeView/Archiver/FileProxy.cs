using System;

namespace NeeView
{
    /// <summary>
    /// ファイルプロキシ
    /// </summary>
    public class FileProxy : IDisposable
    {
        private bool _disposedValue;


        public FileProxy(string path)
        {
            this.Path = path;
        }


        public string Path { get; protected set; }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
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
