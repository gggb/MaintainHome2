using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaintainHome.Models
{
    public class PartInfo
    {
        [PrimaryKey, AutoIncrement]
        public int PartInfoId { get; set; }
        public string? PartName { get; set; }
        public string? PartDescription { get; set; }
        public int TaskId { get; set; }
        public string? ImagePath { get; set; }
    }
}
