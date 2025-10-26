using EMRS.Application.Common;
using EMRS.Application.DTOs.RentalReceiptDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services;

public interface IRentalReceiptService
{
    Task<ResultResponse<RentalReceiptResponse>> CreateRentailReceiptAsync(RentalReceiptCreateRequest rentalReceiptCreateRequest);

}
