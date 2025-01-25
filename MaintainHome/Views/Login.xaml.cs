using Microsoft.Maui.Controls;
using System;
using MaintainHome.Database;
using MaintainHome.Models;

namespace MaintainHome.Views
{
    public partial class Login : ContentPage
    {
        private readonly UserRepository _userRepository;

        public Login()
        {
            InitializeComponent();
            _userRepository = new UserRepository();

            // used to streamline login during debugging
            //UsernameEntry.Text = "billbyrd";
            //PasswordEntry.Text = "gggb0211";            
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var username = UsernameEntry.Text;
                var password = PasswordEntry.Text;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    LoginMessageLabel.Text = "Please enter both username and password.";
                    return;
                }

                var user = await _userRepository.GetUserByUsernameAndPassword(username, password);
                if (user != null)
                {
                    // Set the current user in the App
                    App.CurrentUser = user;

                    //await Application.Current.MainPage.DisplayAlert("Success", "Login successful.", "OK");
                    //await Navigation.PushAsync(new Dashboard());
                    //await Navigation.PushAsync(new MainMenuPage());
                    Application.Current.MainPage = new MainMenuPage();
                }
                else
                {
                    LoginMessageLabel.Text = "Invalid username or password.";
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during login
                await DisplayAlert("Error", $"An error occurred during login: {ex.Message}", "OK");
            }
        }

        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Registration());
        }
    }
}

