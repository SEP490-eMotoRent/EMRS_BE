using EMRS.Application.Common;
using EMRS.Application.DTOs.AdditionalFeeDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services
{
    public interface IAdditionalFeeService
    {

        Task<ResultResponse<AdditionalFeeResponse>> AddLateReturnFeeAsync(AddLateReturnFeeRequest request);
        Task<ResultResponse<AdditionalFeeResponse>> AddCleaningFeeAsync(AddCleaningFeeRequest request);
        Task<ResultResponse<AdditionalFeeResponse>> AddCrossBranchFeeAsync(AddCrossBranchFeeRequest request);
        Task<ResultResponse<AdditionalFeeResponse>> AddExcessKmFeeAsync(AddExcessKmFeeRequest request);
        Task<ResultResponse<AdditionalFeeResponse>> AddDamageFeeAsync(AddDamageFeeRequest request);
        Task<ResultResponse<GetDamageTypesResponse>> GetDamageTypesAsync();
        Task<ResultResponse<List<AdditionalFeeResponse>>> GetFeesByBookingIdAsync(Guid bookingId);
        Task<ResultResponse<string>> DeleteFeeAsync(Guid feeId);
    }
}
