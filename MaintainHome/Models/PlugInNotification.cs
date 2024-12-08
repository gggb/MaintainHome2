using SQLite;
using System;

namespace MaintainHome.Models
{
    public class PlugInNotification
    {
        [PrimaryKey, AutoIncrement]
        public int NotificationId { get; set; } // Primary key
        [Indexed]
        public int TaskId { get; set; } // Foreign key to Tasks table
        public string NotificationType { get; set; } // Type of notification (e.g., Reminder, Overdue, Completion, General)
        public DateTime NotificationDate { get; set; } // Date and time when the notification is to be sent
        public string NotificationTitle { get; set; } // The message to be sent in the notification
        public string NotificationDescription { get; set; }
        public bool NotificationIsSent { get; set; } // Flag to indicate if the notification has been sent
    }
}
