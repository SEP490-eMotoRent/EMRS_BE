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



    void Update(VehicleModel entity);


    IQueryable<VehicleModel> Query();
}
