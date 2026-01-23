using NeeLaboratory.Generators;
using System;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace NeeView
{
    /// <summary>
    /// Cache.db データベース
    /// </summary>
    /// <remarks>
    /// それぞれのテーブルへのアクセスはこのインスタンスを使用して行う
    /// </remarks>
    [LocalDebug]
    public partial class Database : IDisposable
    {
        private static readonly Lazy<Database> _current = new();
        public static Database Current => _current.Value;

        private const string _format = "2.0";
        public static string ThumbnailCacheFileName => "Cache.db";
        public static string DefaultThumbnailCacheFilePath => System.IO.Path.Combine(Environment.LocalApplicationDataPath, ThumbnailCacheFileName);
        public static string DatabasePath => Config.Current.Thumbnail.ThumbnailCacheFilePath ?? DefaultThumbnailCacheFilePath;

        private SQLiteConnection? _connection;
        private bool _disposedValue = false;


        public Database()
        {
            OpenOrRenew();
        }


        public SQLiteConnection? Connection => _connection;


        private void OpenOrRenew()
        {
            Open();
            if (!IsSupportFormat())
            {
                Close();
                FileIO.DeleteFile(DatabasePath);
                Open();
            }
        }

        private void Open()
        {
            if (_disposedValue) throw new ObjectDisposedException(this.GetType().FullName);
            if (_connection is not null) return;

            try
            {
                var path = DatabasePath;
                _connection = new SQLiteConnection($"Data Source={path}");
                _connection.Open();

                InitializePragma();
                CreatePropertyTable();
            }
            catch
            {
                _connection?.Close();
                _connection?.Dispose();
                _connection = null!;
                throw;
            }
        }

        private void Close()
        {
            if (_connection is not null)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }

        public void ThrowIfDiposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(this.GetType().FullName);
        }

        [MemberNotNull(nameof(_connection))]
        private void ThrowIfConnectionNull()
        {
            if (_connection is null) throw new InvalidOperationException("The database is not open.");
        }

        /// <summary>
        /// トランザクション開始
        /// </summary>
        /// <returns></returns>
        public DatabaseTransaction BeginTransaction()
        {
            return new DatabaseTransaction(this);
        }

        /// <summary>
        /// 初期化：PRAGMA設定
        /// </summary>
        private void InitializePragma()
        {
            ThrowIfDiposed();
            ThrowIfConnectionNull();

            using (SQLiteCommand command = _connection.CreateCommand())
            {
                command.CommandText = "PRAGMA auto_vacuum = full";
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 初期化：プロパティテーブルの作成
        /// </summary>
        private void CreatePropertyTable()
        {
            ThrowIfDiposed();
            ThrowIfConnectionNull();

            using (SQLiteCommand command = _connection.CreateCommand())
            {
                // database property
                command.CommandText = "CREATE TABLE IF NOT EXISTS property ("
                            + "key TEXT NOT NULL PRIMARY KEY,"
                            + "value TEXT"
                            + ")";
                command.ExecuteNonQuery();

                // property.format
                SavePropertyIfNotExist("format", _format);
            }
        }


        /// <summary>
        /// プロパティの保存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal void SavePropertyIfNotExist(string key, string value)
        {
            if (_disposedValue) return;
            ThrowIfConnectionNull();

            using (SQLiteCommand command = _connection.CreateCommand())
            {
                command.CommandText = "INSERT OR IGNORE INTO property (key, value) VALUES(@key, @value)";
                command.Parameters.Add(new SQLiteParameter("@key", key));
                command.Parameters.Add(new SQLiteParameter("@value", value));
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// プロパティの読込
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal string? LoadProperty(string key)
        {
            if (_disposedValue) return null;
            ThrowIfConnectionNull();

            using (SQLiteCommand command = _connection.CreateCommand())
            {
                command.CommandText = "SELECT value FROM property WHERE key = @key";
                command.Parameters.Add(new SQLiteParameter("@key", key));

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return reader.GetString(0);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// フォーマットチェック
        /// </summary>
        public bool IsSupportFormat()
        {
            ThrowIfDiposed();
            ThrowIfConnectionNull();

            var format = LoadProperty("format");

            var result = format == null || format == _format;
            LocalDebug.WriteLine($"Format: {format}: {result}");

            return result;
        }

        /// <summary>
        /// VACUUM
        /// </summary>
        internal void Vacuum()
        {
            ThrowIfDiposed();
            ThrowIfConnectionNull();

            using (SQLiteCommand command = _connection.CreateCommand())
            {
                command.CommandText = "VACUUM";
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// DBファイルサイズを取得
        /// </summary>
        public static long GetCacheDatabaseSize()
        {
            if (DatabasePath == null) throw new InvalidOperationException();

            var fileInfo = new FileInfo(DatabasePath);
            if (fileInfo.Exists)
            {
                return fileInfo.Length;
            }
            else
            {
                return 0L;
            }
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Close();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static void DisposeIfExists()
        {
            if (_current.IsValueCreated)
            {
                _current.Value.Dispose();
            }
        }
        #endregion


        /// <summary>
        /// トランザクション ラッパー
        /// </summary>
        public class DatabaseTransaction : IDisposable
        {
            private readonly SQLiteTransaction? _transaction;

            public DatabaseTransaction(Database db)
            {
                _transaction = db.Connection?.BeginTransaction();
            }

            public void Dispose()
            {
                _transaction?.Dispose();
            }

            public void Commit()
            {
                _transaction?.Commit();
            }

            public void Rollback()
            {
                _transaction?.Rollback();
            }
        }
    }
}
