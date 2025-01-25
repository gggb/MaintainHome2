using Microsoft.Maui;
using Microsoft.Maui.Controls;
using MaintainHome.Views;
using MaintainHome.Database;
using System.Threading.Tasks;
using MaintainHome.Models;
using Plugin.LocalNotification.EventArgs;
using Plugin.LocalNotification;
using SQLite;

namespace MaintainHome
{
    public partial class App : Application
    {
        public static User CurrentUser { get; set; } // Track the current user
        public App()
        {
            InitializeComponent();

            // Set the app theme to follow the system theme
            Application.Current.UserAppTheme = AppTheme.Unspecified;

            /// Check and initialize the database if needed 
            InitializeDatabaseIfNeeded();
            

            MainPage = new NavigationPage(new Login());
            
            LocalNotificationCenter.Current.RequestNotificationPermission();  // Request notification permission
            LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationActionTapped;  // Handle notification action tapped
        }

        
        public async void InitializeDatabaseIfNeeded()
        {
            var connection = await DatabaseConnection.GetConnectionAsync();
            try
            {

                // Check if the User table exists (if it doesn't exist, the database has not been created and loaded [due to first time implementation]).
                var result = await connection.ExecuteScalarAsync<string>("SELECT name FROM sqlite_master WHERE type='table' AND name='User'");

                if (string.IsNullOrEmpty(result)) // User table doesn't exist
                {
                    // Prompt the user for confirmation
                    bool initialize = await Application.Current.MainPage.DisplayAlert(
                            "Initialize Database",
                            "If this is the first time using this application, the database needs to be set up for you to log in. Please select 'Yes' to initialize the database for the first time. If you previously set up data, all of the data will be lost. Do you want to proceed?",
                            "Yes",
                            "No");
                    if (initialize)
                    {
                        await DatabaseInitializer.InitializeAsync(connection);
                    }
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking database table: {ex.Message}");
                throw;
            }
        }

        private async void InitializeDatabase()
        {
            // Get the database connection
            var database = await DatabaseConnection.GetConnectionAsync();

            // Initialize the database
            await DatabaseInitializer.InitializeAsync(database);
        }


        private void OnNotificationActionTapped(NotificationActionEventArgs e)
        {
            if (e.IsDismissed) return;

            if (e.IsTapped)
            {
                // Handle the notification tap action
                // Navigate to the relevant page or perform any other action
            }
        }


    }
}
