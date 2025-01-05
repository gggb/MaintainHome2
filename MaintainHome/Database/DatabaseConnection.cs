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
            try
            {
                if (_database == null)
                {
                    var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "MaintainHome.db3");
                    Directory.CreateDirectory(Path.GetDirectoryName(databasePath));
                    _database = new SQLiteAsyncConnection(databasePath);
                }
                return _database;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating database connection: {ex.Message}");
                throw;
            }
        }
    }
}
