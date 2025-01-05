using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaintainHome.Models
{
    public class TaskReport
    {
        public string Title { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public string Period { get; set; }
        public int FrequencyDays { get; set; }
        public int CategoryId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string DateRange { get; set; }
        public List<Tasks> Tasks { get; set; }
        public DateTime DateTimeStamp { get; set; }
        public bool IsBold { get; set; }
        public ObservableCollection<Activity> Activities { get; set; } = new ObservableCollection<Activity>();
        public ObservableCollection<Notification> Notifications { get; set; } = new ObservableCollection<Notification>();
        public ObservableCollection<PartInfo> Parts { get; set; } = new ObservableCollection<PartInfo>();
        public ObservableCollection<TaskNote> Notes { get; set; } = new ObservableCollection<TaskNote>();
        public ObservableCollection<TaskHelp> Helps { get; set; } = new ObservableCollection<TaskHelp>();
    }




    //public class TaskReport
    //{
    //    public string Title { get; set; }
    //    public string Period { get; set; }
    //    public string DateRange { get; set; } // Add this property
    //    public List<Tasks> Tasks { get; set; }
    //    public DateTime DateTimeStamp { get; set; }
    //}




}
