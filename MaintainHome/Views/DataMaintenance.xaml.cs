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
            Device.BeginInvokeOnMainThread(() =>
            {
                BackupStatusLabel.Text = "Backup Status: In progress...";
                BackupLogEditor.Text = "  -Starting backup...\n";
                Console.WriteLine($"Initial UI Update: {BackupStatusLabel.Text}");
            });

            // Step 1: DB Integrity Check
            var (integritySuccess, integrityMessage) = await _backupRestoreRepository.CheckDatabaseIntegrityAsync();
            Device.BeginInvokeOnMainThread(() =>
            {
                BackupLogEditor.Text += $"  -DB Integrity Check: {integrityMessage}\n";
                Console.WriteLine($"DB Integrity Check: {integrityMessage}");
            });
            if (!integritySuccess)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    BackupStatusLabel.Text = "Backup Status: Failed";
                });
                return;
            }

            // Step 2: Check Required Tables
            var (tablesSuccess, tablesMessage) = await _backupRestoreRepository.CheckRequiredTablesAsync();
            Device.BeginInvokeOnMainThread(() =>
            {
                BackupLogEditor.Text += $"  -Backup Tables Check: {tablesMessage}\n";
                Console.WriteLine($"Backup Tables Check: {tablesMessage}");
            });
            if (!tablesSuccess)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    BackupStatusLabel.Text = "  Backup Status: Failed";
                });
                return;
            }

            // Step 3: Perform Backup
            var (backupSuccess, backupMessage) = await _backupRestoreRepository.PerformBackupAsync();
            Device.BeginInvokeOnMainThread(() =>
            {
                BackupLogEditor.Text += $"  -Backup: {backupMessage}\n";
                Console.WriteLine($"Backup: {backupMessage}");
            });
            if (!backupSuccess)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    BackupStatusLabel.Text = "Backup Status: Failed";
                });
                return;
            }

            // Step 4: Backup Integrity Check: runs integrity check and table check on the backup database
            var (backupIntegritySuccess, backupIntegrityMessage) = await _backupRestoreRepository.CheckBackupIntegrityAsync();
            Device.BeginInvokeOnMainThread(() =>
            {
                BackupLogEditor.Text += $"  -Backup Integrity Check: {backupIntegrityMessage}\n";
                Console.WriteLine($"Backup Integrity Check: {backupIntegrityMessage}");
            });

            // Final UI update
            Device.BeginInvokeOnMainThread(() =>
            {
                BackupStatusLabel.Text = backupIntegritySuccess ? "Backup Status: Completed" : "Backup Status: Failed";
                Console.WriteLine($"Final UI Update: {BackupStatusLabel.Text}");
            });
        }

        private async void OnRestoreClicked(object sender, EventArgs e)
        {
            // Update the height request for the backup log
            RestoreLogHeight = 165;
            BackupLogHeight = 0;

            // Initial UI update
            Device.BeginInvokeOnMainThread(() =>
            {
                RestoreStatusLabel.Text = "Restore Status: In progress...";
                RestoreLogEditor.Text = "Starting restore...\n";
                Console.WriteLine($"Initial UI Update: {RestoreStatusLabel.Text}");
            });

            // Step 1: Backup Integrity Check Before Restore
            Device.BeginInvokeOnMainThread(() => RestoreLogEditor.Text += "  -Starting Backup Integrity Check...\n");
            (bool backupIntegrityBeforeSuccess, string backupIntegrityBeforeMessage) = await _backupRestoreRepository.CheckBackupIntegrityBeforeRestoreAsync();
            Device.BeginInvokeOnMainThread(() =>
            {
                RestoreLogEditor.Text += $"  -Backup Integrity Check before Restore: {backupIntegrityBeforeMessage}\n";
                Console.WriteLine($"Backup Integrity Check before Restore: {backupIntegrityBeforeMessage}");
            });
            if (!backupIntegrityBeforeSuccess)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    RestoreStatusLabel.Text = "Restore Status: Failed";
                    RestoreLogEditor.Text += "Restore process halted.\n";
                });
                return;
            }

            // Step 2: Perform Restore
            Device.BeginInvokeOnMainThread(() => RestoreLogEditor.Text += "  -Starting Restore...\n");
            (bool restoreSuccess, string restoreMessage) = await _backupRestoreRepository.PerformRestoreAsync();
            Device.BeginInvokeOnMainThread(() =>
            {
                RestoreLogEditor.Text += $"  -Restore: {restoreMessage}\n";
                Console.WriteLine($"Restore: {restoreMessage}");
            });
            if (!restoreSuccess)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    RestoreStatusLabel.Text = "Restore Status: Failed";
                    RestoreLogEditor.Text += "Restore process halted.\n";
                });
                return;
            }

            // Step 3: DB Integrity Check After Restore
            Device.BeginInvokeOnMainThread(() => RestoreLogEditor.Text += "  -Starting DB Integrity Check after Restore...\n");
            (bool integrityAfterSuccess, string integrityAfterMessage) = await _backupRestoreRepository.CheckDatabaseIntegrityAfterRestoreAsync();
            Device.BeginInvokeOnMainThread(() =>
            {
                RestoreLogEditor.Text += $"  -DB Integrity Check after Restore: {integrityAfterMessage}\n";
                Console.WriteLine($"DB Integrity Check after Restore: {integrityAfterMessage}");
            });

            // Final UI update
            Device.BeginInvokeOnMainThread(async () =>
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


        private async void OnIntegrityCheckClicked(object sender, EventArgs e)
        {
            IntegrityCheckStatusLabel.Text = "Integrity Check Status: In progress...";
            IntegrityCheckLogEditor.Text = "Starting data integrity check...\n";

            try
            {
                var result = await _connection.ExecuteScalarAsync<string>("PRAGMA integrity_check;");
                IntegrityCheckStatusLabel.Text = "Integrity Check Status: Completed";
                IntegrityCheckLogEditor.Text += $"Integrity check result: {result}\n";
            }
            catch (Exception ex)
            {
                IntegrityCheckStatusLabel.Text = "Integrity Check Status: Failed";
                IntegrityCheckLogEditor.Text += $"Integrity check failed: {ex.Message}\n";
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

        public ICommand NavigateBackCommand => new Command(async () => await Shell.Current.GoToAsync(".."));
    }
}
