using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MaintainHome.Models;
using SQLite;

namespace MaintainHome.Database
{
    public class PlugInNotificationRepository
    {
        private readonly SQLiteAsyncConnection _database;

        public PlugInNotificationRepository()
        {
            _database = DatabaseConnection.GetConnectionAsync().Result;
        }

        // Create (Add) PlugInNotification
        public async Task<bool> AddPlugInNotificationAsync(PlugInNotification notification)
        {
            return await _database.InsertAsync(notification) > 0;
        }

        // Read (Get) PlugInNotification by ID
        public async Task<PlugInNotification> GetPlugInNotificationAsync(int notificationId)
        {
            return await _database.FindAsync<PlugInNotification>(notificationId);
        }

        // Read (Get) all PlugInNotifications for a specific TaskId
        public async Task<List<PlugInNotification>> GetAllPlugInNotificationsByTaskIdAsync(int taskId)
        {
            return await _database.Table<PlugInNotification>().Where(n => n.TaskId == taskId).ToListAsync();
        }

        // Update PlugInNotification
        public async Task<bool> UpdatePlugInNotificationAsync(PlugInNotification notification)
        {
            return await _database.UpdateAsync(notification) > 0;
        }

        // Delete PlugInNotification
        public async Task<bool> DeletePlugInNotificationAsync(int notificationId)
        {
            var notification = await _database.FindAsync<PlugInNotification>(notificationId);
            if (notification != null)
            {
                return await _database.DeleteAsync(notification) > 0;
            }
            return false;
        }
    }
}
