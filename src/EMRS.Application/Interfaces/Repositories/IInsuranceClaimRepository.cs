using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Repositories
{
    public interface IInsuranceClaimRepository
    {
        void Add(InsuranceClaim entity);
        Task AddAsync(InsuranceClaim entity);
        void Delete(InsuranceClaim entity);
        IEnumerable<InsuranceClaim> GetAll();
        Task<List<InsuranceClaim>> GetAllAsync();
        InsuranceClaim? FindById(Guid id);
        Task<InsuranceClaim?> FindByIdAsync(Guid id);
        void Update(InsuranceClaim entity);
        IQueryable<InsuranceClaim> Query();
        Task<bool> IsEmptyAsync();

        // Custom methods
        Task<InsuranceClaim?> GetInsuranceClaimWithDetailsAsync(Guid id);
        Task<InsuranceClaim?> GetInsuranceClaimForManagerAsync(Guid id);
        Task<List<InsuranceClaim>> GetInsuranceClaimsByRenterIdAsync(Guid renterId);
        Task<List<InsuranceClaim>> GetInsuranceClaimsByBranchIdAsync(Guid branchId);
    }
}
