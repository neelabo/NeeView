using System;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class FileIdDatabase
    {
        private readonly Database _db;

        public FileIdDatabase(Database db)
        {
            _db = db;
            CreateTable();
        }


        public void CreateTable()
        {
            _db.ThrowIfDiposed();

            if (_db.Connection is null) return;

            using (SQLiteCommand command = _db.Connection.CreateCommand())
            {
                command.CommandText = """
                    CREATE TABLE IF NOT EXISTS files (
                        path TEXT NOT NULL PRIMARY KEY,
                        volume_id INTEGER,
                        file_id BLOB
                    )
                    """;
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// パスと FileId を追加
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileId"></param>
        internal void Write(string path, FileIdEx fileId)
        {
            if (_db.Connection is null) return;

            using (SQLiteCommand command = _db.Connection.CreateCommand())
            {
                command.CommandText = """
                    INSERT INTO files (path, volume_id, file_id)
                    VALUES (@path, @volume_id, @file_id)
                    ON CONFLICT(path) DO UPDATE SET
                        volume_id = excluded.volume_id,
                        file_id = excluded.file_id
                    """;
                command.Parameters.Add(new SQLiteParameter("@path", path));
                command.Parameters.Add(new SQLiteParameter("@volume_id", fileId.VolumePathId));
                command.Parameters.Add(new SQLiteParameter("@file_id", fileId.FileId128));
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// パスの存在確認
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal bool TargetExists(string path)
        {
            if (_db.Connection is null) return false;

            using (SQLiteCommand command = _db.Connection.CreateCommand())
            {
                command.CommandText = """
                    SELECT EXISTS(
                        SELECT 1 FROM files WHERE path = @path
                    );
                    """;
                command.Parameters.Add(new SQLiteParameter("@path", path));
                var result = command.ExecuteScalar();
                return Convert.ToInt32(result) == 1;
            }
        }

        /// <summary>
        /// パスから FileId を取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal FileIdEx? Read(string path)
        {
            if (_db.Connection is null) return null;

            using (SQLiteCommand command = _db.Connection.CreateCommand())
            {
                command.CommandText = """
                    SELECT volume_id, file_id
                    FROM files
                    WHERE path = @path
                    """;
                command.Parameters.Add(new SQLiteParameter("@path", path));

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var volume = (int)reader.GetInt64(0); //(int)reader["vid"];
                        var bytes = (byte[])reader.GetValue(1); //(byte[])reader["fileid"];
                        return new FileIdEx(volume, bytes);
                    }
                }
            }

            return null;
        }

        internal async ValueTask<FileIdEx?> ReadAsync(string path, CancellationToken token)
        {
            if (_db.Connection is null) return null;

            using (SQLiteCommand command = _db.Connection.CreateCommand())
            {
                command.CommandText = """
                    SELECT volume_id, file_id
                    FROM files
                    WHERE path = @path
                    """;
                command.Parameters.Add(new SQLiteParameter("@path", path));

                using (var reader = command.ExecuteReader())
                {
                    if (await reader.ReadAsync(token))
                    {
                        var volumeId = (int)reader.GetInt64(0); // (int)reader["vid"];
                        var bytes = (byte[])reader.GetValue(1); // (byte[])reader["fileid"];
                        return new FileIdEx(volumeId, bytes);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// パスを削除
        /// </summary>
        /// <param name="path"></param>
        internal void Delete(string path)
        {
            if (_db.Connection is null) return;

            using (SQLiteCommand command = _db.Connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM files WHERE path = @path";
                command.Parameters.Add(new SQLiteParameter("@path", path));
                int count = command.ExecuteNonQuery();
            }
        }
    }

}

