using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using SQLite;

namespace MaintainHome.Database
{
    public class BackupRestoreRepository
    {
        private readonly string _databasePath;
        private readonly string _backupPath;
        private readonly SQLiteAsyncConnection _database;

        public BackupRestoreRepository()
        {
            _database = DatabaseConnection.GetConnectionAsync().Result;
            _databasePath = _database.DatabasePath;
            _backupPath = Path.Combine(FileSystem.AppDataDirectory, "Backup", "backup.db3");

            // Ensure the Backup folder exists
            Directory.CreateDirectory(Path.Combine(FileSystem.AppDataDirectory, "Backup"));
        }

        public async Task<(bool, string)> CheckDatabaseIntegrityAsync()
        {
            try
            {
                var integrityResult = await _database.ExecuteScalarAsync<string>("PRAGMA integrity_check;");
                if (integrityResult != "ok")
                {
                    return (false, $"Database integrity check failed. Result: {integrityResult}");
                }
                return (true, "Database integrity check passed.");
            }
            catch (Exception ex)
            {
                return (false, $"Database integrity check failed: {ex.Message}");
            }
        }

        public async Task<(bool, string)> CheckRequiredTablesAsync()
        {
            try
            {
                var requiredTables = new string[]
                {
            "User", "PartBuy", "PartInfo", "Tasks", "TaskActivity", "Notification",
            "Category", "TaskHelp", "TaskNote", "PlugInNotification"
                };

                foreach (var table in requiredTables)
                {
                    var tableInfo = await _database.GetTableInfoAsync(table);
                    if (tableInfo.Count == 0)
                    {
                        return (false, $"Table '{table}' does not exist in the database.");
                    }
                }
                return (true, "All required tables exist in the database.");
            }
            catch (Exception ex)
            {
                return (false, $"Table existence check failed: {ex.Message}");
            }
        }

        public async Task<(bool, string)> PerformBackupAsync()
        {
            try
            {
                if (File.Exists(_databasePath))
                {
                    // Ensure previous backup file is deleted before copying
                    if (File.Exists(_backupPath))
                    {
                        File.Delete(_backupPath);
                    }

                    // Perform the manual copy operation using streams
                    using (var sourceStream = new FileStream(_databasePath, FileMode.Open, FileAccess.Read))
                    using (var destinationStream = new FileStream(_backupPath, FileMode.CreateNew, FileAccess.Write))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }

                    // Verify that the backup file was indeed created
                    if (File.Exists(_backupPath))
                    {
                        return (true, "Backup completed successfully.");
                    }
                    else
                    {
                        return (false, $"Unable to verify the creation of backup file at: {_backupPath}");
                    }
                }
                else
                {
                    return (false, $"Original database file not found at: {_databasePath}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Backup failed: {ex.Message}");
            }
        }

        public async Task<(bool, string)> CheckBackupIntegrityBeforeRestoreAsync()
        {
            try
            {
                var backupConnection = new SQLiteAsyncConnection(_backupPath);
                var backupIntegrityResult = await backupConnection.ExecuteScalarAsync<string>("PRAGMA integrity_check;");
                if (backupIntegrityResult != "ok")
                {
                    return (false, $"Backup integrity check failed before restore: Result: {backupIntegrityResult}");
                }

                var requiredTables = new string[]
                {
            "User", "PartBuy", "PartInfo", "Tasks", "TaskActivity", "Notification",
            "Category", "TaskHelp", "TaskNote", "PlugInNotification"
                };

                foreach (var table in requiredTables)
                {
                    var tableInfo = await backupConnection.GetTableInfoAsync(table);
                    if (tableInfo.Count == 0)
                    {
                        return (false, $"Table '{table}' does not exist in the backup file.");
                    }
                }

                return (true, "Backup integrity check passed before restore. All required tables exist in the backup file.");
            }
            catch (Exception ex)
            {
                return (false, $"Backup integrity check before restore failed: {ex.Message}");
            }
        }

        public async Task<(bool, string)> PerformRestoreAsync()
        {
            try
            {
                if (File.Exists(_backupPath))
                {
                    // Ensure previous database file is deleted before copying
                    if (File.Exists(_databasePath))
                    {
                        File.Delete(_databasePath);
                    }

                    // Perform the manual copy operation using streams
                    using (var sourceStream = new FileStream(_backupPath, FileMode.Open, FileAccess.Read))
                    using (var destinationStream = new FileStream(_databasePath, FileMode.CreateNew, FileAccess.Write))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }

                    // Verify that the database file was indeed restored
                    if (File.Exists(_databasePath))
                    {
                        return (true, $"Restore completed successfully. Database file restored from: {_backupPath}");
                    }
                    else
                    {
                        return (false, $"Restore failed: Unable to verify the restoration of database file to: {_databasePath}");
                    }
                }
                else
                {
                    return (false, $"Restore failed: Backup file not found at: {_backupPath}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Restore failed: {ex.Message}");
            }
        }

        public async Task<(bool, string)> CheckDatabaseIntegrityAfterRestoreAsync()
        {
            try
            {
                var integrityResult = await _database.ExecuteScalarAsync<string>("PRAGMA integrity_check;");
                if (integrityResult != "ok")
                {
                    return (false, $"Database integrity check after restore failed. Result: {integrityResult}");
                }
                return (true, "Database integrity check after restore passed.");
            }
            catch (Exception ex)
            {
                return (false, $"Database integrity check after restore failed: {ex.Message}");
            }
        }

