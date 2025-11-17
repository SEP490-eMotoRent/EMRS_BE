using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RentalContractDTOs
{
    public class PdfGenerateRequest
    {
        public string TemplateBase64 { get; set; }
        public List<string> Parameters { get; set; }
    }
}
