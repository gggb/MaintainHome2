using Microsoft.Maui;
using Microsoft.Maui.Controls;
using MaintainHome.Views;
using MaintainHome.Database;
using System.Threading.Tasks;

namespace MaintainHome
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Call the asynchronous database initialization method
            InitializeDatabase();

            MainPage = new NavigationPage(new Login());
        }

        private async void InitializeDatabase()
        {
            // Get the database connection
            var database = await DatabaseConnection.GetConnectionAsync();

            // Initialize the database
            await DatabaseInitializer.InitializeAsync(database);
        }
    }
}
