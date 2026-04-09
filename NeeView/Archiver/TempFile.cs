using System;
using System.Diagnostics;
using System.IO;

namespace NeeView
{
    /// <summary>
    /// テンポラリファイル
    /// </summary>
    public class TempFile : FileProxy, ITrash
    {
        public TempFile(string path) : base(path)
        {
            // テンポラリフォルダー以外は非対応
            Debug.Assert(path.StartsWith(Temporary.Current.TempDirectory, StringComparison.Ordinal));

            UpdateLastAccessTime();
        }


        /// <summary>
        /// 最終アクセス日時
        /// </summary>
        public DateTime LastAccessTime { get; private set; }


        /// <summary>
        /// 最終アクセス日時更新
        /// </summary>
        public void UpdateLastAccessTime()
        {
            this.LastAccessTime = DateTime.Now;
        }

        #region ITrash Support

        public bool IsDisposed => _disposedValue;

        #endregion

        #region IDisposable Support

        private bool _disposedValue = false;

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    this.LastAccessTime = default;
                }

                try
                {
                    if (Path != null && Path.StartsWith(Temporary.Current.TempDirectory, StringComparison.Ordinal)) // 念入りチェック
                    {
                        if (FileIO.FileExists(Path))
                        {
                            File.Delete(Path);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

                _disposedValue = true;

                base.Dispose(disposing);
            }
        }

        ~TempFile()
        {
            Dispose(false);
        }

        #endregion
    }
}
