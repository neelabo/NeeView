using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class ThumbnailCacheDatabase
    {
        private readonly Database _db;


        public ThumbnailCacheDatabase(Database db)
        {
            _db = db;
            CreateThumbsTable();
        }


        [Conditional("DEBUG")]
        private static void LocalWriteLine(string format, params object[] args)
        {
            //var prefix = $"[TC:{System.Environment.CurrentManagedThreadId}] "; 
            //Debug.WriteLine(prefix + format, args);
        }

        /// <summary>
        /// 初期化：データテーブルの作成
        /// </summary>
        private void CreateThumbsTable()
        {
            _db.ThrowIfDiposed();

            if (_db.Connection is null) return;

            using (SQLiteCommand command = _db.Connection.CreateCommand())
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
        /// すべてのサムネイルを削除
        /// </summary>
        internal void DeleteAll()
        {
            // TODO: とても重いので async 化すべきか？

            if (_db.Connection is null) return;

            LocalWriteLine($"Delete: All");

            using (SQLiteCommand command = _db.Connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM thumbs";
                int count = command.ExecuteNonQuery();
                LocalWriteLine($"Delete: {count}");
            }

            _db.Vacuum();
        }

        /// <summary>
        /// 古いサムネイルを削除
        /// </summary>
        /// <param name=""></param>
        internal void Delete(TimeSpan limitTime)
        {
            if (_db.Connection is null) return;

            var limitDateTime = DateTime.Now - limitTime;
            LocalWriteLine($"Delete: before {limitDateTime}");

            using (SQLiteCommand command = _db.Connection.CreateCommand())
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
            if (_db.Connection is null) return;

            LocalWriteLine($"Save: {header.Key}");

            using (SQLiteCommand command = _db.Connection.CreateCommand())
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
            if (_db.Connection is null) return null;

            var key = header.Key;
            var size = header.Size;
            var ghash = header.GenerateHash;

            using (SQLiteCommand command = _db.Connection.CreateCommand())
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
            if (_db.Connection is null) return;

            using (var transaction = _db.Connection.BeginTransaction())
            {
                using (SQLiteCommand command = _db.Connection.CreateCommand())
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
    }
}
