using System.Collections.Generic;
using System.Data.SQLite;

namespace NeeView
{
    public class VolumeDatabase
    {
        private readonly Database _db;


        public VolumeDatabase(Database db)
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
                command.CommandText = "CREATE TABLE IF NOT EXISTS volumes ("
                            + "volume_id INTEGER NOT NULL PRIMARY KEY,"
                            + "volume_path TEXT"
                            + ")";
                command.ExecuteNonQuery();
            }
        }

        internal void WriteIfNotExist(int volumeId, string volumePath)
        {
            if (_db.Connection is null) return;

            using (SQLiteCommand command = _db.Connection.CreateCommand())
            {
                command.CommandText = "INSERT OR IGNORE INTO volumes (volume_id, volume_path) VALUES(@volume_id, @volume_path)";
                command.Parameters.Add(new SQLiteParameter("@volume_id", volumeId));
                command.Parameters.Add(new SQLiteParameter("@volume_path", volumePath));
                command.ExecuteNonQuery();
            }
        }

        internal Dictionary<int, string> ReadTable()
        {
            var volumeTable = new Dictionary<int, string>();

            if (_db.Connection is null) return volumeTable;

            using (SQLiteCommand command = _db.Connection.CreateCommand())
            {
                command.CommandText = "SELECT volume_id, volume_path FROM volumes";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var volumeId = (int)reader.GetInt64(0);
                        var volumePath = reader.GetString(1);
                        volumeTable.Add(volumeId, volumePath);
                    }
                }
            }
            return volumeTable;
        }

    }

}

