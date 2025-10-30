using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories
{
    public interface IInsurancePackageRepository
    {
        // CRUD methods (from GenericRepository pattern)
        void Add(InsurancePackage entity);
        Task AddAsync(InsurancePackage entity);
        void Delete(InsurancePackage entity);
        IEnumerable<InsurancePackage> GetAll();
        Task<List<InsurancePackage>> GetAllAsync();
        InsurancePackage? FindById(Guid id);
        Task<InsurancePackage?> FindByIdAsync(Guid id);
        void Update(InsurancePackage entity);
        IQueryable<InsurancePackage> Query();
        Task<bool> IsEmptyAsync();

        // Custom methods
        Task<List<InsurancePackage>> GetAllActivePackagesAsync();
        Task<InsurancePackage?> GetPackageByNameAsync(string packageName);
    }
}
