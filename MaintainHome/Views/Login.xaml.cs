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
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            UsernameEntry.Text = "billbyrd";
            PasswordEntry.Text = "gggb0211";
            var username = UsernameEntry.Text;
            var password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                //LoginMessageLabel.Text = "Please enter both username and password.";
                return;
            }

            var user = await _userRepository.GetUserByUsernameAndPassword(username, password);
            if (user != null)
            {
                //await Application.Current.MainPage.DisplayAlert("Success", "Login successful.", "OK");
                await Navigation.PushAsync(new Dashboard());
                // Navigate to the next page or perform any other action
            }
            else
            {
                //LoginMessageLabel.Text = "Invalid username or password.";
            }
        }

        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Registration());
        }
    }
}
