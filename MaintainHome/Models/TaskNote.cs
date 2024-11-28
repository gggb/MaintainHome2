using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaintainHome.Models
{
    public class TaskNote
    {
        [PrimaryKey, AutoIncrement]
        public int NoteId { get; set; }
        public int TaskId { get; set; }
        public string? Type { get; set; }
        public string? NoteContent { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
