using System;
using System.IO;
using SQLite;
using System.Threading.Tasks;

namespace MaintainHome.Database
{
    public static class DatabaseConnection
    {
        private static SQLiteAsyncConnection _database;

        public static async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            if (_database == null)
            {
                var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "MaintainHome.db3");
                Directory.CreateDirectory(Path.GetDirectoryName(databasePath));
                _database = new SQLiteAsyncConnection(databasePath);
            }
            return _database;
        }
    }
}
