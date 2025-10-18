using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class Media : BaseEntity
    {
        public string MediaName { get; set; }
        public string MediaType { get; set; }
        public string FileUrl { get; set; }

        public string Description { get; set; }

        public string DocNo { get; set; }
    }

}
