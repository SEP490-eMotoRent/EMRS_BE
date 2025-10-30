using EMRS.Application.Common;
using EMRS.Application.DTOs.InsurancePackageDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services;

public interface IInsurancePackageService
{
    Task<ResultResponse<InsurancePackageResponse>> CreateInsurancePackage(InsurancePackageCreateRequest request);
    Task<ResultResponse<List<InsurancePackageResponse>>> GetAllInsurancePackages();
    Task<ResultResponse<InsurancePackageResponse>> GetInsurancePackageById(Guid id);
}
