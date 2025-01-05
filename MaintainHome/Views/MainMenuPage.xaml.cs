using MaintainHome.Views;
using MaintainHome.Models;  
using MaintainHome.Database;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace MaintainHome.Views
{
    public partial class MainMenuPage : FlyoutPage
    {
        public Dashboard _dashboard { get; private set; }// Declare the _dashboard field
        public MainMenuPage()
        {
            InitializeComponent();
            _dashboard = new Dashboard();
        }

        private void OnLoginClicked(object sender, EventArgs e)
        {
            // Correctly reference the Detail NavigationPage and set it
            Detail = new NavigationPage(new Login());
            IsPresented = false; // Close the flyout menu
        }

        private void OnDashboardClicked(object sender, EventArgs e)
        {
            // Correctly reference the Detail NavigationPage and set it
            Detail = new NavigationPage(_dashboard);
            IsPresented = false; // Close the flyout menu
        }

        private void OnReportsClicked(object sender, EventArgs e)
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
        private void OnInitialSetupClicked(object sender, EventArgs e)
        {
            OnInitialSetupClickedAsync(sender, e).ConfigureAwait(false);
        }

        private async Task OnInitialSetupClickedAsync(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Confirm Action", "This initial set-up overrides task-related data with default data! Are you sure you want to overide data with default task data?", "Yes", "No");
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

        private void OnMaintenanceClicked(object sender, EventArgs e)
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

    }
}
