using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities
{
    public partial class Configuration:BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
    }
}
