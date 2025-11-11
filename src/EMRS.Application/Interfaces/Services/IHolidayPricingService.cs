using EMRS.Application.Common;
using EMRS.Application.DTOs.HolidayPricingDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services
{
    public interface IHolidayPricingService
    {
        Task<ResultResponse<List<HolidayPricingResponse>>> GetAllAsync();
        Task<ResultResponse<HolidayPricingResponse>> GetByIdAsync(Guid id);
        Task<ResultResponse<HolidayPricingResponse>> CreateAsync(HolidayPricingCreateRequest request);
        Task<ResultResponse<HolidayPricingResponse>> UpdateAsync(HolidayPricingUpdateRequest request);
        Task<ResultResponse<bool>> DeleteAsync(Guid id);
    }
}
