using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaintainHome.Models;

namespace MaintainHome.Database
{
    public class NotificationRepository
    {
        private readonly SQLiteAsyncConnection _database;

        public NotificationRepository()
        {
            _database = DatabaseConnection.GetConnectionAsync().Result;
        }

        // Create (Add) Notification
        public async Task<bool> AddNotificationAsync(Notification notification)
        {
            return await _database.InsertAsync(notification) > 0;
        }

        // Read (Get) all Notifications for a specific Task ID
        public async Task<List<Notification>> GetNotificationsAsyncByTaskId(int taskId)
        {
            return await _database.Table<Notification>().Where(t => t.TaskId == taskId).ToListAsync();
        }




        // Read (Get) Notification by ID
        public async Task<Notification> GetNotificationAsync(int notificationId)
        {
            return await _database.FindAsync<Notification>(notificationId);
        }

        // Update Notification
        public async Task<bool> UpdateNotificationAsync(Notification notification)
        {
            return await _database.UpdateAsync(notification) > 0;
        }

        // Delete Notification
        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            var notification = await _database.FindAsync<Notification>(notificationId);
            if (notification != null)
            {
                return await _database.DeleteAsync(notification) > 0;
            }
            return false;
        }
    }
}
