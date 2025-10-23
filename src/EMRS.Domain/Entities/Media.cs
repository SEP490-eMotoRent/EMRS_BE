using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class Media : BaseEntity
    {
       
        public string MediaType { get; set; }
        public string FileUrl { get; set; }

       

        public Guid DocNo { get; set; }

        public string EntityType { get; set; }
    }

}
