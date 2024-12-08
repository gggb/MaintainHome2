using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaintainHome.Models
{
    public class PartBuy
    {
        [PrimaryKey, AutoIncrement]
        public int PartBuyId { get; set; }
        [Indexed] public int PartInfoId { get; set; }
        public string? SourceName { get; set; }
        public string? SourceURL { get; set; }
        public double Price { get; set; }
        public bool Availability { get; set; }
    }
}

