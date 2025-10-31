using EMRS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.ConfigurationDTOs
{
    public class ConfigurationCreateRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public ConfigurationTypeEnum Type { get; set; }
        public string Value { get; set; }
    }
}
