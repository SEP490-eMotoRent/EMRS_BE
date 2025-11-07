using EMRS.Application.Common;
using EMRS.Application.DTOs.VehicleModelDTOs;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories;

public interface IVehicleModelRepository
{
    void Add(VehicleModel entity);

    Task AddAsync(VehicleModel entity);

    void Delete(VehicleModel entity);


    IEnumerable<VehicleModel> GetAll();

    Task<List<VehicleModel>> GetAllAsync();

    VehicleModel? FindById(Guid id);

    Task<VehicleModel?> FindByIdAsync(Guid id);

    IQueryable<VehicleModel> SearchAvailableModelsQuery(VehicleModelSearchRequest request);
    Task<PaginationResult<List<VehicleModel>>> SearchAvailableModelsPaginationAsync(
         VehicleModelSearchRequest request, int pageSize, int pageNum);
    Task<IEnumerable<VehicleModel>> GetVehicleModelsWithReferencesAsync();
    Task<VehicleModel?> GetVehicleModelWithReferencesByIdAsync(Guid vehicleModelId);
    void Update(VehicleModel entity);


    IQueryable<VehicleModel> Query();
}
