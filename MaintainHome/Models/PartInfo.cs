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
        [Indexed] public int TaskId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public double Price { get; set; }
        public string? Source { get; set; }
        
        //public string? ImagePath { get; set; }
    }
}
