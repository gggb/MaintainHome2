using Microsoft.Maui.Controls;
using System;
using MaintainHome.Database;
using MaintainHome.Models;
using MaintainHome.Helper;
using MaintainHome.Behaviors;

namespace MaintainHome.Views
{
    public partial class Registration : ContentPage
    {
        private readonly UserRepository _userRepository;

        public Registration()
        {
            InitializeComponent();
            _userRepository = new UserRepository();
        }

        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            var username = UsernameEntry.Text;
            var email = EmailEntry.Text;
            var phone = PhoneEntry.Text;
            var password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(password))
            {
                RegisterMessageLabel.Text = "All fields are required.";
                return;
            }

            // Hash the password before storing
            var hashedPassword = PasswordHelper.HashPassword(password);


            var newUser = new User
            {
                UserName = username,
                Email = email,
                Phone = phone,
                Password = password
            };

            var result = await _userRepository.AddUserAsync(newUser);
            if (result)
            {
                await Application.Current.MainPage.DisplayAlert("Success", "Account created successfully.", "OK");
                // Navigate back to login page
                await Navigation.PopAsync();
            }
            else
            {
                RegisterMessageLabel.Text = "Username already exists. Please choose a different username.";
            }
        }

        private async void OnBackToLoginButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
