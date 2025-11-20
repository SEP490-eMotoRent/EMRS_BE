using EMRS.Application.Common;
using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories;

public interface IBranchRepository
{
    void Add(Branch entity);

    Task AddAsync(Branch entity);
    Task<Branch?> GetBranchByStaffIdAsync(Guid staffId);
    void Delete(Branch entity);
    Task<List<Branch>> GetBranchesInBoundingBoxAsync(double lat, double lon, double latRange, double lonRange);
    Task<List<Branch>> GetBranchByVehicleModelIdAsync(Guid vehicleModelId);
    IEnumerable<Branch> GetAll();

    Task<List<Branch>> GetAllAsync();

    Branch? FindById(Guid id);

    Task<Branch?> FindByIdAsync(Guid id);

    Task<List<Branch>> SearchBranchWithAvailableModelsAsync(
    BranchSearchRequest request);

    void Update(Branch entity);


    IQueryable<Branch> Query();
}