        public async Task<(bool, string)> CheckBackupIntegrityAsync()
        {
            try
            {
                var backupConnection = new SQLiteAsyncConnection(_backupPath);
                var backupIntegrityResult = await backupConnection.ExecuteScalarAsync<string>("PRAGMA integrity_check;");
                if (backupIntegrityResult != "ok")
                {
                    return (false, $"Backup integrity check failed: Result: {backupIntegrityResult}");
                }

                var requiredTables = new string[]
                {
            "User", "PartBuy", "PartInfo", "Tasks", "TaskActivity", "Notification",
            "Category", "TaskHelp", "TaskNote", "PlugInNotification"
                };

                foreach (var table in requiredTables)
                {
                    var tableInfo = await backupConnection.GetTableInfoAsync(table);
                    if (tableInfo.Count == 0)
                    {
                        return (false, $"Table '{table}' does not exist in the backup file.");
                    }
                }

                return (true, "Backup integrity check passed. All required tables exist in the backup file.");
            }
            catch (Exception ex)
            {
                return (false, $"Backup integrity check failed: {ex.Message}");
            }
        }



        public async Task<(bool, string)> BackupDatabaseAsync()
        {
            try
            {
                if (File.Exists(_databasePath))
                {
                    // Log the paths being used
                    Console.WriteLine($"Database Path: {_databasePath}");
                    Console.WriteLine($"Backup Path: {_backupPath}");

                    // Perform the in-depth integrity check before the backup
                    var integrityResult = await _database.ExecuteScalarAsync<string>("PRAGMA integrity_check;");
                    if (integrityResult != "ok")
                    {
                        return (false, $"Backup failed: Database integrity check failed. Result: {integrityResult}");
                    }

                    // Check if all required tables exist
                    var requiredTables = new string[]
                    {
                "User", "PartBuy", "PartInfo", "Tasks", "TaskActivity", "Notification",
                "Category", "TaskHelp", "TaskNote", "PlugInNotification"
                    };

                    foreach (var table in requiredTables)
                    {
                        var tableInfo = await _database.GetTableInfoAsync(table);
                        if (tableInfo.Count == 0)
                        {
                            return (false, $"Backup failed: Table '{table}' does not exist in the database.");
                        }
                    }

                    // Ensure previous backup file is deleted before copying
                    if (File.Exists(_backupPath))
                    {
                        File.Delete(_backupPath);
                    }

                    // Perform the manual copy operation using streams
                    using (var sourceStream = new FileStream(_databasePath, FileMode.Open, FileAccess.Read))
                    using (var destinationStream = new FileStream(_backupPath, FileMode.CreateNew, FileAccess.Write))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }

                    // Verify that the backup file was indeed created
                    if (File.Exists(_backupPath))
                    {
                        Console.WriteLine("Backup completed successfully.");

                        // Verify the integrity of the backup file
                        var backupConnection = new SQLiteAsyncConnection(_backupPath);
                        var backupIntegrityResult = await backupConnection.ExecuteScalarAsync<string>("PRAGMA integrity_check;");
                        if (backupIntegrityResult != "ok")
                        {
                            return (false, $"Backup integrity check failed: Result: {backupIntegrityResult}");
                        }

                        foreach (var table in requiredTables)
                        {
                            var tableInfo = await backupConnection.GetTableInfoAsync(table);
                            if (tableInfo.Count == 0)
                            {
                                return (false, $"Backup integrity check failed: Table '{table}' does not exist in the backup file.");
                            }
                        }

                        Console.WriteLine("Backup integrity check passed. All required tables and row counts match in the backup file.");
                        return (true, $"Backup completed successfully. Backup file located at: {_backupPath}");
                    }
                    else
                    {
                        return (false, $"Backup failed: Unable to verify the creation of backup file at: {_backupPath}");
                    }
                }
                else
                {
                    return (false, $"Backup failed: Original database file not found at: {_databasePath}");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return (false, $"Backup failed: Unauthorized access - {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Backup failed: {ex.Message}");
            }
        }



        public async Task<(bool, string)> RestoreDatabaseAsync()
        {
            try
            {
                if (File.Exists(_backupPath))
                {
                    // Ensure previous database file is deleted before copying
                    if (File.Exists(_databasePath))
                    {
                        File.Delete(_databasePath);
                    }

                    // Perform the manual copy operation using streams
                    using (var sourceStream = new FileStream(_backupPath, FileMode.Open, FileAccess.Read))
                    using (var destinationStream = new FileStream(_databasePath, FileMode.CreateNew, FileAccess.Write))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }

                    // Verify that the database file was indeed restored
                    if (File.Exists(_databasePath))
                    {
                        return (true, $"Restore completed successfully. Database file restored from: {_backupPath}");
                    }
                    else
                    {
                        return (false, $"Restore failed: Unable to verify the restoration of database file to: {_databasePath}");
                    }
                }
                else
                {
                    return (false, $"Restore failed: Backup file not found at: {_backupPath}");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return (false, $"Restore failed: Unauthorized access - {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Restore failed: {ex.Message}");
            }
        }

    }
}
