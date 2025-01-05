using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MaintainHome.Models;

namespace MaintainHome.Database
{
    public class NotificationRepository
    {
        private readonly SQLiteAsyncConnection _database;

        public NotificationRepository()
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

        // Create (Add) Notification
        public async Task<bool> AddNotificationAsync(Notification notification)
        {
            try
            {
                return await _database.InsertAsync(notification) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding notification: {ex.Message}");
                return false;
            }
        }

        // Read (Get) all Notifications for a specific Task ID
        public async Task<List<Notification>> GetNotificationsAsyncByTaskId(int taskId)
        {
            try
            {
                return await _database.Table<Notification>().Where(t => t.TaskId == taskId).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving notifications for task ID {taskId}: {ex.Message}");
                return new List<Notification>();
            }
        }

        // Read (Get) Notification by ID
        public async Task<Notification> GetNotificationAsync(int notificationId)
        {
            try
            {
                return await _database.FindAsync<Notification>(notificationId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving notification with ID {notificationId}: {ex.Message}");
                return null;
            }
        }

        // Update Notification
        public async Task<bool> UpdateNotificationAsync(Notification notification)
        {
            try
            {
                return await _database.UpdateAsync(notification) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating notification with ID {notification.NotificationId}: {ex.Message}");
                return false;
            }
        }

        // Delete Notification
        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            try
            {
                var notification = await _database.FindAsync<Notification>(notificationId);
                if (notification != null)
                {
                    return await _database.DeleteAsync(notification) > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting notification with ID {notificationId}: {ex.Message}");
                return false;
            }
        }

        // Delete all notifications for a specific Task ID
        public async Task DeleteNotificationsByTaskIdAsync(int taskId)
        {
            try
            {
                var notifications = await _database.Table<Notification>().Where(n => n.TaskId == taskId).ToListAsync();
                foreach (var notification in notifications)
                {
                    await _database.DeleteAsync(notification);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting notifications for task ID {taskId}: {ex.Message}");
            }
        }
    }
}
