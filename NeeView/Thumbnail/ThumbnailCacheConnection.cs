using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class ThumbnailCacheConnection : IDisposable
    {
        private const string _format = "2.0";

        private readonly SQLiteConnection _connection;


        public ThumbnailCacheConnection(string path)
        {
            try
            {
                _connection = new SQLiteConnection($"Data Source={path}");
                _connection.Open();

                InitializePragma();
                CreatePropertyTable();

                if (IsSupportFormat())
                {
                    CreateThumbsTable();
                }
                else
                {
                    throw new ThumbnailCacheFormatException();
                }
            }
            catch
            {
                _connection?.Close();
                _connection?.Dispose();
                _connection = null!;
                throw;
            }
        }


        [Conditional("DEBUG")]
        private static void LocalWriteLine(string format, params object[] args)
        {
            //var prefix = $"[TC:{System.Environment.CurrentManagedThreadId}] "; 
            //Debug.WriteLine(prefix + format, args);
        }


        /// <summary>
        /// VACUUM
        /// </summary>
        internal void Vacuum()
        {
            using (SQLiteCommand command = _connection.CreateCommand())
            {
                command.CommandText = "VACUUM";
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// フォーマットチェック
        /// </summary>
        private bool IsSupportFormat()
        {
            var format = LoadProperty("format");

            var result = format == null || format == _format;
            LocalWriteLine($"Format: {format}: {result}");

            return result;
        }


        /// <summary>
        /// 初期化：PRAGMA設定
        /// </summary>
        private void InitializePragma()
        {
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
        /// 初期化：データテーブルの作成
        /// </summary>
        private void CreateThumbsTable()
        {
            using (SQLiteCommand command = _connection.CreateCommand())
            {
                // thumbnails 
                command.CommandText = "CREATE TABLE IF NOT EXISTS thumbs ("
                            + "key TEXT NOT NULL PRIMARY KEY,"
                            + "size INTEGER,"
                            + "date DATETIME,"
                            + "ghash INTEGER,"
                            + "value BLOB"
                            + ")";
                command.ExecuteNonQuery();
            }
        }


        /// <summary>
        /// プロパティの保存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal void SaveProperty(string key, string value)
        {
            if (_disposedValue) return;

            using (SQLiteCommand command = _connection.CreateCommand())
            {
                command.CommandText = $"REPLACE INTO property (key, value) VALUES (@key, @value)";
                command.Parameters.Add(new SQLiteParameter("@key", key));
                command.Parameters.Add(new SQLiteParameter("@value", value));
                command.ExecuteNonQuery();
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
        /// 古いサムネイルを削除
        /// </summary>
        /// <param name=""></param>
        internal void Delete(TimeSpan limitTime)
        {
            if (_disposedValue) return;

            var limitDateTime = DateTime.Now - limitTime;
            LocalWriteLine($"Delete: before {limitDateTime}");

            using (SQLiteCommand command = _connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM thumbs WHERE date < @date";
                command.Parameters.Add(new SQLiteParameter("@date", limitDateTime));
                int count = command.ExecuteNonQuery();

                LocalWriteLine($"Delete: {count}");
            }
        }


        /// <summary>
        /// サムネイルの保存
        /// </summary>
        /// <param name="header"></param>
        /// <param name="data"></param>
        internal void Save(ThumbnailCacheHeader header, byte[] data)
        {
            if (_disposedValue) return;

            LocalWriteLine($"Save: {header.Key}");

            using (SQLiteCommand command = _connection.CreateCommand())
            {
                Save(command, header, data);
            }
        }


        /// <summary>
        /// サムネイルの保存
        /// </summary>
        /// <param name="command"></param>
        /// <param name="header"></param>
        /// <param name="data"></param>
        private static void Save(SQLiteCommand command, ThumbnailCacheHeader header, byte[] data)
        {
            command.CommandText = "REPLACE INTO thumbs (key, size, date, ghash, value) VALUES (@key, @size, @date, @ghash, @value)";
            command.Parameters.Add(new SQLiteParameter("@key", header.Key));
            command.Parameters.Add(new SQLiteParameter("@size", header.Size));
            command.Parameters.Add(new SQLiteParameter("@date", header.AccessTime));
            command.Parameters.Add(new SQLiteParameter("@ghash", header.GenerateHash));
            command.Parameters.Add(new SQLiteParameter("@value", data));
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// アクセス日時を更新
        /// </summary>
        /// <param name="command"></param>
        /// <param name="header"></param>
        private static void UpdateDate(SQLiteCommand command, ThumbnailCacheHeader header)
        {
            command.CommandText = "UPDATE thumbs SET date = @date WHERE key = @key";
            command.Parameters.Add(new SQLiteParameter("@key", header.Key));
            command.Parameters.Add(new SQLiteParameter("@date", header.AccessTime));
            command.ExecuteNonQuery();
        }


        /// <summary>
        /// サムネイルデータの読込
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        internal async ValueTask<ThumbnailCacheRecord?> LoadAsync(ThumbnailCacheHeader header, CancellationToken token)
        {
            if (_disposedValue) return null;

            var key = header.Key;
            var size = header.Size;
            var ghash = header.GenerateHash;

            using (SQLiteCommand command = _connection.CreateCommand())
            {
                command.CommandText = "SELECT value, date FROM thumbs WHERE key = @key AND size = @size AND ghash = @ghash";
                command.Parameters.Add(new SQLiteParameter("@key", key));
                command.Parameters.Add(new SQLiteParameter("@size", size));
                command.Parameters.Add(new SQLiteParameter("@ghash", ghash));

                using (var reader = command.ExecuteReader())
                {
                    if (await reader.ReadAsync(token))
                    {
                        var bytes = reader["value"] as byte[];
                        if (bytes is not null)
                        {
                            var dateTime = (reader["date"] as DateTime?) ?? DateTime.MinValue;

                            LocalWriteLine($"Load: o {header.Key}");
                            return new ThumbnailCacheRecord(bytes, dateTime);
                        }
                    }
                }
            }

            LocalWriteLine($"Load: x {header.Key}");
            return null;
        }


        /// <summary>
        /// 予約分をまとめて更新
        /// </summary>
        /// <param name="saveQueue">データを更新するもの</param>
        /// <param name="updateQueue">日付を更新するもの</param>
        public void SaveQueue(Dictionary<string, ThumbnailCacheItem> saveQueue, Dictionary<string, ThumbnailCacheHeader> updateQueue)
        {
            if (_disposedValue) return;

            using (var transaction = _connection.BeginTransaction())
            {
                using (SQLiteCommand command = _connection.CreateCommand())
                {
                    foreach (var item in saveQueue.Values)
                    {
                        LocalWriteLine($"Save: {item.Header.Key}");
                        Save(command, item.Header, item.Body);
                    }

                    foreach (var item in updateQueue.Values)
                    {
                        LocalWriteLine($"Update: {item.Key}");
                        UpdateDate(command, item);
                    }
                }
                transaction.Commit();
            }
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _connection.Close();
                    _connection.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
