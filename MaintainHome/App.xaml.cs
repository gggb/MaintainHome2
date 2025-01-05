﻿using Microsoft.Maui;
using Microsoft.Maui.Controls;
using MaintainHome.Views;
using MaintainHome.Database;
using System.Threading.Tasks;
using MaintainHome.Models;
using Plugin.LocalNotification.EventArgs;
using Plugin.LocalNotification;

namespace MaintainHome
{
    public partial class App : Application
    {
        public static User CurrentUser { get; set; } // Track the current user
        public App()
        {
            InitializeComponent();

            // Call the asynchronous database initialization method
            //InitializeDatabase();  This method can now be initialized in the flyout menu "Initial Setup" sub menu.

            MainPage = new NavigationPage(new Login());
            


            LocalNotificationCenter.Current.RequestNotificationPermission();  // Request notification permission
            LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationActionTapped;  // Handle notification action tapped

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
