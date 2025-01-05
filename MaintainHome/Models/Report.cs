using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaintainHome.Models
{
    public class Report
    {
        public string Title { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; }
        public DateTime DateTimeStamp { get; set; }
    }

}
