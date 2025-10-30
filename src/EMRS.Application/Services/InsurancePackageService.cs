using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.InsuranceClaimDTOs;
using EMRS.Application.DTOs.InsurancePackageDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services
{
    public class InsurancePackageService : IInsurancePackageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public InsurancePackageService(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResultResponse<InsurancePackageResponse>> CreateInsurancePackage(
            InsurancePackageCreateRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Validate package name uniqueness
                var existingPackage = await _unitOfWork.GetInsurancePackageRepository()
                    .GetPackageByNameAsync(request.PackageName);

                if (existingPackage != null)
                {
                    return ResultResponse<InsurancePackageResponse>.Failure(
                        "Insurance package with this name already exists");
                }

                // Validate business rules
                if (request.PackageFee <= 0)
                {
                    return ResultResponse<InsurancePackageResponse>.Failure(
                        "Package fee must be greater than zero");
                }

                if (request.CoverageVehiclePercentage < 0 || request.CoverageVehiclePercentage > 100)
                {
                    return ResultResponse<InsurancePackageResponse>.Failure(
                        "Coverage vehicle percentage must be between 0 and 100");
                }

                // Create new insurance package
                var insurancePackage = new InsurancePackage
                {
                    PackageName = request.PackageName,
                    PackageFee = request.PackageFee,
                    CoveragePersonLimit = request.CoveragePersonLimit,
                    CoveragePropertyLimit = request.CoveragePropertyLimit,
                    CoverageVehiclePercentage = request.CoverageVehiclePercentage,
                    CoverageTheft = request.CoverageTheft,
                    DeductibleAmount = request.DeductibleAmount,
                    Description = request.Description,
                    IsActive = request.IsActive
                };

                await _unitOfWork.GetInsurancePackageRepository().AddAsync(insurancePackage);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                var response = _mapper.Map<InsurancePackageResponse>(insurancePackage);

                return ResultResponse<InsurancePackageResponse>.SuccessResult(
                    "Insurance claims retrieved successfully",
                    response);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultResponse<InsurancePackageResponse>.Failure(
                    $"An error occurred while creating insurance package: {ex.Message}");
            }
        }

         public async Task<ResultResponse<List<InsurancePackageResponse>>> GetAllInsurancePackages()
        {
            try
            {
                var packages = await _unitOfWork.GetInsurancePackageRepository().GetAllAsync();

                if (packages == null || packages.Count == 0)
                {
                    return ResultResponse<List<InsurancePackageResponse>>.NotFound(
                        "No insurance packages found");
                }

                var response = _mapper.Map<List<InsurancePackageResponse>>(packages);

                return ResultResponse<List<InsurancePackageResponse>>.SuccessResult(
                    "Insurance claims retrieved successfully",
                    response);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<InsurancePackageResponse>>.Failure(
                    $"An error occurred while retrieving insurance packages: {ex.Message}");
            }
        }

        public async Task<ResultResponse<InsurancePackageResponse>> GetInsurancePackageById(Guid id)
        {
            try
            {
                var package = await _unitOfWork.GetInsurancePackageRepository().FindByIdAsync(id);

                if (package == null)
                {
                    return ResultResponse<InsurancePackageResponse>.NotFound(
                        "Insurance package not found");
                }

                var response = _mapper.Map<InsurancePackageResponse>(package);

                return ResultResponse<InsurancePackageResponse>.SuccessResult(
                    "Insurance package retrieved successfully", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<InsurancePackageResponse>.Failure(
                    $"An error occurred while retrieving insurance package: {ex.Message}");
            }
        }
    }

}
