using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.AdditionalFeeDTOs
{
    public class AddLateReturnFeeRequest
    {
        public Guid BookingId { get; set; }
    }
}
