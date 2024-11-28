using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaintainHome.Models;
using MaintainHome.Helper;


namespace MaintainHome.Database
{
    public class UserRepository
    {
        private readonly SQLiteAsyncConnection _database;

        public UserRepository()
        {
            _database = DatabaseConnection.GetConnectionAsync().Result;
        }

        public async Task<bool> AddUserAsync(User user)
        {
            // Hash the password before storing
            user.Password = PasswordHelper.HashPassword(user.Password);

            // Store the hashed password in SecureStorage
            await SecureStorage.SetAsync(user.UserName, user.Password);

            return await _database.InsertAsync(user) > 0;
        }

        public async Task<User> GetUserAsync(int userId)
        {
            return await _database.FindAsync<User>(userId);
        }

        //================================================
            public async Task<User> GetUserByUsernameAndPassword(string username, string password)
            {
                // Step 1: Retrieve user from the SQLite database
                var user = await _database.Table<User>().FirstOrDefaultAsync(u => u.UserName == username);
                if (user != null)
                {
                    //await Application.Current.MainPage.DisplayAlert("User Found", $"User: {user.UserName}", "OK");

                    // Step 2: Retrieve hashed password from SecureStorage
                    var storedPasswordHash = await SecureStorage.GetAsync(username);
                    //await Application.Current.MainPage.DisplayAlert("Retrieved Hashed Password", storedPasswordHash, "OK");

                    // Step 3: Verify the entered password with the stored hashed password
                    if (storedPasswordHash != null && PasswordHelper.VerifyPassword(password, storedPasswordHash))
                    {
                        //await Application.Current.MainPage.DisplayAlert("Verification", "Password verification succeeded.", "OK");
                        return user;
                    }
                    else
                    {
                        //await Application.Current.MainPage.DisplayAlert("Verification", "Password verification failed.", "OK");
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("User Not Found", "User not found in the database.", "OK");
                }
                return null;
            }
    //================================================



    public async Task<bool> UpdateUserAsync(User user)
        {
            return await _database.UpdateAsync(user) > 0;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _database.FindAsync<User>(userId);
            if (user != null)
            {
                return await _database.DeleteAsync(user) > 0;
            }
            return false;
        }
    }
}
