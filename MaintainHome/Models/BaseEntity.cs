using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaintainHome.Models
{
    public abstract class BaseEntity
    {
        public virtual int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public abstract Task<bool> Add();
        public abstract Task<bool> Get(int id);
        public abstract Task<bool> Update();
        public abstract Task<bool> Delete(int id);
    }
}
