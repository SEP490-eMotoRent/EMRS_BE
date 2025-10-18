using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories;

public interface IRentalPricingRepository
{
    void Add(RentalPricing entity);

    Task AddAsync(RentalPricing entity);

    void Delete(RentalPricing entity);


    IEnumerable<RentalPricing> GetAll();

    Task<List<RentalPricing>> GetAllAsync();

    RentalPricing? FindById(Guid id);

    Task<RentalPricing?> FindByIdAsync(Guid id);



    void Update(RentalPricing entity);


    IQueryable<RentalPricing> Query();
}
