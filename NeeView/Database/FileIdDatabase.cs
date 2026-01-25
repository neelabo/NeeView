using System;
using System.Data.SQLite;

namespace NeeView
{
    /// <summary>
    /// パスとFileIdの対応データベース
    /// </summary>
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
                    WITHOUT ROWID
                    """;
                command.ExecuteNonQuery();
            }

            using (SQLiteCommand command = _db.Connection.CreateCommand())
            {
                command.CommandText = """
                    CREATE INDEX IF NOT EXISTS idx_volume_file_path
                    ON files (volume_id, file_id, path)
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
                        SELECT 1 FROM files
                        WHERE path = @path
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
                        var volume = (int)reader.GetInt64(0);
                        var bytes = (byte[])reader.GetValue(1);
                        return new FileIdEx(volume, bytes);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// FileId からパスを取得
        /// </summary>
        /// <param name="fileId">FileId</param>
        /// <param name="predicate">受け入れ条件</param>
        /// <returns></returns>
        internal string? GetPath(FileIdEx fileId, Func<string, bool> predicate)
        {
            if (_db.Connection is null) return null;

            using (SQLiteCommand command = _db.Connection.CreateCommand())
            {
                command.CommandText = """
                    SELECT path
                    FROM files
                    WHERE volume_id = @volume_id AND file_id = @file_id
                    """;
                command.Parameters.Add(new SQLiteParameter("@volume_id", fileId.VolumePathId));
                command.Parameters.Add(new SQLiteParameter("@file_id", fileId.FileId128));

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var path = reader.GetString(0);
                        if (predicate(path))
                        {
                            return path;
                        }
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

