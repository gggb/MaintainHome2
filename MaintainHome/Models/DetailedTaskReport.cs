using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaintainHome.Models
{
    public class DetailedTaskReport 
    { 
        public Tasks Task { get; set; } 
        public List<TaskActivity> Activities { get; set; } 
        public List<Notification> Notifications { get; set; } 
        public List<PartInfo> Parts { get; set; } 
        public List<TaskNote> Notes { get; set; } 
        public List<TaskHelp> Helps { get; set; } 
    }
}
