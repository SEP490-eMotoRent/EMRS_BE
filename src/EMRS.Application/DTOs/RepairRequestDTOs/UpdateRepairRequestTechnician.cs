using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.RepairRequestDTOs
{
    public class UpdateRepairRequestTechnician
    {
        public Guid RepairRequestId { get; set; }
        public object? Checklist { get; set; }
    }
}
