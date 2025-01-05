using SQLite;
using System;
using System.Collections.Generic;
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
            try
            {
                _database = DatabaseConnection.GetConnectionAsync().Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing database connection: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> AddUserAsync(User user)
        {
            try
            {
                // Hash the password before storing
                user.Password = PasswordHelper.HashPassword(user.Password);

                // Store the hashed password in SecureStorage
                await SecureStorage.SetAsync(user.UserName, user.Password);

                return await _database.InsertAsync(user) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding user: {ex.Message}");
                return false;
            }
        }

        public async Task<User> GetUserAsync(int userId)
        {
            try
            {
                return await _database.FindAsync<User>(userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user with ID {userId}: {ex.Message}");
                return null;
            }
        }

        // Get all Users
        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                // Fetch all Users
                return await _database.QueryAsync<User>("SELECT * FROM User");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching open users: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<User> GetUserByUsernameAndPassword(string username, string password)
        {
            try
            {
                // Step 1: Retrieve user from the SQLite database
                var user = await _database.Table<User>().FirstOrDefaultAsync(u => u.UserName == username);
                if (user != null)
                {
                    // Step 2: Retrieve hashed password from SecureStorage
                    var storedPasswordHash = await SecureStorage.GetAsync(username);

                    // Step 3: Verify the entered password with the stored hashed password
                    if (storedPasswordHash != null && PasswordHelper.VerifyPassword(password, storedPasswordHash))
                    {
                        return user;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user by username and password: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                return await _database.UpdateAsync(user) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user with ID {user.UserId}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _database.FindAsync<User>(userId);
                if (user != null)
                {
                    return await _database.DeleteAsync(user) > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting user with ID {userId}: {ex.Message}");
                return false;
            }
        }
    }
}
