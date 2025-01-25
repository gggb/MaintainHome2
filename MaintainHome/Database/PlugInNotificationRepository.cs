using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
//using CloudKit;
using MaintainHome.Models;
using SQLite;

namespace MaintainHome.Database
{
    public class PlugInNotificationRepository
    {
        private readonly SQLiteAsyncConnection _database;

        public PlugInNotificationRepository()
        {
            try
            {
                _database = DatabaseConnection.GetConnectionAsync().Result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing database connection: {ex.Message}");
            }
        }
        
        public async Task AddOrUpdatePlugInNotificationAsync(PlugInNotification notification)
        {
            try
            {
                // Using a single query to check existence and then update or insert
                var existingNotifications = await _database.Table<PlugInNotification>()
                                                           .Where(n => n.TaskId == notification.TaskId)
                                                           .ToListAsync();

                if (existingNotifications.Count > 1)   // This should not occur... EVER!!!!
                {
                    Debug.WriteLine($"Warning: Multiple notifications found for TaskId {notification.TaskId}"); 
                    // Optionally handle the cleanup of duplicates here
                }

                var existingNotification = existingNotifications.FirstOrDefault();
                if (existingNotification != null)    // if record exists, update it.
                {
                    existingNotification.NotificationId = notification.NotificationId;
                    existingNotification.TaskId = notification.TaskId;
                    existingNotification.NotificationType = notification.NotificationType;
                    existingNotification.NotificationDate = notification.NotificationDate;
                    existingNotification.NotificationTitle = notification.NotificationTitle;
                    existingNotification.NotificationDescription = notification.NotificationDescription;
                    existingNotification.NotificationIsSent = notification.NotificationIsSent;

                    await _database.UpdateAsync(existingNotification);
                }
                else    // if record does not exist, add it 
                {
                    await _database.InsertAsync(notification);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding or updating PlugInNotification: {ex.Message}");
            }
        }

        // Read (Get) PlugInNotification for a specific TaskId
        public async Task<PlugInNotification> GetPlugInNotificationByTaskIdAsync(int taskId)
        {
            try
            {
                return await _database.Table<PlugInNotification>().FirstOrDefaultAsync(n => n.TaskId == taskId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting PlugInNotification by TaskId: {ex.Message}");
                return null;
            }
        }

        public async Task<List<PlugInNotification>> GetPlugInNotificationsByTaskIdAsync(int taskId) 
        {
            try
            {
                return await _database.Table<PlugInNotification>()
                .Where(n => n.TaskId == taskId)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting PlugInNotifications by TaskId: {ex.Message}");
                return null;
            }
        }

       // Delete PlugInNotification
        public async Task<bool> DeletePlugInNotificationAsync(int notificationId)
        {
            try
            {
                var notification = await _database.FindAsync<PlugInNotification>(notificationId);
                if (notification != null)
                {
                    return await _database.DeleteAsync(notification) > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting PlugInNotification: {ex.Message}");
                return false;
            }
        }
    }
}
