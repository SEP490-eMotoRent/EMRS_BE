using EMRS.Application.Common;
using EMRS.Application.DTOs.VehicleDTOs;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories;

public interface IVehicleRepository
{
    void Add(Vehicle entity);

    Task AddAsync(Vehicle entity);

    void Delete(Vehicle entity);


    IEnumerable<Vehicle> GetAll();

    Task<List<Vehicle>> GetAllAsync();

    Vehicle? FindById(Guid id);

    Task<Vehicle?> FindByIdAsync(Guid id);

    IQueryable<Vehicle> Query();

    void Update(Vehicle entity);
    Task<PaginationResult<List<Vehicle>>> GetVehicleListWithReferencesAsync(
        VehicleSearchRequest vehicleSearchRequest, int PageSize, int PageNum);
    Task<Vehicle?> GetVehicleWithReferencesAsync(Guid vehicleId, Guid vehicleModelId);
    Task<bool> IsEmptyAsync();
}
