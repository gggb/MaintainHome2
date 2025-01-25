using MaintainHome.Database;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Controls;
using SQLite;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MaintainHome.Views
{
    public partial class DataMaintenance : ContentPage
    {
        private MainMenuPage _mainMenuPage;
        private Dashboard _dashboard; 
        private readonly BackupRestoreRepository _backupRestoreRepository;
        private readonly SQLiteAsyncConnection _connection;

        //===============================================================
        private double _backupLogHeight = 0; 
        private double _restoreLogHeight = 0; 
        public double BackupLogHeight 
        { 
            get => _backupLogHeight; 
            set 
            { 
                if (_backupLogHeight != value) 
                { 
                    _backupLogHeight = value; 
                    OnPropertyChanged(nameof(BackupLogHeight)); 
                } 
            } 
        }
        public double RestoreLogHeight
        {
            get => _restoreLogHeight;
            set
            {
                if (_restoreLogHeight != value)
                {
                    _restoreLogHeight = value;
                    OnPropertyChanged(nameof(RestoreLogHeight));
                }
            }
        }
        //===============================================================



        public DataMaintenance(Dashboard dashboard)
        {
            InitializeComponent();
            BindingContext = this;
            _dashboard = dashboard;

            // Initialize the database and backup paths
            _backupRestoreRepository = new BackupRestoreRepository();

            _connection = DatabaseConnection.GetConnectionAsync().Result;

            // Ensure the Backup folder exists
            Directory.CreateDirectory(Path.Combine(FileSystem.AppDataDirectory, "Backup"));
        }

        private async void OnBackupClicked(object sender, EventArgs e)
        {
            // Update the height request for the backup log
            BackupLogHeight = 150;
            RestoreLogHeight = 0;

            // Initial UI update
            Dispatcher.Dispatch(() =>
            {
                BackupStatusLabel.Text = "Backup Status: In progress...";
                BackupLogEditor.Text = "  -Starting backup...\n";
                Console.WriteLine($"Initial UI Update: {BackupStatusLabel.Text}");
            });

            // Step 1: DB Integrity Check
            var (integritySuccess, integrityMessage) = await _backupRestoreRepository.CheckDatabaseIntegrityAsync();
            Dispatcher.Dispatch(() =>
            {
                BackupLogEditor.Text += $"  -DB Integrity Check: {integrityMessage}\n";
                Console.WriteLine($"DB Integrity Check: {integrityMessage}");
            });
            if (!integritySuccess)
            {
                Dispatcher.Dispatch(() =>
                {
                    BackupStatusLabel.Text = "Backup Status: Failed";
                });
                return;
            }

            // Step 2: Check Required Tables
            var (tablesSuccess, tablesMessage) = await _backupRestoreRepository.CheckRequiredTablesAsync();
            Dispatcher.Dispatch(() =>
            {
                BackupLogEditor.Text += $"  -Backup Tables Check: {tablesMessage}\n";
                Console.WriteLine($"Backup Tables Check: {tablesMessage}");
            });
            if (!tablesSuccess)
            {
                Dispatcher.Dispatch(() =>
                {
                    BackupStatusLabel.Text = "  Backup Status: Failed";
                });
                return;
            }

            // Step 3: Perform Backup
            var (backupSuccess, backupMessage) = await _backupRestoreRepository.PerformBackupAsync();
            Dispatcher.Dispatch(() =>
            {
                BackupLogEditor.Text += $"  -Backup: {backupMessage}\n";
                Console.WriteLine($"Backup: {backupMessage}");
            });
            if (!backupSuccess)
            {
                Dispatcher.Dispatch(() =>
                {
                    BackupStatusLabel.Text = "Backup Status: Failed";
                });
                return;
            }

            // Step 4: Backup Integrity Check: runs integrity check and table check on the backup database
            var (backupIntegritySuccess, backupIntegrityMessage) = await _backupRestoreRepository.CheckBackupIntegrityAsync();
            Dispatcher.Dispatch(() =>
            {
                BackupLogEditor.Text += $"  -Backup Integrity Check: {backupIntegrityMessage}\n";
                Console.WriteLine($"Backup Integrity Check: {backupIntegrityMessage}");
            });

            //Ensure the backup file is writable
            string backupFilePath = "/data/user/0/com.mycompany.HomeUpkeepPro/files/Backup/backup.db3"; // Update with the correct path
            if (File.Exists(backupFilePath))
            {
                Console.WriteLine("Backup file exists.");
                var attributes = File.GetAttributes(backupFilePath);
                Console.WriteLine($"Backup file attributes: {attributes}");

                // Ensure the file is writable
                File.SetAttributes(backupFilePath, attributes & ~FileAttributes.ReadOnly);
                Console.WriteLine("Backup file set to writable.");
            }
            else
            {
                Console.WriteLine("Backup file does not exist.");
            }

            // Final UI update
            Dispatcher.Dispatch(() =>
            {
                BackupStatusLabel.Text = backupIntegritySuccess ? "Backup Status: Completed" : "Backup Status: Failed";
                Console.WriteLine($"Final UI Update: {BackupStatusLabel.Text}");
            });
        }



        private async void OnRestoreClicked(object sender, EventArgs e)
        {
            try
            {
                // Update the height request for the backup log
                RestoreLogHeight = 200;
                BackupLogHeight = 0;

                // Initial UI update
                Dispatcher.Dispatch(() =>
                {
                    RestoreStatusLabel.Text = "Restore Status: In progress...";
                    RestoreLogEditor.Text = "Starting restore...\n";
                    Console.WriteLine($"Initial UI Update: {RestoreStatusLabel.Text}");
                });

                // Step 1: Perform Backup Integrity Check Before Restore
                Dispatcher.Dispatch(() => RestoreLogEditor.Text += "  -Starting Backup Integrity Check...\n");
                var (backupIntegrityBeforeSuccess, backupIntegrityBeforeMessage) = await _backupRestoreRepository.CheckBackupIntegrityBeforeRestoreAsync();
                Dispatcher.Dispatch(() =>
                {
                    RestoreLogEditor.Text += $"  -Backup Integrity Check before Restore: {backupIntegrityBeforeMessage}\n";
                    Console.WriteLine($"Backup Integrity Check before Restore: {backupIntegrityBeforeMessage}");
                });
                if (!backupIntegrityBeforeSuccess)
                {
                    Dispatcher.Dispatch(() =>
                    {
                        RestoreStatusLabel.Text = "Restore Status: Failed";
                        RestoreLogEditor.Text += "Restore process halted.\n";
                    });
                    return;
                }

                // Step 2: Perform Restore
                Dispatcher.Dispatch(() => RestoreLogEditor.Text += "  -Starting Restore...\n");
                var (restoreSuccess, restoreMessage) = await _backupRestoreRepository.PerformRestoreAsync();
                Dispatcher.Dispatch(() =>
                {
                    RestoreLogEditor.Text += $"  -Restore: {restoreMessage}\n";
                    Console.WriteLine($"Restore: {restoreMessage}");
                });
                if (!restoreSuccess)
                {
                    Dispatcher.Dispatch(() =>
                    {
                        RestoreStatusLabel.Text = "Restore Status: Failed";
                        RestoreLogEditor.Text += "Restore process halted.\n";
                    });
                    return;
                }

                // Step 3: DB Integrity Check After Restore
                Dispatcher.Dispatch(() => RestoreLogEditor.Text += "  -Starting DB Integrity Check after Restore...\n");
                var (integrityAfterSuccess, integrityAfterMessage) = await _backupRestoreRepository.CheckDatabaseIntegrityAfterRestoreAsync();
                Dispatcher.Dispatch(() =>
                {
                    RestoreLogEditor.Text += $"  -DB Integrity Check after Restore: {integrityAfterMessage}\n";
                    Console.WriteLine($"DB Integrity Check after Restore: {integrityAfterMessage}");
                });

                // Ensure the database file is writable
                string dbPath = _connection.DatabasePath; // Using the _connection.DatabasePath
                Console.WriteLine($"Database file path: {dbPath}");

                // Log file attributes and existence
                if (File.Exists(dbPath))
                {
                    Console.WriteLine("Database file exists.");
                    var attributes = File.GetAttributes(dbPath);
                    Console.WriteLine($"Database file attributes: {attributes}");

                    // Ensure the file is writable
                    File.SetAttributes(dbPath, attributes & ~FileAttributes.ReadOnly);
                    Console.WriteLine("Database file set to writable.");
                }
                else
                {
                    Console.WriteLine("Database file does not exist.");
                    RestoreStatusLabel.Text = "Restore Status: Failed";
                    RestoreLogEditor.Text += "Database file does not exist.\n";
                    return;
                }

#if ANDROID
        // For Android, ensure file permissions are set
        Java.IO.File file = new Java.IO.File(dbPath);
        file.SetWritable(true, false); // Set to writable for owner and group
        file.SetReadable(true, false); // Ensure it's readable for owner and group
        Console.WriteLine($"Database file {dbPath} set to writable on Android.");
#endif

                // Simulate app restart by closing the connection and creating a new one
                await SimulateAppRestartAsync(dbPath);

                // Final UI update
                Dispatcher.Dispatch(async () =>
                {
                    RestoreStatusLabel.Text = integrityAfterSuccess ? "Restore Status: Completed" : "Restore Status: Failed";
                    RestoreLogEditor.Text += integrityAfterSuccess ? "Restore process completed successfully.\n" : "Restore process failed.\n";
                    Console.WriteLine($"Final UI Update: {RestoreStatusLabel.Text}");

                    if (integrityAfterSuccess)
                    {
                        // Call the method to update the dashboard
                        await _dashboard.LoadDataAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                // Log the exception or display an alert to inform the user
                Dispatcher.Dispatch(() =>
                {
                    RestoreStatusLabel.Text = "Restore Status: Error";
                    RestoreLogEditor.Text += $"An error occurred: {ex.Message}\nStack Trace: {ex.StackTrace}\n";
                    Console.WriteLine($"An error occurred: {ex.Message}\nStack Trace: {ex.StackTrace}");
                });
            }
        }

        private async Task SimulateAppRestartAsync(string dbPath)
        {
            // Close the existing connection
            await _connection.CloseAsync();

            // Create a new local connection for the PRAGMA operation
            var newConnection = new SQLiteAsyncConnection(dbPath);
            
        }







        private async void OnIntegrityCheckClicked(object sender, EventArgs e)
        {
            Dispatcher.Dispatch(() =>
            {
                IntegrityCheckStatusLabel.Text = "PRAGMA Integrity Check Status: In progress...";
                IntegrityCheckLogEditor.Text = "Starting data integrity check...\n";
            });

            try
            {
                var result = await _connection.ExecuteScalarAsync<string>("PRAGMA integrity_check;");
                Dispatcher.Dispatch(() =>
                {
                    IntegrityCheckStatusLabel.Text = "PRAGMA Integrity Check Status: Completed";
                    IntegrityCheckLogEditor.Text += $"PRAGMA Integrity check result: {result}\n";
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Dispatch(() =>
                {
                    IntegrityCheckStatusLabel.Text = "Integrity Check Status: Failed";
                    IntegrityCheckLogEditor.Text += $"Integrity check failed: {ex.Message}\n";
                });
            }
        }

        private async void OnVacuumClicked(object sender, EventArgs e)
        {
            VacuumStatusLabel.Text = "VACUUM Status: In progress...";
            VacuumLogEditor.Text = "Starting VACUUM procedure...\n";

            try
            {
                await _connection.ExecuteAsync("VACUUM;");
                VacuumStatusLabel.Text = "VACUUM Status: Completed";
                VacuumLogEditor.Text += "VACUUM procedure completed successfully.\n";
            }
            catch (Exception ex)
            {
                VacuumStatusLabel.Text = "VACUUM Status: Failed";
                VacuumLogEditor.Text += $"VACUUM procedure failed: {ex.Message}\n";
            }
        }


        private async void OnVacuumClicked9(object sender, EventArgs e)
        {
            VacuumStatusLabel.Text = "VACUUM Status: In progress...";
            VacuumLogEditor.Text = "Starting VACUUM procedure...\n";

            try
            {
                await _connection.ExecuteAsync("VACUUM;");
                VacuumStatusLabel.Text = "VACUUM Status: Completed";
                VacuumLogEditor.Text += "VACUUM procedure completed successfully.\n";
            }
            catch (Exception ex)
            {
                VacuumStatusLabel.Text = "VACUUM Status: Failed";
                VacuumLogEditor.Text += $"VACUUM procedure failed: {ex.Message}\n";
                Console.WriteLine($"VACUUM procedure failed: {ex.Message}");
            }
        }

        public ICommand NavigateBackCommand => new Command(async () => await Shell.Current.GoToAsync(".."));
    }
}
