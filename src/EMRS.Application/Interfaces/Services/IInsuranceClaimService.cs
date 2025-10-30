using EMRS.Application.Common;
using EMRS.Application.DTOs.InsuranceClaimDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services
{
    public interface IInsuranceClaimService
    {
        Task<ResultResponse<InsuranceClaimResponse>> CreateInsuranceClaim(CreateInsuranceClaimRequest request);
        Task<ResultResponse<InsuranceClaimDetailResponse>> GetInsuranceClaimDetail(Guid id);
        Task<ResultResponse<List<InsuranceClaimResponse>>> GetMyInsuranceClaims();
    }
}
