using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaintainHome.Models
{
    public class TaskHelp
    {
        [PrimaryKey, AutoIncrement]
        public int TaskHelpsId { get; set; }
        [Indexed]
        public int TaskId { get; set; }  // Foreign key linking to Tasks
        public string? Description { get; set; }
        public string Type { get; set; }    // Type 1 = video, 2 = written instructions. 3 = pictoral instructions
        public string? URL { get; set; }
    }
}
