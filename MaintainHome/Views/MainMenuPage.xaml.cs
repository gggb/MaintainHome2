using MaintainHome.Views;
using MaintainHome.Models;
using MaintainHome.Database;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls.PlatformConfiguration;
//using AVFoundation;

namespace MaintainHome.Views
{
    public partial class MainMenuPage : FlyoutPage
    {
        public Dashboard _dashboard { get; private set; } // Declare the _dashboard field

        public MainMenuPage()
        {
            InitializeComponent();
            _dashboard = new Dashboard();
        }

        private void OnLoginClicked(object sender, EventArgs e)
        {
            try
            {
                // Correctly reference the Detail NavigationPage and set it
                Detail = new NavigationPage(new Login());
                IsPresented = false; // Close the flyout menu
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while navigating to Login: {ex.Message}");
                DisplayAlert("Error", $"An error occurred while navigating to Login: {ex.Message}", "OK");
            }
        }

        private void OnDashboardClicked(object sender, EventArgs e)
        {
            try
            {
                // Correctly reference the Detail NavigationPage and set it
                Detail = new NavigationPage(_dashboard);
                IsPresented = false; // Close the flyout menu
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while navigating to Dashboard: {ex.Message}");
                DisplayAlert("Error", $"An error occurred while navigating to Dashboard: {ex.Message}", "OK");
            }
        }

        private void OnReportsClicked(object sender, EventArgs e)
        {
            try
            {
                if (Detail is NavigationPage navigationPage)
                {
                    navigationPage.PushAsync(new Reports());
                }
                else
                {
                    Detail = new NavigationPage(new Reports());
                }
                IsPresented = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while navigating to Reports: {ex.Message}");
                DisplayAlert("Error", $"An error occurred while navigating to Reports: {ex.Message}", "OK");
            }
        }

        private void OnInitialSetupClicked(object sender, EventArgs e)
        {
            OnInitialSetupClickedAsync(sender, e).ConfigureAwait(false);
        }

        private async Task OnInitialSetupClickedAsync(object sender, EventArgs e)
        {
            try
            {
                bool answer = await DisplayAlert("Confirm Action", "This initial set-up overrides task-related data with default data! Are you sure you want to override data with default task data?", "Yes", "No");
                if (!answer)
                {
                    return; // If "No" is selected, return and do not proceed further
                }

                // Get the database connection
                var database = await DatabaseConnection.GetConnectionAsync();

                // Initialize the database
                await DatabaseInitializer.InitializeAsync(database);

                await DisplayAlert("Action Alert", "Default task-related data has been loaded.", "OK");

                OnDashboardClicked(sender, e);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred during initial setup: {ex.Message}");
                await DisplayAlert("Error", $"An error occurred during initial setup: {ex.Message}", "OK");
            }
        }

        private void OnMaintenanceClicked(object sender, EventArgs e)
        {
            try
            {
                if (Detail is NavigationPage navigationPage)
                {
                    navigationPage.PushAsync(new DataMaintenance(_dashboard));
                }
                else
                {
                    Detail = new NavigationPage(new DataMaintenance(_dashboard));
                }
                IsPresented = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while navigating to Maintenance: {ex.Message}");
                DisplayAlert("Error", $"An error occurred while navigating to Maintenance: {ex.Message}", "OK");
            }
        }

        private async void OnExitClicked(object sender, EventArgs e)
        {
            try
            {
                bool answer = await DisplayAlert("Exit", "Do you really want to exit?", "Yes", "No");
                if (answer)
                {
#if ANDROID
                    // Try to close the application using FinishAffinity
                    Platform.CurrentActivity.FinishAffinity();

                    // Introduce a small delay to ensure the first command has time to execute
                    await Task.Delay(500);

                    // Check if the app is still running before attempting to call FinishAndRemoveTask
                    var activity = (MainActivity)Platform.CurrentActivity;
                    if (!activity.IsFinishing) // Check if the activity is finishing; if it isn't, try the second method.
                    {
                        activity.FinishAndRemoveTask();
                    }
#elif WINDOWS
                    // Exit the application on Windows
                    System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
#else
                    // Other platform-specific exit code
#endif
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred during exit: {ex.Message}");
                await DisplayAlert("Error", $"An error occurred during exit: {ex.Message}", "OK");
            }
        }
    }
}
