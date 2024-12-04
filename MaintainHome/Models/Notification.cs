using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;

namespace MaintainHome.Models
{
    public class Notification
    {
        [PrimaryKey, AutoIncrement]
        public int NotificationId { get; set; }
        public int TaskId { get; set; }
        //public int UserId { get; set; }
        public string? Message { get; set; }
        public string? TargetName { get; set; }
        public string? TargetEmail { get; set; }
        public string? TargetPhone { get; set; }
    }
}